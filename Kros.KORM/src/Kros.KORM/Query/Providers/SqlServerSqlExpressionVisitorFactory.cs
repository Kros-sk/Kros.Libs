using Kros.KORM.Metadata;
using Kros.KORM.Query.Sql;
using Kros.Utils;
using System.Data;

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
            return new DefaultQuerySqlGenerator(_databaseMapper);
        }
    }
}
