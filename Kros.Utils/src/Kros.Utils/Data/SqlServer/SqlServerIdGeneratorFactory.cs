using Kros.Utils;
using System.Data.SqlClient;

namespace Kros.Data.SqlServer
{
    /// <summary>
    /// Interface popisujúci factory triedu, ktorá vie vytvoriť inštanciu <see cref="SqlServerIdGenerator"/>.
    /// </summary>
    /// <seealso cref="SqlServerIdGenerator"/>
    /// <seealso cref="IdGeneratorFactories"/>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\IdGeneratorExamples.cs" region="IdGeneratorFactory"/>
    /// </example>
    /// <remarks>Štandardne sa nevytvára priamo ale cez <see cref="IdGeneratorFactories"/>.</remarks>
    public class SqlServerIdGeneratorFactory
        : IIdGeneratorFactory
    {
        /// <summary>
        /// Defaultný názov tabuľky, kde si uchovávame identifikátory pre jednotlivé tabuľky.
        /// </summary>
        public const string DefaultIdStoreTableName = "IdStore";

        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerIdGeneratorFactory"/> class.
        /// </summary>
        /// <param name="connection">Connection, ktorá sa použije pre získavanie unikátnych identifikátorov.</param>
        public SqlServerIdGeneratorFactory(SqlConnection connection)
        {
            _connection = Check.NotNull(connection, nameof(connection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerIdGeneratorFactory"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string, ktorý sa použije na vytvorenie conenction pre získavanie unikátnych identifikátorov.</param>
        public SqlServerIdGeneratorFactory(string connectionString)
        {
            _connectionString = Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
        }

        /// <inheritdoc/>
        public IIdGenerator GetGenerator(string tableName) =>
            GetGenerator(tableName, 1);

        /// <inheritdoc/>
        public IIdGenerator GetGenerator(string tableName, int batchSize) =>
            _connection != null ?
                new SqlServerIdGenerator(_connection, tableName, batchSize) :
                new SqlServerIdGenerator(_connectionString, tableName, batchSize);

        /// <summary>
        /// Registrovanie factory metód na vytvorenie inštancie do <see cref="IdGeneratorFactories"/>.
        /// </summary>
        public static void Register() =>
            IdGeneratorFactories.Register<SqlConnection>("System.Data.SqlClient",
                (conn) => new SqlServerIdGeneratorFactory(conn as SqlConnection),
                (connString) => new SqlServerIdGeneratorFactory(connString));
    }
}
