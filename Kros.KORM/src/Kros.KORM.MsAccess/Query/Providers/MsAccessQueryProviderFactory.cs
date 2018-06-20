using Kros.Data.MsAccess;
using Kros.KORM.Helper;
using Kros.KORM.Materializer;
using Kros.KORM.Metadata;
using Kros.KORM.MsAccess.Query.Sql;
using System.Configuration;
using System.Data.Common;
using System.Data.OleDb;

namespace Kros.KORM.Query.MsAccess
{
    /// <summary>
    /// Factory, which know create MsAccess query provider.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.IQueryProviderFactory" />
    public class MsAccessQueryProviderFactory : IQueryProviderFactory
    {
        /// <summary>
        /// Creates the specified MsAccess QueryProvider factory.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="databaseMapper">Database mapper.</param>
        /// <returns>
        /// Instance of <see cref="MsAccessQueryProvider"/>.
        /// </returns>
        public IQueryProvider Create(DbConnection connection, IModelBuilder modelBuilder, IDatabaseMapper databaseMapper)
            => new MsAccessQueryProvider(
                connection, new MsAccessSqlExpressionVisitorFactory(databaseMapper), modelBuilder, new Logger());

        /// <summary>
        /// Creates the specified MsAccess QueryProvider factory.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="databaseMapper">Database mapper.</param>
        /// <returns>
        /// Instance of <see cref="MsAccessQueryProvider"/>.
        /// </returns>
        public IQueryProvider Create(
            ConnectionStringSettings connectionString,
            IModelBuilder modelBuilder,
            IDatabaseMapper databaseMapper)
            => new MsAccessQueryProvider(
                connectionString, new MsAccessSqlExpressionVisitorFactory(databaseMapper), modelBuilder, new Logger());

        /// <summary>
        /// Registers instance of this type to <see cref="QueryProviderFactories"/>.
        /// </summary>
        public static void Register()
        {
            QueryProviderFactories.Register<OleDbConnection>(MsAccessDataHelper.ClientId, new MsAccessQueryProviderFactory());
        }
    }
}
