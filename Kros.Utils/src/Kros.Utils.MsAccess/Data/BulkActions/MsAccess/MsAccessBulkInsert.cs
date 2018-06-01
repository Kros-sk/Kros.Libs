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
    /// Trieda na hromadné vkladanie dát do MS Access tabuľky.
    /// </summary>
    /// <remarks>
    /// Na pozadí sa využíva vkladanie dát z textového CSV súboru.
    /// </remarks>
    public class MsAccessBulkInsert : IBulkInsert
    {

        #region Constants

        /// <summary>
        /// Predvolený oddeľovať dát vo vstupnom súbore - čiarka (<b>,</b>).
        /// </summary>
        public const char DefaultValueDelimiter = ',';

        /// <summary>
        /// Predvolená kódová stránka vstupného súboru - <b>UTF-8</b>.
        /// </summary>
        public const int DefaultCodePage = Utf8CodePage;

        /// <summary>
        /// Kódová stránka UTF-8.
        /// </summary>
        public const int Utf8CodePage = 65001;

        /// <summary>
        /// Kódová stránka vstupného súboru - <b>1250</b>.
        /// </summary>
        public const int WindowsCentralEuropeCodePage = 1250;

        /// <summary>
        /// Kódová stránka ANSI.
        /// </summary>
        public const int AnsiCodePage = int.MaxValue;

        /// <summary>
        /// Kódová stránka OEM.
        /// </summary>
        public const int OemCodePage = int.MaxValue - 1;

        #endregion


        #region Private fields

        private OleDbConnection _connection;
        private readonly bool _disposeOfConnection = false;

        #endregion


        #region Constructors

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connectionString"/>.</summary>
        /// <param name="connectionString">Spojenie na databázu, kam sa vložia dáta.</param>
        /// <remarks></remarks>
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
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/></summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.</param>
        /// <remarks></remarks>
        public MsAccessBulkInsert(OleDbConnection connection)
            : this(connection, null, DefaultCodePage, DefaultValueDelimiter)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/> a externej transakcie <paramref name="externalTransaction"/>.</summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        /// <remarks></remarks>
        public MsAccessBulkInsert(OleDbConnection connection, OleDbTransaction externalTransaction)
            : this(connection, externalTransaction, DefaultCodePage, DefaultValueDelimiter)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/>, externej transakcie <paramref name="externalTransaction"/>, kódovaním CSV súboru
        /// <paramref name="csvFileCodePage"/>.</summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        /// <param name="csvFileCodePage">Kódová stránka vstupného súboru.</param>
        /// <remarks></remarks>
        public MsAccessBulkInsert(OleDbConnection connection, OleDbTransaction externalTransaction, int csvFileCodePage)
            : this(connection, externalTransaction, csvFileCodePage, DefaultValueDelimiter)
        {
        }

        /// <summary>
        /// Inicializuje novú inštanciu <see cref="MsAccessBulkInsert"/> použitím spojenia na databázu
        /// <paramref name="connection"/>, externej transakcie <paramref name="externalTransaction"/>, kódovaním CSV súboru
        /// <paramref name="csvFileCodePage"/> a oddeľovačom hodnôt v CSV súbore <paramref name="valueDelimiter"/>.
        /// </summary>
        /// <param name="connection">Spojenie na databázu, kam sa vložia dáta. Spojenie musí byť otvorené.
        /// Ak je na spojení spustená transakcia, musí byť zadaná v parametri <paramref name="externalTransaction"/>.</param>
        /// <param name="externalTransaction">Externá transakcia, v ktorej hromadné vloženie prebehne.</param>
        /// <param name="csvFileCodePage">Kódová stránka vstupného súboru.</param>
        /// <param name="valueDelimiter">Oddeľovač dát vo vstupnom súbore.</param>
        /// <remarks></remarks>
        public MsAccessBulkInsert(
            OleDbConnection connection,
            OleDbTransaction externalTransaction,
            int csvFileCodePage,
            char valueDelimiter)
        {
            _connection = connection;
            ExternalTransaction = externalTransaction;
            CodePage = csvFileCodePage;
            ValueDelimiter = valueDelimiter;
        }

        #endregion


        #region Common

        /// <summary>
        /// Zoznam vkladaných stĺpcov.
        /// </summary>
        /// <value>Zoznam stĺpcov <see cref="MsAccessBulkInsertColumnCollection" />.</value>
        /// <remarks>Stĺpce musia byť v zozname v rovnakom poradí, ako sú uložené vo vstupnom súbore.</remarks>
        public MsAccessBulkInsertColumnCollection Columns { get; } = new MsAccessBulkInsertColumnCollection();

        /// <summary>
        /// Kódová stránka vstupného súboru.
        /// </summary>
        /// <value>Číslo.</value>
        /// <remarks></remarks>
        public int CodePage { get; }

        /// <summary>
        /// Oddeľovač hodnôt v CSV súbore.
        /// </summary>
        public char ValueDelimiter { get; }

        /// <summary>
        /// Externá transakcia, v ktorej sa vykoná vloženie dát.
        /// </summary>
        public OleDbTransaction ExternalTransaction { get; }

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

        /// <summary>
        /// <c>MsAccessBulkInsert</c> toto nastavenie nepoužíva.
        /// </summary>
        public int BulkInsertTimeout { get; set; }

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

        /// <summary>
        /// Hromadne vloží dáta z CSV súboru <paramref name="sourceFilePath" />.
        /// </summary>
        /// <param name="sourceFilePath">Cesta k vstupnému súboru dát.</param>
        /// <param name="useAsync">Can execute asynchronously?</param>
        /// <exception cref="FileNotFoundException">Vstupný súbor <paramref name="sourceFilePath" /> neexistuje.</exception>
        /// <remarks>V prípade, že súbor existuje, ale je prázdny, metóda nič nerobí a vráti <b>0</b>.</remarks>
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

        /// <summary>
        /// Vytvorá SQL príkaz pre vloženie dát do tabuľky <paramref name="tableName">tableName</paramref>
        /// zo vstupného súboru <paramref name="sourceFilePath">sourceFilePath</paramref>
        /// </summary>
        /// <param name="tableName">Názov tabuľky, do ktorej sa vkladá.</param>
        /// <param name="sourceFilePath">Cesta k vstupnému súboru s dátami.</param>
        /// <returns>Reťazec - SQL príkaz, na vloženie dát.</returns>
        /// <remarks></remarks>
        private string CreateInsertSql(string tableName, string sourceFilePath)
        {
            StringBuilder sql = new StringBuilder(2000);

            sql.AppendFormat("INSERT INTO {0} (", tableName);
            foreach (var column in Columns)
            {
                sql.AppendFormat("[{0}], ", column.ColumnName);
            }
            sql.Length -= 2; // Odstránenie poslednej čiarky a medzery.
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
            sql.Length -= 2; // Odstránenie poslednej čiarky a medzery.
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

        /// <summary>
        /// Uvoľní všetky zdroje používané <c>MsAccessBulkInsert</c>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Uvoľní všetky zdroje triedy <c>MsAccessBulkInsert</c>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion

    }
}
