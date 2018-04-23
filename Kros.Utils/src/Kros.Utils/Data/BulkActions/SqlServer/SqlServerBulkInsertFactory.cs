using Kros.Utils;
using System.Data.SqlClient;

namespace Kros.Data.BulkActions.SqlServer
{
    /// <summary>
    /// Creates instances of <see cref="IBulkInsert"/> for bulk inserting.
    /// </summary>
    /// <seealso cref="Kros.Data.BulkActions.IBulkInsertFactory" />
    public class SqlServerBulkInsertFactory : IBulkInsertFactory
    {

        private readonly SqlConnection _connection;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerBulkInsertFactory"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public SqlServerBulkInsertFactory(SqlConnection connection)
        {
            _connection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerBulkInsertFactory"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlServerBulkInsertFactory(string connectionString)
        {
            _connectionString = Check.NotNullOrEmpty(connectionString, nameof(connectionString));
        }

        /// <summary>
        /// Creates a new <see cref="T:Kros.Data.BulkActions.IBulkInsert"/> class.
        /// </summary>
        /// <returns>The Bulk Insert.</returns>
        public IBulkInsert GetBulkInsert() =>
            _connection != null ? new SqlServerBulkInsert(_connection) : new SqlServerBulkInsert(_connectionString);


        /// <summary>
        /// Registers factory methods for creation instances to <see cref="BulkInsertFactories"/>.
        /// </summary>
        public static void Register() =>
            BulkInsertFactories.Register<SqlConnection>("System.Data.SqlClient",
                (conn) => new SqlServerBulkInsertFactory(conn as SqlConnection),
                (connString) => new SqlServerBulkInsertFactory(connString));
    }
}
