using Kros.Utils;
using System.Data.OleDb;

namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// Creates instances of <see cref="IBulkInsert"/> for bulk inserting.
    /// </summary>
    /// <seealso cref="Kros.Data.BulkActions.IBulkInsertFactory" />
    public class MsAccessBulkInsertFactory : IBulkInsertFactory
    {

        private readonly OleDbConnection _connection;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessBulkInsertFactory"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public MsAccessBulkInsertFactory(OleDbConnection connection)
        {
            _connection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessBulkInsertFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public MsAccessBulkInsertFactory(string connectionString)
        {
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));
        }

        /// <summary>
        /// Creates a new <see cref="T:Kros.Data.BulkActions.IBulkInsert" /> class.
        /// </summary>
        /// <returns>The Bulk Insert.</returns>
        public IBulkInsert GetBulkInsert() =>
            _connection != null ? new MsAccessBulkInsert(_connection) : new MsAccessBulkInsert(_connectionString);

        /// <summary>
        /// Registers factory methods for creation instances to <see cref="BulkInsertFactories"/>.
        /// </summary>
        public static void Register() =>
            BulkInsertFactories.Register<OleDbConnection>("System.Data.OleDb",
                (conn) => new MsAccessBulkInsertFactory(conn as OleDbConnection),
                (connString) => new MsAccessBulkInsertFactory(connString));
    }
}
