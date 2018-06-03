using Kros.Data.MsAccess;
using Kros.Utils;
using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Threading.Tasks;

namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// Trieda umožňujúca rýchlu hromadnú editáciu dát pre Ms Access.
    /// </summary>
    /// <remarks>Používa tempovú databazázu.</remarks>
    /// <example>
    ///   <code source="..\Examples\Kros.Utils.MsAccess\MsAccessExamples.cs" title="Bulk update" region="BulkUpdate" lang="C#" />
    /// </example>
    public class MsAccessBulkUpdate : BulkUpdateBase
    {
        #region Private fields

        private string _tempDatabasePath;
        private OleDbConnection _tempDatabase;

        #endregion


        #region Constructors

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkUpdate"/> použitím spojenia na databázu
        /// <paramref name="connectionString"/>.</summary>
        /// <param name="connectionString">Spojenie na databázu, kam sa vložia dáta.</param>
        public MsAccessBulkUpdate(string connectionString)
        {
            Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            ExternalTransaction = null;
            _connection = new OleDbConnection(connectionString);
            _disposeOfConnection = true;
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkUpdate"/> použitím spojenia na databázu
        /// <paramref name="connection"/>.</summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.</param>
        public MsAccessBulkUpdate(OleDbConnection connection)
            : this(connection, null)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkUpdate"/> použitím spojenia na databázu
        /// <paramref name="connection"/> a externej transakcie <paramref name="externalTransaction"/>.</summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        public MsAccessBulkUpdate(OleDbConnection connection, OleDbTransaction externalTransaction)
        {
            _connection = connection;
            ExternalTransaction = externalTransaction;
        }

        #endregion


        #region BulkUpdateBase Members

        /// <inheritdoc/>
        protected override IBulkInsert CreateBulkInsert() => new MsAccessBulkInsert(_tempDatabase);

        /// <inheritdoc/>
        protected override void InvokeAction(string tempTableName)
        {
            TempTableAction?.Invoke(_tempDatabase, null, tempTableName);
            _tempDatabase.Close();
        }

        /// <inheritdoc/>
        protected override string GetTempTableName()
        {
            ProviderType provider = MsAccessDataHelper.GetProviderType(_connection);

            _tempDatabasePath = Path.GetTempFileName();
            MsAccessDataHelper.CreateEmptyDatabase(_tempDatabasePath, provider);
            _tempDatabase = new OleDbConnection(MsAccessDataHelper.CreateConnectionString(_tempDatabasePath, provider));
            _tempDatabase.Open();

            return DestinationTableName;
        }

        /// <inheritdoc/>
        protected override void CreateTempTable(IDataReader reader, string tempTableName)
        {
            using (var cmd = _tempDatabase.CreateCommand())
            {
                cmd.CommandText = $"SELECT {GetColumnNamesForTempTable(reader)} INTO {(tempTableName)} " +
                                  $"FROM [{(_connection as OleDbConnection).DataSource}].[{DestinationTableName}] " +
                                  $"WHERE (1 = 2)";
                cmd.ExecuteNonQuery();
            }
        }

        /// <inheritdoc/>
        protected override string GetTempTableNameForBulkInsert(string name) => name;

        /// <inheritdoc/>
        protected override IDbCommand CreateCommandForPrimaryKey()
        {
            return _tempDatabase.CreateCommand();
        }

        /// <inheritdoc/>
        protected async override Task UpdateDestinationTableAsync(IDataReader reader, string tempTableName, bool useAsync)
        {
            using (var cmd = _connection.CreateCommand())
            {
                var tempDatabasePathAndTable = $"[{_tempDatabasePath}].[{tempTableName}]";
                var tempAlias = Guid.NewGuid();
                var innerJoin = $"[{DestinationTableName}].[{PrimaryKeyColumn}] = [{tempAlias}].[{PrimaryKeyColumn}]";

                cmd.Transaction = ExternalTransaction;
                cmd.CommandText = $"UPDATE [{DestinationTableName}] INNER JOIN {tempDatabasePathAndTable} AS [{tempAlias}] " +
                                                                  $"ON ({innerJoin}) " +
                                  $"SET {GetUpdateColumnNames(reader, $"{tempAlias}")} ";
                if (useAsync)
                {
                    await (cmd as DbCommand).ExecuteNonQueryAsync();
                }
                else
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_tempDatabase != null)
                {
                    _tempDatabase.Dispose();
                    _tempDatabase = null;
                }

                if (!string.IsNullOrEmpty(_tempDatabasePath))
                {
                    if (File.Exists(_tempDatabasePath))
                    {
                        File.Delete(_tempDatabasePath);
                    }
                    _tempDatabasePath = Path.ChangeExtension(_tempDatabasePath, "ldb");
                    if (File.Exists(_tempDatabasePath))
                    {
                        File.Delete(_tempDatabasePath);
                    }
                    _tempDatabasePath = null;
                }
            }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        #endregion

    }
}
