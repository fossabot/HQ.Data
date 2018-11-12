using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using fastmember;
using table_descriptor.Extensions;

namespace table_descriptor
{
    /// <summary>
    /// A conventions-based descriptor that maps classes to database objects assuming that the column names and types
    /// in the database match the property names, that identity fields are the singular fields that end with ID; every
    /// other key is assigned. All other opt-ins (transient and computed markers) must use the <see cref="TransientAttribute"/>
    /// and <see cref="ComputedAttribute"/>, respectively.
    /// <remarks>
    /// Id -> PK Identity
    /// OneId, TwoId -> Keys
    /// CreatedAt -> Computed (assumes timestamp field)
    /// </remarks>
    /// </summary>
    public class SimpleDescriptor : IDescriptor
    {
        private static readonly Hashtable Descriptors = new Hashtable();

        public static SimpleDescriptor Create<T>()
        {
            return Create(typeof(T));
        }

        public static SimpleDescriptor Create(Type type)
        {
            var obj = (SimpleDescriptor)Descriptors[type];
            if (obj != null) return obj;
            lock (Descriptors)
            {
                obj = (SimpleDescriptor)Descriptors[type];
                if (obj != null) return obj;

                obj = new SimpleDescriptor(type);
                Descriptors[type] = obj;
                return obj;
            }
        }

        public string Schema { get; set; }
        public string Table { get; private set; }
        public PropertyToColumn Identity { get; private set; }
        public PropertyToColumn SearchKey { get; private set; }
        public IList<PropertyToColumn> All { get; private set; }
        public IList<PropertyToColumn> Insertable { get; private set; }
        public IList<PropertyToColumn> Computed { get; private set; }
        public IList<PropertyToColumn> Keys { get; private set; }
        public IList<PropertyToColumn> Assigned { get; private set; }
        public Type Type { get; private set; }

        public PropertyToColumn this[int index]
        {
            get { return All[index]; }
            set { All[index] = value; }
        }

        protected SimpleDescriptor(Type type)
        {
            Type = type;

            TableNameAttribute tableNameAttribute = type.GetTypeInfo().GetCustomAttribute<TableNameAttribute>();
            Table = tableNameAttribute != null ? tableNameAttribute.Name : type.Name;

            All = new List<PropertyToColumn>();
            Describe();
            BuildCachedProperties();
        }

        private void BuildCachedProperties()
        {
            Identity = All.SingleOrDefault(p => p.IsIdentity);
            SearchKey = All.SingleOrDefault(p => p.IsSearchKey);
            Insertable = All.Where(p => !p.IsComputed && !p.IsIdentity).ToList();
            Computed = All.Where(p => p.IsComputed).ToList();
            Keys = All.Where(p => p.IsKey).ToList();
            Assigned = All.Where(p => !p.IsComputed).ToList();
        }

        public void Describe()
        {
            All.Clear();

            var ids = new List<PropertyToColumn>();
            var properties = ProjectFromTypeAccessor().ToList();
            var columnMappings = new List<PropertyToColumn>();

            foreach (var property in properties)
            {
                if (Exists(property) || property.Has<TransientAttribute>())
                {
                    continue;
                }

                var column = new PropertyToColumn(property)
                {
                    IsComputed = property.Has<ComputedAttribute>(),
                    IsSearchKey = property.Has<SearchKeyAttribute>()
                };

                if (property.Has<ColumnNameAttribute>())
                {
                    column.ColumnName = property.Get<ColumnNameAttribute>().Name;
                    columnMappings.Add(column);
                }

                All.Add(column);

                // If a class property ends with ID, assume it to be a Identity Key column unless user opts out w/ Key attribute
                if (column.Property.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    var hasKey = property.Has<KeyAttribute>();
                    if (!hasKey || property.Get<KeyAttribute>().Identity)
                    {
                        AssignKey(property, column, ids);
                    }
                }

                // If a class property is CreatedAt, assume it to be a database-side timestamp field
                if (column.Property.Name.Equals("createdat", StringComparison.OrdinalIgnoreCase))
                {
                    column.IsComputed = true;
                }
            }

            MakeCompositeKeyIfMultiplePossibleIdentities(ids);

            foreach (var property in properties)
            {
                if (!property.Has<IdentityAttribute>()) continue;
                var column = All.SingleOrDefault(a => a.ColumnName.Equals(property.Name));
                if (column == null) continue;

                column.IsIdentity = true;
                column.IsComputed = true;
            }

        }

        public virtual void OnDescribed() { }

        private static void AssignKey(PropertyAccessor property, PropertyToColumn column, ICollection<PropertyToColumn> ids)
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

        private IEnumerable<PropertyAccessor> ProjectFromTypeAccessor()
        {
            var accessor = TypeAccessor.Create(Type);
            var properties = accessor.PropertyInfos;
            var accessors = properties.Select(property => new PropertyAccessor(accessor, property.PropertyType, property.Name));
            return accessors;
        }

        private bool Exists(PropertyAccessor accessor)
        {
            return All.Any(p => p.Property.Name.Equals(accessor.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}