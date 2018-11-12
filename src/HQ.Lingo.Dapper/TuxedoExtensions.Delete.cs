﻿#region LICENSE

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
using System.Data;
using System.Linq.Expressions;
using Dapper;

namespace HQ.Lingo.Dapper
{
    partial class TuxedoExtensions
    {
        public static int DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null,
            int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            var query = Tuxedo.DeleteAll<T>();
            return connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
        }

        public static int Delete<T>(this IDbConnection connection, dynamic where, IDbTransaction transaction = null,
            int? commandTimeout = null, params Expression<Func<T, object>>[] sortOn) where T : class
        {
            Query query = Tuxedo.Delete<T>(where);
            return connection.Execute(query.Sql, Prepare(query.Parameters), transaction, commandTimeout);
        }
    }
}
