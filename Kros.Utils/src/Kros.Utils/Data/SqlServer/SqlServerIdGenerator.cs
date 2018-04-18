using Kros.Properties;
using Kros.Utils;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace Kros.Data.SqlServer
{
    /// <summary>
    /// Generátor unikátnych identifikátorov pre sql server.
    /// </summary>
    /// <seealso cref="IdGeneratorFactories" />
    /// <seealso cref="SqlServerIdGeneratorFactory" />
    /// <remarks>Štandardne sa nevytvára priamo ale cez <see cref="SqlServerIdGeneratorFactory"/>.</remarks>
    /// <example>
    /// <code language="cs" source="..\Examples\Kros.Utils\IdGeneratorExamples.cs" region="IdGeneratorFactory"/>
    /// </example>
    public class SqlServerIdGenerator
        : IdGeneratorBase
    {

        private const string GetNewIdSpName = "spGetNewId";

        /// <summary>
        /// Konštruktor.
        /// </summary>
        /// <param name="connectionString">
        /// Connection string, ktorý sa použije na vytvorenie conenction pre získavanie unikátnych identifikátorov.
        /// </param>
        /// <param name="tableName">Názov tabuľky, pre ktorú generujem identifikátory.</param>
        /// <param name="batchSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        public SqlServerIdGenerator(string connectionString, string tableName, int batchSize)
            : base(connectionString, tableName, batchSize)
        {
        }

        /// <summary>
        /// Konštruktor.
        /// </summary>
        /// <param name="connection">Connection, ktorá sa použije pre získavanie unikátnych identifikátorov.</param>
        /// <param name="tableName">Názov tabuľky, pre ktorú generujem identifikátory.</param>
        /// <param name="batchSize">Veľkosť dávky, ktorú si zarezervuje dopredu.</param>
        public SqlServerIdGenerator(SqlConnection connection, string tableName, int batchSize)
            : base(connection, tableName, batchSize)
        {

        }

        /// <inheritdoc/>
        protected override int GetNewIdFromDbCore()
        {
            using (var cmd = Connection.CreateCommand() as SqlCommand)
            {
                cmd.CommandText = GetNewIdSpName;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TableName", TableName);
                cmd.Parameters.AddWithValue("@NumberOfItems", BatchSize);

                return (int)cmd.ExecuteScalar();
            }
        }

        /// <inheritdoc/>
        protected override DbConnection CreateConnection(string connectionString) =>
            new SqlConnection(connectionString);

        /// <summary>
        /// Získanie scriptu na vytvorenie store procedure.
        /// </summary>
        public static string GetStoredProcedureCreationScript() =>
            ResourceHelper.GetResourceContent("Kros.Resources.SqlIdGeneratorStoredProcedureScript.sql");

        /// <summary>
        /// Získanie scriptu na vytvorenie IdStore tabuľky.
        /// </summary>
        public static string GetIdStoreTableCreationScript() =>
            ResourceHelper.GetResourceContent("Kros.Resources.SqlIdGeneratorTableScript.sql");

        /// <inheritdoc/>
        public override void InitDatabaseForIdGenerator()
        {
            using (ConnectionHelper.OpenConnection(Connection))
            using (var cmd = Connection.CreateCommand())
            {
                cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IdStore' AND type = 'U')" +
                    Environment.NewLine + GetIdStoreTableCreationScript();
                cmd.ExecuteNonQuery();

                cmd.CommandText = "IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'spGetNewId' AND type = 'P')" +
                    Environment.NewLine + $"EXEC('{GetStoredProcedureCreationScript().Replace("'", "''")}');";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
