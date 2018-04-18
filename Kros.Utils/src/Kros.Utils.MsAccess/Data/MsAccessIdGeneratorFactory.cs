using Kros.Utils;
using System.Data.OleDb;

namespace Kros.Data.MsAccess
{
    /// <summary>
    /// Interface popisujúci factory triedu, ktorá vie vytvoriť inštanciu <see cref="MsAccessIdGenerator"/>.
    /// </summary>
    /// <seealso cref="MsAccessIdGenerator"/>
    /// <seealso cref="IdGeneratorFactories"/>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\IdGeneratorExamples.cs" region="IdGeneratorFactory"/>
    /// </example>
    /// <remarks>Štandardne sa nevytvára priamo ale cez <see cref="IdGeneratorFactories"/>.</remarks>
    public class MsAccessIdGeneratorFactory
        : IIdGeneratorFactory
    {

        private readonly string _connectionString;
        private readonly OleDbConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessIdGeneratorFactory"/> class.
        /// </summary>
        /// <param name="connection">Connection, ktorá sa použije pre získavanie unikátnych identifikátorov.</param>
        public MsAccessIdGeneratorFactory(OleDbConnection connection)
        {
            _connection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsAccessIdGeneratorFactory"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, ktorý sa použije na vytvorenie conenction pre získavanie unikátnych identifikátorov.
        /// </param>
        public MsAccessIdGeneratorFactory(string connectionString)
        {
            _connectionString = Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
        }

        /// <inheritdoc/>
        public IIdGenerator GetGenerator(string tableName) =>
            GetGenerator(tableName, 1);

        /// <inheritdoc/>
        public IIdGenerator GetGenerator(string tableName, int batchSize) =>
            _connection != null ?
                new MsAccessIdGenerator(_connection, tableName, batchSize) :
                new MsAccessIdGenerator(_connectionString, tableName, batchSize);

        /// <summary>
        /// Registrovanie factory metód na vytvorenie inštancie do <see cref="IdGeneratorFactories"/>.
        /// </summary>
        public static void Register() =>
            IdGeneratorFactories.Register<OleDbConnection>("System.Data.OleDb",
                (conn) => new MsAccessIdGeneratorFactory(conn as OleDbConnection),
                (connString) => new MsAccessIdGeneratorFactory(connString));
    }
}