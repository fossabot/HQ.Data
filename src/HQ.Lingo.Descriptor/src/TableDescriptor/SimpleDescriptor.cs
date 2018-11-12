#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using HQ.Common.FastMember;
using HQ.Lingo.Descriptor.TableDescriptor.Extensions;

namespace HQ.Lingo.Descriptor.TableDescriptor
{
    /// <summary>
    ///     A conventions-based descriptor that maps classes to database objects assuming that the column names and types
    ///     in the database match the property names, that identity fields are the singular fields that end with ID; every
    ///     other key is assigned. All other opt-ins (transient and computed markers) must use the
    ///     <see cref="TransientAttribute" />
    ///     and <see cref="ComputedAttribute" />, respectively.
    ///     <remarks>
    ///         Id -> PK Identity
    ///         OneId, TwoId -> Keys
    ///         CreatedAt -> Computed (assumes timestamp field)
    ///     </remarks>
    /// </summary>
    public class SimpleDescriptor : Descriptor
    {
        private static readonly Hashtable Descriptors = new Hashtable();

        public SimpleDescriptor(Type type)
        {
            Type = type;
            Table = type.Name;
            All = new List<PropertyToColumn>();
            Describe();
            BuildCachedProperties();
        }

        public Type Type { get; private set; }

        public PropertyToColumn this[int index]
        {
            get { return All[index]; }
            set { All[index] = value; }
        }

        public string Schema { get; set; }
        public string Table { get; private set; }
        public PropertyToColumn Identity { get; private set; }
        public IList<PropertyToColumn> All { get; private set; }
        public IList<PropertyToColumn> Insertable { get; private set; }
        public IList<PropertyToColumn> Computed { get; private set; }
        public IList<PropertyToColumn> Keys { get; private set; }
        public IList<PropertyToColumn> Assigned { get; private set; }

        public void Describe()
        {
            All.Clear();
            var ids = new List<PropertyToColumn>();
            var properties = ProjectFromComponentModel().ToList();

            foreach (var property in properties)
            {
                if (Exists(property) || property.Has<TransientAttribute>()) continue;

                var column = new PropertyToColumn(property)
                {
                    IsComputed = property.Has<ComputedAttribute>()
                };

                All.Add(column);

                if (column.Property.Name.EndsWith("id", true, CultureInfo.InvariantCulture))
                    AssignKey(property, column, ids);

                if (column.Property.Name.Equals("createdat", StringComparison.OrdinalIgnoreCase))
                    column.IsComputed = true;
            }

            MakeCompositeKeyIfMultiplePossibleIdentities(ids);

            foreach (var property in properties)
                if (property.Has<IdentityAttribute>())
                {
                    var column = All.SingleOrDefault(a => a.ColumnName.Equals(property.Name));
                    if (column != null)
                    {
                        column.IsIdentity = true;
                        column.IsComputed = true;
                    }
                }
        }

        public static SimpleDescriptor Create<T>()
        {
            return Create(typeof(T));
        }

        public static SimpleDescriptor Create(Type type)
        {
            var obj = (SimpleDescriptor) Descriptors[type];
            if (obj != null) return obj;
            lock (Descriptors)
            {
                obj = (SimpleDescriptor) Descriptors[type];
                if (obj != null) return obj;

                obj = new SimpleDescriptor(type);
                Descriptors[type] = obj;
                return obj;
            }
        }

        private void BuildCachedProperties()
        {
            Identity = All.SingleOrDefault(p => p.IsIdentity);
            Insertable = All.Where(p => !p.IsComputed && !p.IsIdentity).ToList();
            Computed = All.Where(p => p.IsComputed).ToList();
            Keys = All.Where(p => p.IsKey).ToList();
            Assigned = All.Where(p => !p.IsComputed).ToList();
        }

        private static void AssignKey(PropertyAccessor property, PropertyToColumn column,
            ICollection<PropertyToColumn> ids)
        {
            column.IsKey = true;
            if (property.IsPrimitiveInteger())
            {
                column.IsIdentity = true;
                column.IsComputed = true;
                ids.Add(column);
            }
        }

        private static void MakeCompositeKeyIfMultiplePossibleIdentities(ICollection<PropertyToColumn> ids)
        {
            if (ids.Count <= 1) return;
            foreach (var id in ids)
            {
                id.IsKey = true;
                id.IsIdentity = false;
                id.IsComputed = false;
            }
        }

        private IEnumerable<PropertyAccessor> ProjectFromComponentModel()
        {
            var accessor = TypeAccessor.Create(Type);
            var properties = TypeDescriptor.GetProperties(Type);
            var descriptors = properties.Cast<PropertyDescriptor>();
            var accessors = descriptors.Select(property =>
                new PropertyAccessor(accessor, property.PropertyType, property.Name));
            return accessors;
        }

        private bool Exists(PropertyAccessor accessor)
        {
            return All.Any(p => p.Property.Name.Equals(accessor.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
