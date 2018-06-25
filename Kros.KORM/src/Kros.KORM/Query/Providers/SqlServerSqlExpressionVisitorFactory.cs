using Kros.Data.SqlServer;
using Kros.KORM.Metadata;
using Kros.KORM.Query.Sql;
using Kros.Utils;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Kros.KORM.Query.Providers
{
    public class SqlServerSqlExpressionVisitorFactory : ISqlExpressionVisitorFactory
    {
        IDatabaseMapper _databaseMapper;

        public SqlServerSqlExpressionVisitorFactory(IDatabaseMapper databaseMapper)
        {
            _databaseMapper = Check.NotNull(databaseMapper, nameof(databaseMapper));
        }

        public ISqlExpressionVisitor CreateVisitor(IDbConnection connection)
        {
            Version sqlServerVersion = (connection as SqlConnection).GetVersion();
            if (sqlServerVersion >= SqlServerVersions.Server2012)
            {
                return new SqlServer2012SqlGenerator(_databaseMapper);
            }
            else if (sqlServerVersion >= SqlServerVersions.Server2008)
            {
                return new SqlServer2008SqlGenerator(_databaseMapper);
            }
            return new DefaultQuerySqlGenerator(_databaseMapper);
        }
    }
}
