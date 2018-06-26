using Kros.KORM.Metadata;
using Kros.KORM.Query.Sql;
using Kros.KORM.Query.Sql.MsAccess;
using Kros.Utils;
using System.Data;

namespace Kros.KORM.MsAccess.Query.Sql
{
    /// <summary>
    /// <inheritdoc cref="ISqlExpressionVisitorFactory"/>
    /// </summary>
    public class MsAccessSqlExpressionVisitorFactory : ISqlExpressionVisitorFactory
    {
        private readonly IDatabaseMapper _databaseMapper;

        /// <summary>
        /// Creates an instance with specified database mapper <paramref name="databaseMapper"/>.
        /// </summary>
        /// <param name="databaseMapper">Database mapper.</param>
        public MsAccessSqlExpressionVisitorFactory(IDatabaseMapper databaseMapper)
            => _databaseMapper = Check.NotNull(databaseMapper, nameof(databaseMapper));

        /// <summary>
        /// Returns <see cref="MsAccessQuerySqlGenerator"/>.
        /// </summary>
        /// <param name="connection">Database connection.</param>
        /// <returns>Instance of <see cref="MsAccessQuerySqlGenerator"/>.</returns>
        public ISqlExpressionVisitor CreateVisitor(IDbConnection connection)
            => new MsAccessQuerySqlGenerator(_databaseMapper);
    }
}
