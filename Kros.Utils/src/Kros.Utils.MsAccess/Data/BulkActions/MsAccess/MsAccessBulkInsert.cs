using Kros.Data.Schema;
using Kros.Data.Schema.MsAccess;
using Kros.Utils;
using System;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kros.Data.BulkActions.MsAccess
{
    /// <summary>
    /// The calss for fast bulk insert big amount of data into Microsoft Access database.
    /// </summary>
    /// <remarks>
    /// In the background, it creates a text CSV file with data which are inserted.
    /// </remarks>
    public class MsAccessBulkInsert : IBulkInsert
    {
        #region Constants

        /// <summary>
        /// Default value separator for CSV file: comma (<b>,</b>).
        /// </summary>
        public const char DefaultValueDelimiter = ',';

        /// <summary>
        /// Default code page: <see cref="Utf8CodePage"/>.
        /// </summary>
        public const int DefaultCodePage = Utf8CodePage;

        /// <summary>
        /// UTF-8 code page: 65001.
        /// </summary>
        public const int Utf8CodePage = 65001;

        /// <summary>
        /// Windows Central Europe code page: <b>1250</b>.
        /// </summary>
        public const int WindowsCentralEuropeCodePage = 1250;

        /// <summary>
        /// ANSI code page.
        /// </summary>
        public const int AnsiCodePage = int.MaxValue;

        /// <summary>
        /// OEM code page.
        /// </summary>
        public const int OemCodePage = int.MaxValue - 1;

        #endregion

        #region Private fields

        private OleDbConnection _connection;
        private readonly bool _disposeOfConnection = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance with database connection specifiend in <paramref name="connectionString"/>
        /// and default settings for CSV file.
        /// </summary>
        /// <param name="connectionString">Connection string for the database connection where the data will be inserted.</param>
        public MsAccessBulkInsert(string connectionString)
        {
            Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            ExternalTransaction = null;
            CodePage = DefaultCodePage;
            ValueDelimiter = DefaultValueDelimiter;
            _connection = new OleDbConnection(connectionString);
            _disposeOfConnection = true;
        }

        /// <summary>
        /// Creates a new instance with database connection <paramref name="connection"/> and default settings for CSV file.
        /// </summary>
        /// <param name="connection">Database connection where the data will be inserted. The connection mus be opened.</param>
        /// <exception cref="ArgumentNullException">Value of <paramref name="connection"/> is <see langword="null"/>.</exception>
        public MsAccessBulkInsert(OleDbConnection connection)
            : this(connection, null, DefaultCodePage, DefaultValueDelimiter)
        {
        }

        /// <summary>
        /// Creates a new instance with database connection <paramref name="connection"/>, transaction
        /// <paramref name="externalTransaction"/> and default settings for CSV file.
        /// </summary>
        /// <param name="connection">Database connection where the data will be inserted. The connection mus be opened.
        /// If there already is running transaction in this connection, it must be specified in
        /// <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Transaction in which the bulk insert will be performed.</param>
        /// <exception cref="ArgumentNullException">Value of <paramref name="connection"/> is <see langword="null"/>.</exception>
        public MsAccessBulkInsert(OleDbConnection connection, OleDbTransaction externalTransaction)
            : this(connection, externalTransaction, DefaultCodePage, DefaultValueDelimiter)
        {
        }

        /// <summary>
        /// Creates a new instance with database connection <paramref name="connection"/>, transaction
        /// <paramref name="externalTransaction"/> and CSV file code page <paramref name="csvFileCodePage"/>.
        /// </summary>
        /// <param name="connection">Database connection where the data will be inserted. The connection mus be opened.
        /// If there already is running transaction in this connection, it must be specified in
        /// <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Transaction in which the bulk insert will be performed.</param>
        /// <param name="csvFileCodePage">Code page for generated CSV file.</param>
        /// <exception cref="ArgumentNullException">Value of <paramref name="connection"/> is <see langword="null"/>.</exception>
        public MsAccessBulkInsert(OleDbConnection connection, OleDbTransaction externalTransaction, int csvFileCodePage)
            : this(connection, externalTransaction, csvFileCodePage, DefaultValueDelimiter)
        {
        }

        /// <summary>
        /// Creates a new instance with database connection <paramref name="connection"/>, transaction
        /// <paramref name="externalTransaction"/> and CSV file settings <paramref name="csvFileCodePage"/>
        /// and <paramref name="valueDelimiter"/>.
        /// </summary>
        /// <param name="connection">Database connection where the data will be inserted. The connection mus be opened.
        /// If there already is running transaction in this connection, it must be specified in
        /// <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Transaction in which the bulk insert will be performed.</param>
        /// <param name="csvFileCodePage">Code page for generated CSV file.</param>
        /// <param name="valueDelimiter">Value delimiter for generated CSV file.</param>
        /// <exception cref="ArgumentNullException">Value of <paramref name="connection"/> is <see langword="null"/>.</exception>
        public MsAccessBulkInsert(
            OleDbConnection connection,
            OleDbTransaction externalTransaction,
            int csvFileCodePage,
            char valueDelimiter)
        {
            _connection = Check.NotNull(connection, nameof(connection));
            ExternalTransaction = externalTransaction;
            CodePage = csvFileCodePage;
            ValueDelimiter = valueDelimiter;
        }

        #endregion

        #region Common

        /// <summary>
        /// List of inserted columns.
        /// </summary>
        /// <value>List of columns as <see cref="MsAccessBulkInsertColumnCollection" />.</value>
        /// <remarks>Columns in the list must be in the same order as they are in input CSV file.</remarks>
        public MsAccessBulkInsertColumnCollection Columns { get; } = new MsAccessBulkInsertColumnCollection();

        /// <summary>
        /// Code page used for CSV file and bulk insert. Default value is 65001 <see cref="Utf8CodePage"/>.
        /// </summary>
        /// <value>Number of code page.</value>
        public int CodePage { get; }

        /// <summary>
        /// Value separator in generated CSV file.
        /// </summary>
        public char ValueDelimiter { get; }

        /// <summary>
        /// External transaction, in which bulk insert is executed.
        /// </summary>
        public OleDbTransaction ExternalTransaction { get; }

        #endregion

        #region IBulkInsert

        /// <summary>
        /// This setting is not used.
        /// </summary>
        public int BatchSize { get; set; } = 0;

        /// <summary>
        /// This setting is not used.
        /// </summary>
        public int BulkInsertTimeout { get; set; }

        /// <summary>
        /// Destination table name in database.
        /// </summary>
        public string DestinationTableName { get; set; }

        /// <inheritdoc/>
        public void Insert(IBulkActionDataReader reader)
        {
            using (var bulkInsertReader = new BulkActionDataReader(reader))
            {
                Insert(bulkInsertReader);
            }
        }

        /// <inheritdoc/>
        public async Task InsertAsync(IBulkActionDataReader reader)
        {
            using (var bulkInsertReader = new BulkActionDataReader(reader))
            {
                await InsertAsync(bulkInsertReader);
            }
        }

        /// <inheritdoc/>
        public void Insert(IDataReader reader) => InsertCoreAsync(reader, useAsync: false).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public Task InsertAsync(IDataReader reader) => InsertCoreAsync(reader, useAsync: true);

        private async Task InsertCoreAsync(IDataReader reader, bool useAsync)
        {
            string filePath = null;

            try
            {
                filePath = CreateDataFile(reader);
                InitBulkInsert(filePath, reader);
                await InsertAsync(filePath, useAsync);
            }
            finally
            {
                if (filePath != null)
                {
                    try
                    {
                        string dataFolder = Path.GetDirectoryName(filePath);
                        if (Directory.Exists(dataFolder))
                        {
                            Directory.Delete(dataFolder, true);
                        }
                    }
                    catch { }
                }
            }
        }

        /// <inheritdoc/>
        public void Insert(DataTable table)
        {
            using (var reader = table.CreateDataReader())
            {
                Insert(reader);
            }
        }

        /// <inheritdoc/>
        public async Task InsertAsync(DataTable table)
        {
            using (var reader = table.CreateDataReader())
            {
                await InsertAsync(reader);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private async Task InsertAsync(string sourceFilePath, bool useAsync)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException(string.Format("Dáta nie je možné vložiť. Dátový súbor \"{0}\" neexistuje.",
                                                              sourceFilePath), sourceFilePath);
            }
            if ((new FileInfo(sourceFilePath)).Length == 0)
            {
                return;
            }

            if ((ExternalTransaction == null) && _connection.IsOpened())
            {
                _connection.Close();
                _connection.Open();
            }

            using (ConnectionHelper.OpenConnection(_connection))
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = CreateInsertSql(DestinationTableName, sourceFilePath);
                cmd.Transaction = ExternalTransaction;
                if (useAsync)
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Helpers

        private string CreateDataFile(IDataReader data)
        {
            string dataFilePath;

            using (CsvFileWriter csvWriter = CreateCsvWriter())
            {
                dataFilePath = csvWriter.FilePath;
                csvWriter.Write(data);
            }

            return dataFilePath;
        }

        private CsvFileWriter CreateCsvWriter()
        {
            string dataFile = Path.Combine(CreateDataFolder(), "data.csv");
            if (CodePage == Utf8CodePage)
            {
                return new CsvFileWriter(dataFile, new UTF8Encoding(false), false);
            }
            return new CsvFileWriter(dataFile, CodePage, false);
        }

        private string CreateDataFolder()
        {
            const string mainFolderName = "KrosBulkInsert";
            string folder = Path.Combine(Path.GetTempPath(), mainFolderName, Guid.NewGuid().ToString());
            while (Directory.Exists(folder))
            {
                folder = Path.Combine(Path.GetTempPath(), mainFolderName, Guid.NewGuid().ToString());
            }
            Directory.CreateDirectory(folder);

            return folder;
        }

        // Vytvorá SQL príkaz pre vloženie dát do tabuľky "tableName" zo vstupného súboru "sourceFilePath".
        private string CreateInsertSql(string tableName, string sourceFilePath)
        {
            StringBuilder sql = new StringBuilder(2000);

            sql.AppendFormat("INSERT INTO {0} (", tableName);
            foreach (var column in Columns)
            {
                sql.AppendFormat("[{0}], ", column.ColumnName);
            }
            sql.Length -= 2; // Removes last comma and space.
            sql.AppendLine(")");

            sql.Append("SELECT ");
            dynamic i = 0;
            foreach (var column in Columns)
            {
                i += 1;
                if (column.ColumnType == BulkInsertColumnType.Text)
                {
                    sql.AppendFormat("IIF(F{0} IS NULL, '', F{0}) AS [{1}], ", i, column.ColumnName);
                }
                else
                {
                    sql.AppendFormat("F{0} AS [{1}], ", i, column.ColumnName);
                }
            }
            sql.Length -= 2; // Removes last comma and space.
            sql.AppendLine();
            sql.AppendFormat("FROM [Text;Database={0}].[{1}]",
                Path.GetDirectoryName(sourceFilePath), Path.GetFileName(sourceFilePath));

            return sql.ToString();
        }

        private string SqlCodePage
        {
            get
            {
                if (CodePage == AnsiCodePage)
                {
                    return "ANSI";
                }
                else if (CodePage == OemCodePage)
                {
                    return "OEM";
                }
                return CodePage.ToString();
            }
        }

        private void InitBulkInsert(string dataFilePath, IDataReader data)
        {
            using (StreamWriter schemaFile = InitSchemaFile(dataFilePath))
            {
                var schemaLoader = new MsAccessSchemaLoader();
                TableSchema tableSchema = schemaLoader.LoadTableSchema(_connection, DestinationTableName);
                for (int i = 0; i < data.FieldCount; i++)
                {
                    MsAccessColumnSchema column = tableSchema.Columns[data.GetName(i)] as MsAccessColumnSchema;
                    Columns.Add(column);
                    int colNumber = i + 1;
                    schemaFile.WriteLine($"Col{colNumber}=F{colNumber} {GetColumnDataType(column)}");
                }
            }
        }

        private StreamWriter InitSchemaFile(string dataFilePath)
        {
            string dataFolder = Path.GetDirectoryName(dataFilePath);
            string fileName = Path.GetFileName(dataFilePath);

            StreamWriter writer = new StreamWriter(Path.Combine(dataFolder, "schema.ini"), false, Encoding.ASCII);

            writer.WriteLine($"[{fileName}]");
            writer.WriteLine($"Format=Delimited({ValueDelimiter})");
            writer.WriteLine($"CharacterSet={SqlCodePage}");
            writer.WriteLine("MaxScanRows=25");
            writer.WriteLine("ColNameHeader=False");
            writer.WriteLine("DecimalSymbol=.");
            writer.WriteLine("DateTimeFormat=yyyy-mm-dd hh:nn:ss");

            return writer;
        }

        private string GetColumnDataType(MsAccessColumnSchema column)
        {
            switch (column.OleDbType)
            {
                case OleDbType.Boolean:
                    return "Bit";

                case OleDbType.TinyInt:
                case OleDbType.UnsignedTinyInt:
                    return "Byte";

                case OleDbType.SmallInt:
                case OleDbType.UnsignedSmallInt:
                    return "Short";

                case OleDbType.Integer:
                case OleDbType.BigInt:
                case OleDbType.UnsignedInt:
                case OleDbType.UnsignedBigInt:
                    return "Long";

                case OleDbType.Decimal:
                case OleDbType.Numeric:
                    return "Decimal";

                case OleDbType.Currency:
                    return "Currency";

                case OleDbType.Single:
                    return "Single";

                case OleDbType.Double:
                    return "Double";

                case OleDbType.Date:
                    return "DateTime";

                case OleDbType.Guid:
                    return "Text";

                case OleDbType.Char:
                case OleDbType.WChar:
                case OleDbType.VarChar:
                case OleDbType.VarWChar:
                case OleDbType.LongVarWChar:
                case OleDbType.LongVarChar:
                    if (column.Size > 0)
                    {
                        return $"Text Width {column.Size}";
                    }
                    else
                    {
                        return "Memo";
                    }

                default:
                    throw new InvalidOperationException(
                        $"Nepodporovaný dátový typ pre MsAccessBulkInsert: OleDbType.{column.OleDbType}");
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
            Dispose(true);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion
    }
}
