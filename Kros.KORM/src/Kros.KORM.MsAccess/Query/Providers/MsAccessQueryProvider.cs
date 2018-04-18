using Kros.Data.BulkActions;
using Kros.Data.BulkActions.MsAccess;
using Kros.Data.Schema;
using Kros.Data.Schema.MsAccess;
using Kros.KORM.Helper;
using Kros.KORM.Materializer;
using Kros.KORM.Query.Sql;
using System.Configuration;
using System.Data.Common;
using System.Data.OleDb;

namespace Kros.KORM.Query.MsAccess
{
    /// <summary>
    /// Provider, which know execute query for MsAccess.
    /// </summary>
    /// <seealso cref="Kros.KORM.Query.QueryProvider" />
    public class MsAccessQueryProvider : QueryProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessQueryProvider"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string settings.</param>
        /// <param name="sqlGenerator">The SQL generator.</param>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="logger">The logger.</param>
        public MsAccessQueryProvider(ConnectionStringSettings connectionString,
           ISqlExpressionVisitor sqlGenerator,
           IModelBuilder modelBuilder,
           ILogger logger)
            : base(connectionString, sqlGenerator, modelBuilder, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessQueryProvider" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="sqlGenerator">The SQL generator.</param>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="logger">The logger.</param>
        public MsAccessQueryProvider(DbConnection connection,
            ISqlExpressionVisitor sqlGenerator,
            IModelBuilder modelBuilder,
            ILogger logger)
                : base(connection, sqlGenerator, modelBuilder, logger)
        {
        }

        /// <summary>
        /// Vráti <see cref="OleDbFactory.Instance">OleDbFactory.Instance</see>.
        /// </summary>
        public override DbProviderFactory DbProviderFactory => OleDbFactory.Instance;

        /// <summary>
        /// Creates instance of <see cref="MsAccessBulkInsert" />.
        /// </summary>
        /// <returns>
        /// Instance of <see cref="MsAccessBulkInsert" />.
        /// </returns>
        public override IBulkInsert CreateBulkInsert()
        {
            var transaction = GetCurrentTransaction();
            if (IsExternalConnection || transaction != null)
            {
                return new MsAccessBulkInsert(Connection as OleDbConnection, transaction as OleDbTransaction);
            }
            else
            {
                return new MsAccessBulkInsert(ConnectionString);
            }
        }

        /// <summary>
        /// Creates instance of <see cref="MsAccessBulkUpdate" />.
        /// </summary>
        /// <returns>
        /// Instance of <see cref="MsAccessBulkUpdate" />.
        /// </returns>
        public override IBulkUpdate CreateBulkUpdate()
        {
            var transaction = GetCurrentTransaction();

            if (IsExternalConnection || transaction != null)
            {
                return new MsAccessBulkUpdate(Connection as OleDbConnection, transaction as OleDbTransaction);
            }
            else
            {
                return new MsAccessBulkUpdate(ConnectionString);
            }
        }

        /// <summary>
        /// Returns instance of <see cref="MsAccessSchemaLoader"/>.
        /// </summary>
        protected override IDatabaseSchemaLoader GetSchemaLoader()
        {
            return new MsAccessSchemaLoader();
        }
    }
}
