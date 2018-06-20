using System.Data;

namespace Kros.KORM.Query.Sql
{
    public interface ISqlExpressionVisitorFactory
    {
        ISqlExpressionVisitor CreateVisitor(IDbConnection connection);
    }
}
