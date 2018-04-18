using Kros.Utils;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Kros.Data.BulkActions.SqlServer
{
    /// <summary>
    /// Trieda umožňujúca rýchlu hromadnú editáciu dát pre SQL Server.
    /// </summary>
    /// <example>
    ///   <code source="..\Examples\Kros.Utils\BulkUpdateExamples.cs" title="Bulk update" region="BulkUpdate" lang="C#" />
    /// </example>
    public class SqlServerBulkUpdate : BulkUpdateBase
    {

        #region Constructors

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkUpdate"/> použitím spojenia na databázu
        /// <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, mysí sa použiť konštruktor s externou transakciou a tá musí
        /// byť zadaná.</param>
        public SqlServerBulkUpdate(SqlConnection connection)
            : this(connection, null)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkUpdate"/> použitím spojenia na databázu
        /// <paramref name="connection"/> a externej transakcie <paramref name="externalTransaction"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        public SqlServerBulkUpdate(SqlConnection connection, SqlTransaction externalTransaction)
        {
            _connection = Check.NotNull(connection, nameof(connection));

            ExternalTransaction = externalTransaction;
        }


        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkUpdate"/> použitím spojenia na databázu
        /// <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Spojenie na databázu, kam sa vložia dáta.</param>
        public SqlServerBulkUpdate(string connectionString)
            : this(new SqlConnection(connectionString), null)
        {
            _disposeOfConnection = true;
        }

        #endregion


        #region BulkUpdateBase Members

        /// <inheritdoc/>
        protected override IBulkInsert CreateBulkInsert()
        {
            return new SqlServerBulkInsert((SqlConnection)_connection, (SqlTransaction)ExternalTransaction);
        }

        /// <inheritdoc/>
        protected override void InvokeAction(string tempTableName)
        {
            TempTableAction?.Invoke(_connection, ExternalTransaction, tempTableName);
        }

        /// <inheritdoc/>
        protected override string GetTempTableName() => $"{PrefixTempTable}{DestinationTableName}_{Guid.NewGuid()}";

        /// <inheritdoc/>
        protected override void CreateTempTable(IDataReader reader, string tempTableName)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = ExternalTransaction;
                cmd.CommandText = $"SELECT {GetColumnNamesForTempTable(reader)} INTO [{tempTableName}] " +
                                  $"FROM [{DestinationTableName}] " +
                                  $"WHERE (1 = 2)";
                cmd.ExecuteNonQuery();
            }
        }

        /// <inheritdoc/>
        protected override IDbCommand CreateCommandForPrimaryKey()
        {
            var ret = _connection.CreateCommand();

            ret.Transaction = ExternalTransaction;

            return ret;
        }

        /// <inheritdoc/>
        protected override void UpdateDestinationTable(IDataReader reader, string tempTableName)
        {
            using (var cmd = _connection.CreateCommand())
            {
                var innerJoin = $"[{DestinationTableName}].[{PrimaryKeyColumn}] = [{tempTableName}].[{PrimaryKeyColumn}]";

                cmd.Transaction = ExternalTransaction;
                cmd.CommandText = $"UPDATE [{DestinationTableName}] " +
                                  $"SET {GetUpdateColumnNames(reader, tempTableName)} " +
                                  $"FROM [{DestinationTableName}] INNER JOIN [{tempTableName}] " +
                                                                $"ON ({innerJoin})";
                cmd.ExecuteNonQuery();
            }
        }

        /// <inheritdoc/>
        protected override void DoneTempTable(string tempTableName)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.Transaction = ExternalTransaction;
                cmd.CommandText = $"DROP TABLE [{tempTableName}]";
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

    }
}
