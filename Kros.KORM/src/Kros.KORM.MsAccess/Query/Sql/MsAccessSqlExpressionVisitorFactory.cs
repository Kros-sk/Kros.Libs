using Kros.KORM.Metadata;
using Kros.KORM.Query.Sql;
using Kros.Utils;
using System.Data;

namespace Kros.KORM.MsAccess.Query.Sql
{
    public class MsAccessSqlExpressionVisitorFactory : ISqlExpressionVisitorFactory
    {
        IDatabaseMapper _databaseMapper;

        public MsAccessSqlExpressionVisitorFactory(IDatabaseMapper databaseMapper)
        {
            _databaseMapper = Check.NotNull(databaseMapper, nameof(databaseMapper));
        }

        public ISqlExpressionVisitor CreateVisitor(IDbConnection connection)
        {
            return new DefaultQuerySqlGenerator(_databaseMapper);
        }
    }
}
