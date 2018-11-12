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

using System.Collections.Generic;
using System.Diagnostics;
using HQ.Common.Helpers;
using HQ.Lingo.Builders.Extensions;
using HQ.Lingo.Descriptor;
using HQ.Lingo.Dialects;

namespace HQ.Lingo.Builders
{
    public static class UpdateBuilder
    {
        public const string SetSuffix = "_set";

        public static string Update(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<string> keys, string setSuffix = SetSuffix)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("UPDATE ");
                sb.AppendTable(d, table, schema).Append(" SET ");
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column).Append(" = ").AppendParameter(d, column).Append(setSuffix);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.AppendWhereClause(d, keys);
            });
        }

        public static string Update(this ISqlDialect d, string table, string schema, IList<string> columns,
            IList<string> keys, IList<string> setParameters, IList<string> whereParameters,
            string setSuffix = SetSuffix)
        {
            Debug.Assert(columns != null);
            Debug.Assert(columns.Count == setParameters?.Count && columns.Count >= whereParameters?.Count);

            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("UPDATE ");
                sb.AppendTable(d, table, schema).Append(" SET ");
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column).Append(" = ").AppendParameter(d, setParameters[i]).Append(setSuffix);
                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.AppendWhereClause(d, keys, whereParameters);
            });
        }

        public static string Update(this ISqlDialect d, string table, string schema, IList<PropertyToColumn> columns,
            IList<string> keys, string setSuffix = SetSuffix)
        {
            return StringBuilderPool.Scoped(sb =>
            {
                sb.Append("UPDATE ");
                sb.AppendTable(d, table, schema).Append(" SET ");
                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    sb.AppendName(d, column.ColumnName).Append(" = ")
                        .AppendParameter(d, column.ColumnName)
                        .Append(setSuffix);

                    if (i < columns.Count - 1)
                        sb.Append(", ");
                }

                sb.AppendWhereClause(d, keys);
            });
        }
    }
}
