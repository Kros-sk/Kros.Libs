using Kros.KORM.Metadata;
using System.Linq.Expressions;

namespace Kros.KORM.Query.Sql
{
    /// <summary>
    /// Generator sql query for SQL server.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.Sql.DefaultQuerySqlGenerator" />
    public class SqlServerQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="databaseMapper">Database mapper.</param>
        public SqlServerQuerySqlGenerator(IDatabaseMapper databaseMapper)
            : base(databaseMapper)
        {
        }

        /// <summary>
        /// Generates the SQL from expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// SQL select command text.
        /// </returns>
        public override string GenerateSql(Expression expression)
        {
            return base.GenerateSql(expression);
        }
    }
}
