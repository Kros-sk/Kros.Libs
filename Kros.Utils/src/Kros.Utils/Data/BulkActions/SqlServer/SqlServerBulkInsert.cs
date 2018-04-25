using Kros.Data.Schema;
using Kros.Utils;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Kros.Data.BulkActions.SqlServer
{
    /// <summary>
    /// Trieda umožňujúca rýchle hromadné vkladanie dát pre SQL Server.
    /// </summary>
    public class SqlServerBulkInsert : IBulkInsert
    {

        /// <summary>
        /// Predvolená hodnota <see cref="SqlBulkCopyOptions"/> pre internú inštanciu <see cref="SqlBulkCopy"/>, ak sa nepoužíva
        /// externá transakcia.
        /// hodnota je <c>SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction</c>.
        /// </summary>
        public static SqlBulkCopyOptions DefaultBulkCopyOptions { get; } =
            SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction;

        /// <summary>
        /// Predvolená hodnota <see cref="SqlBulkCopyOptions"/> pre internú inštanciu <see cref="SqlBulkCopy"/>, ak sa používa
        /// externá transakcia.
        /// Hodnota je ako <c>SqlBulkCopyOptions.TableLock</c>.
        /// </summary>
        public static SqlBulkCopyOptions DefaultBulkCopyOptionsExternalTransaction { get; } = SqlBulkCopyOptions.TableLock;

        #region Private fields

        private SqlConnection _connection;
        private readonly bool _disposeOfConnection = false;

        #endregion


        #region Constructors

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, mysí sa použiť konštruktor s externou transakciou a tá musí
        /// byť zadaná.</param>
        public SqlServerBulkInsert(SqlConnection connection)
            : this(connection, null, DefaultBulkCopyOptions)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/> a externej transakcie <paramref name="externalTransaction"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        public SqlServerBulkInsert(SqlConnection connection, SqlTransaction externalTransaction)
            : this(connection, externalTransaction,
                  externalTransaction == null ? DefaultBulkCopyOptions : DefaultBulkCopyOptionsExternalTransaction)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/> a zadaných nastavení <paramref name="options"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.</param>
        /// <param name="options">Nastavenia <see cref="SqlBulkCopyOptions"/>.</param>
        public SqlServerBulkInsert(SqlConnection connection, SqlBulkCopyOptions options)
            : this(connection, null, options)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/>, externej transakcie <paramref name="externalTransaction"/>
        /// a zadaných nastavení <paramref name="options"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        /// <param name="options">Nastavenia <see cref="SqlBulkCopyOptions"/>.</param>
        public SqlServerBulkInsert(SqlConnection connection, SqlTransaction externalTransaction, SqlBulkCopyOptions options)
        {
            Check.NotNull(connection, nameof(connection));
            _connection = connection;
            ExternalTransaction = externalTransaction;
            BulkCopyOptions = options;
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Spojenie na databázu, kam sa vložia dáta.</param>
        public SqlServerBulkInsert(string connectionString)
            : this(connectionString, DefaultBulkCopyOptions)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="SqlServerBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connectionString"/> a zadaných nastavení <paramref name="options"/>.
        /// </summary>
        /// <param name="connectionString">Spojenie na databázu, kam sa vložia dáta.</param>
        /// <param name="options">Nastavenia <see cref="SqlBulkCopyOptions"/>.</param>
        public SqlServerBulkInsert(string connectionString, SqlBulkCopyOptions options)
            : this(new SqlConnection(connectionString), null, options)
        {
            _disposeOfConnection = true;
        }

        #endregion


        #region Common

        /// <summary>
        /// Externá transakcia, v ktorej sa vykoná vloženie dát.
        /// </summary>
        public SqlTransaction ExternalTransaction { get; }

        /// <summary>
        /// Nastavenia <see cref="BulkCopyOptions"/> pre internú inštanciu <see cref="SqlBulkCopy"/>.
        /// </summary>
        public SqlBulkCopyOptions BulkCopyOptions { get; }

        #endregion


        #region IBulkInsert

        private int _batchSize = 0;

        /// <summary>
        /// Počet riadkov v dávke, ktorá sa posiela do databázy. Ak je hodnota 0, veľkosť dávky nie je obmedzená.
        /// </summary>
        /// <exception cref="ArgumentException">Zadaná hodnota je záporná.</exception>
        public int BatchSize
        {
            get
            {
                return _batchSize;
            }
            set
            {
                _batchSize = Check.GreaterOrEqualThan(value, 0, nameof(value));
            }
        }


        private int _bulkInsertTimeout = 0;

        /// <summary>
        /// Počet sekúnd na dokončenie operácie. ak je hodnota 0, trvanie operácie nie je obmedzené.
        /// </summary>
        /// <exception cref="ArgumentException">Zadaná hodnota je záporná.</exception>
        public int BulkInsertTimeout
        {
            get
            {
                return _bulkInsertTimeout;
            }
            set
            {
                _bulkInsertTimeout = Check.GreaterOrEqualThan(value, 0, nameof(value));
            }
        }

        /// <summary>
        /// Meno cieľovej tabuľky v databáze.
        /// </summary>
        public string DestinationTableName { get; set; }

        /// <summary>
        /// Vloží všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        public void Insert(IBulkActionDataReader reader)
        {
            using (var bulkInsertReader = new BulkActionDataReader(reader))
            {
                Insert(bulkInsertReader);
            }
        }

        /// <summary>
        /// Vloží všetky dáta zo zdroja <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">Zdroj dát.</param>
        public void Insert(IDataReader reader)
        {
            using (ConnectionHelper.OpenConnection(_connection))
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection, BulkCopyOptions, ExternalTransaction))
            {
                bulkCopy.DestinationTableName = DestinationTableName;
                bulkCopy.BulkCopyTimeout = BulkInsertTimeout;
                bulkCopy.BatchSize = BatchSize;
                SetColumnMappings(bulkCopy, reader);
                bulkCopy.WriteToServer(reader);
            }
        }

        private void SetColumnMappings(SqlBulkCopy bulkCopy, IDataReader reader)
        {
            var tableSchema = DatabaseSchemaLoader.Default.LoadTableSchema(_connection, DestinationTableName);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string sourceColumn = reader.GetName(i);
                string destinationColumn = sourceColumn;

                if (tableSchema != null)
                {
                    if (tableSchema.Columns.Contains(sourceColumn))
                    {
                        destinationColumn = tableSchema.Columns[sourceColumn].Name;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Destination table \"{bulkCopy.DestinationTableName}\" does not have column \"{sourceColumn}\".");
                    }
                }
                bulkCopy.ColumnMappings.Add(sourceColumn, destinationColumn);
            }
        }

        /// <summary>
        /// Vloží všetky riadky z tabuľky <paramref name="table"/>.
        /// </summary>
        /// <param name="table">Zdrojové dáta.</param>
        public void Insert(DataTable table)
        {
            using (var reader = table.CreateDataReader())
            {
                Insert(reader);
            }
        }

        #endregion


        #region IDisposable

        private bool disposedValue = false;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_disposeOfConnection)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion

    }
}
