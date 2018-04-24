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
        /// Gets the bulk insert.
        /// </summary>
        /// <returns>The bulk insert.</returns>
        public IBulkInsert GetBulkInsert() =>
            _connection != null ? new MsAccessBulkInsert(_connection) : new MsAccessBulkInsert(_connectionString);

        /// <summary>
        /// Gets the bulk insert.
        /// </summary>
        /// <param name="externalTransaction">The external transaction.</param>
        /// <returns>The bulk insert.</returns>
        public IBulkInsert GetBulkInsert(OleDbTransaction externalTransaction) =>
            new MsAccessBulkInsert(_connection, externalTransaction);

        /// <summary>
        /// Gets the bulk insert.
        /// </summary>
        /// <param name="externalTransaction">The external transaction.</param>
        /// <param name="csvFileCodePage">The CSV file code page.</param>
        /// <returns>The bulk insert.</returns>
        public IBulkInsert GetBulkInsert(OleDbTransaction externalTransaction, int csvFileCodePage) =>
            new MsAccessBulkInsert(_connection, externalTransaction, csvFileCodePage);

        /// <summary>
        /// Gets the bulk insert.
        /// </summary>
        /// <param name="externalTransaction">The external transaction.</param>
        /// <param name="csvFileCodePage">The CSV file code page.</param>
        /// <param name="valueDelimiter">The value delimiter.</param>
        /// <returns>The bulk insert.</returns>
        public IBulkInsert GetBulkInsert(OleDbTransaction externalTransaction, int csvFileCodePage, char valueDelimiter) =>
            new MsAccessBulkInsert(_connection, externalTransaction, csvFileCodePage, valueDelimiter);

        /// <summary>
        /// Registers factory methods for creation instances to <see cref="BulkInsertFactories"/>.
        /// </summary>
        public static void Register() =>
            BulkInsertFactories.Register<OleDbConnection>("System.Data.OleDb",
                (conn) => new MsAccessBulkInsertFactory(conn as OleDbConnection),
                (connString) => new MsAccessBulkInsertFactory(connString));
    }
}
