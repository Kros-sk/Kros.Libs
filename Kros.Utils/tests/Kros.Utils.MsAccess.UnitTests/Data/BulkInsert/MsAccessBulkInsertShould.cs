using Kros.Data.BulkActions;
using Kros.Data.BulkActions.MsAccess;
using Kros.Data.MsAccess;
using Kros.UnitTests;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Kros.Utils.UnitTests.Data.BulkActions
{
    public class MsAccessBulkInsertShould
    {

        #region Nested types

        public class DataItem
        {
            public int Id { get; set; }
            public string ColNote { get; set; }
            public byte? ColByte { get; set; }
            public Int16? ColShort { get; set; }
            public int? ColInt { get; set; }
            public float? ColSingle { get; set; }
            public double? ColDouble { get; set; }
            public decimal? ColCurrency { get; set; }
            public DateTime? ColDate { get; set; }
            public Guid? ColGuid { get; set; }
            public bool? ColBool { get; set; }
            public string ColShortText { get; set; }
            public string ColLongText { get; set; }
        }

        #endregion

        #region Constants

        private const string AccdbFileName = "MsAccessBulkInsert.accdb";
        private const string MdbFileName = "MsAccessBulkInsert.mdb";
        private const string TableName = "TestTable";

        private const float FloatMinimum = (float)-999999999999999999999.999999999999;
        private const float FloatMaximum = -(float)999999999999999999999.999999999999;
        private const double DoubleMinimum = FloatMinimum;
        private const double DoubleMaximum = FloatMaximum;
        private const decimal DecimalMinimum = (decimal)-99999999999999.9999999999;
        private const decimal DecimalMaximum = (decimal)99999999999999.9999999999;

        #endregion

        #region Tests

        [SkippableFact]
        public void BulkInsertDataFromDataTableIntoAccdb()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            using (var helper = CreateHelper(ProviderType.Ace, AccdbFileName))
            {
                DataTableBulkInsertCore(helper.Connection);
            }
        }

        [SkippableFact]
        public void BulkInsertDataFromDataTableIntoMdb() =>
            BulkInsertDataFromDataTableIntoMdbCore();

        [SkippableFact]
        public void BulkInsertDataFromDataTableIntoMdbSynchronouslyWithoutDeadLock() =>
            AsyncContext.Run(() => BulkInsertDataFromDataTableIntoMdbCore());

        private void BulkInsertDataFromDataTableIntoMdbCore()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            using (var helper = CreateHelper(ProviderType.Jet, MdbFileName))
            {
                DataTableBulkInsertCore(helper.Connection);
            }
        }

        [SkippableFact]
        public async Task BulkInsertDataFromDataTableIntoMdbAsynchronously()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            using (var helper = CreateHelper(ProviderType.Jet, MdbFileName))
            {
                DataTable expectedData = CreateDataTableDataSource();

                MsAccessBulkInsert bulkInsert = new MsAccessBulkInsert(helper.Connection);
                bulkInsert.DestinationTableName = TableName;
                await bulkInsert.InsertAsync(expectedData);

                DataTable actualData = LoadData(helper.Connection);

                MsAccessBulkHelper.CompareTables(actualData, expectedData);
            }
        }

        private void DataTableBulkInsertCore(OleDbConnection cn)
        {
            DataTable expectedData = CreateDataTableDataSource();

            MsAccessBulkInsert bulkInsert = new MsAccessBulkInsert(cn);
            bulkInsert.DestinationTableName = TableName;
            bulkInsert.Insert(expectedData);

            DataTable actualData = LoadData(cn);

            MsAccessBulkHelper.CompareTables(actualData, expectedData);
        }

        [SkippableFact]
        public void BulkInsertDataFromIBulkActionDataReaderIntoAccdb()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            using (var helper = CreateHelper(ProviderType.Ace, AccdbFileName))
            {
                IBulkActionDataReaderBulkInsertCore(helper.Connection);
            }
        }

        [SkippableFact]
        public void BulkInsertDataFromIBulkActionDataReaderIntoMdb() =>
            BulkInsertDataFromIBulkActionDataReaderIntoMdbCore();

        [SkippableFact]
        public void BulkInsertDataFromIBulkActionDataReaderIntoMdbSynchronouslyWithoutDeadLock() =>
            AsyncContext.Run(() => BulkInsertDataFromIBulkActionDataReaderIntoMdbCore());

        [SkippableFact]
        public async Task BulkInsertDataFromIBulkActionDataReaderIntoMdbAsynchronously()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            using (var helper = CreateHelper(ProviderType.Jet, MdbFileName))
            {
                DataTable expectedData = CreateDataTableDataSource();
                DataTable actualData = null;

                using (IBulkActionDataReader reader = CreateIDataReaderDataSource())
                {
                    MsAccessBulkInsert bulkInsert = new MsAccessBulkInsert(helper.Connection);
                    bulkInsert.DestinationTableName = TableName;
                    await bulkInsert.InsertAsync(reader);
                    actualData = LoadData(helper.Connection);
                }

                MsAccessBulkHelper.CompareTables(actualData, expectedData);
            }
        }

        private void BulkInsertDataFromIBulkActionDataReaderIntoMdbCore()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            using (var helper = CreateHelper(ProviderType.Jet, MdbFileName))
            {
                IBulkActionDataReaderBulkInsertCore(helper.Connection);
            }
        }

        private void IBulkActionDataReaderBulkInsertCore(OleDbConnection cn)
        {
            DataTable expectedData = CreateDataTableDataSource();
            DataTable actualData = null;

            using (IBulkActionDataReader reader = CreateIDataReaderDataSource())
            {
                MsAccessBulkInsert bulkInsert = new MsAccessBulkInsert(cn);
                bulkInsert.DestinationTableName = TableName;
                bulkInsert.Insert(reader);
                actualData = LoadData(cn);
            }

            MsAccessBulkHelper.CompareTables(actualData, expectedData);
        }

        [SkippableFact]
        public void BulkInsertDataFromIDataReaderIntoMdbSynchronouslyWithoutDeadLock()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();

            AsyncContext.Run(() =>
            {
                using (var helper = CreateHelper(ProviderType.Jet, MdbFileName))
                {
                    DataTable expectedData = CreateDataTableDataSource();
                    DataTable actualData = null;

                    using (IDataReader reader = expectedData.CreateDataReader())
                    {
                        MsAccessBulkInsert bulkInsert = new MsAccessBulkInsert(helper.Connection);
                        bulkInsert.DestinationTableName = TableName;
                        bulkInsert.Insert(reader);
                        actualData = LoadData(helper.Connection);
                    }

                    MsAccessBulkHelper.CompareTables(actualData, expectedData);
                }
            });
        }

        [SkippableFact]
        public async Task BulkInsertDataFromIDataReaderIntoMdbAsynchronously()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();

            using (var helper = CreateHelper(ProviderType.Jet, MdbFileName))
            {
                DataTable expectedData = CreateDataTableDataSource();
                DataTable actualData = null;

                using (IDataReader reader = expectedData.CreateDataReader())
                {
                    MsAccessBulkInsert bulkInsert = new MsAccessBulkInsert(helper.Connection);
                    bulkInsert.DestinationTableName = TableName;
                    await bulkInsert.InsertAsync(reader);
                    actualData = LoadData(helper.Connection);
                }

                MsAccessBulkHelper.CompareTables(actualData, expectedData);
            }
        }

        #endregion


        #region Helpers

        private static Dictionary<string, Dictionary<string, object>> _rawData = new Dictionary<string, Dictionary<string, object>>
        {
            {
                "ColByte",
                new Dictionary<string, object> {
                    { "Byte - 123", (byte)123 },
                    { "Byte - MinValue", byte.MinValue },
                    { "Byte - MaxValue", byte.MaxValue },
                }
            },
            {
                "ColShort",
                new Dictionary<string, object> {
                    { "Short - 123", (short)123 },
                    { "Short - MinValue", short.MinValue },
                    { "Short - MaxValue", short.MaxValue },
                }
            },
            {
                "ColInt",
                new Dictionary<string, object> {
                    { "Int - 123", 123 },
                    { "Int - MinValue", int.MinValue },
                    { "Int - MaxValue", int.MaxValue },
                }
            },
            {
                "ColSingle",
                new Dictionary<string, object> {
                    { "Single - 123.456", (float)123456.654321 },
                    { "Single - MinValue", FloatMinimum },
                    { "Single - MaxValue", FloatMaximum },
                }
            },
            {
                "ColDouble",
                new Dictionary<string, object> {
                    { "Double - 123.456", 123456.654321 },
                    { "Double - MinValue", DoubleMinimum },
                    { "Double - MaxValue", DoubleMaximum },
                }
            },
            {
                "ColCurrency",
                new Dictionary<string, object> {
                    { "Currency - 123.456", (decimal)123456.6543 },
                    { "Currency - MinValue", DecimalMinimum },
                    { "Currency - MaxValue", DecimalMaximum },
                }
            },
            {
                "ColDate",
                new Dictionary<string, object> {
                    { "DateTime - 10.12.1978 7:30:59", new DateTime(1978, 12, 10, 7, 30, 59) },
                    { "DateTime - 1.1.1900", new DateTime(1900,1,1) },
                    { "DateTime - MaxValue", DateTime.MaxValue },
                }
            },
            {
                "ColGuid",
                new Dictionary<string, object> {
                    { "Guid", new Guid("abcdef00-1234-5678-9000-abcdefabcdef") },
                }
            },
            {
                "ColBool",
                new Dictionary<string, object> {
                    { "Bool - True", true },
                    { "Bool - False", false },
                }
            },
            {
                "ColShortText",
                new Dictionary<string, object> {
                    { "ShortText", "lorem ipsum" },
                }
            },
            {
                "ColLongText",
                new Dictionary<string, object> {
                    {
                        "LongText",
                        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ut ullamcorper nisl. Nulla iaculis " +
                        "scelerisque dui ut molestie. Suspendisse potenti. In hac habitasse platea dictumst. Maecenas " +
                        "pellentesque ante tortor, vitae pellentesque dolor eleifend sed. Cras a commodo arcu. Nulla " +
                        "convallis vulputate quam, vel lobortis mauris feugiat nec. Nullam tincidunt, sapien eu cursus varius, " +
                        "metus lacus ultrices leo, eu accumsan sem lacus nec elit. Vestibulum ac felis vitae odio interdum " +
                        "ullamcorper."
                    },
                }
            },
        };

        private MsAccessTestHelper CreateHelper(ProviderType provider, string fileName)
        {
            string resourceName = Helpers.RootNamespaceResources + "." + fileName;
            Stream sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            return new MsAccessTestHelper(provider, sourceStream);
        }

        private DataTable LoadData(OleDbConnection cn)
        {
            DataTable data = new DataTable(TableName);

            using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM {TableName}", cn))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
            {
                adapter.FillSchema(data, SchemaType.Source);
                adapter.Fill(data);
            }

            return data;
        }

        private DataTable CreateDataTableDataSource()
        {
            DataTable table = CreateBulkInsertDataTable();
            FillBulkInsertDataTable(table);

            return table;
        }

        private DataTable CreateBulkInsertDataTable()
        {
            DataTable table = new DataTable("data");

            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("ColNote", typeof(string));
            table.Columns.Add("ColByte", typeof(byte));
            table.Columns.Add("ColShort", typeof(short));
            table.Columns.Add("ColInt", typeof(int));
            table.Columns.Add("ColSingle", typeof(float));
            table.Columns.Add("ColDouble", typeof(double));
            table.Columns.Add("ColCurrency", typeof(decimal));
            table.Columns.Add("ColDate", typeof(DateTime));
            table.Columns.Add("ColGuid", typeof(Guid));
            table.Columns.Add("ColBool", typeof(bool));
            table.Columns.Add("ColShortText", typeof(string));
            table.Columns.Add("ColLongText", typeof(string));

            table.PrimaryKey = new DataColumn[] { table.Columns["Id"] };

            return table;
        }

        private void FillBulkInsertDataTable(DataTable table)
        {
            int id = 1;
            foreach (KeyValuePair<string, Dictionary<string, object>> rawItem in _rawData)
            {
                AddBulkInsertDataRow(table, ref id, rawItem.Key, rawItem.Value);
            }
        }

        private void AddBulkInsertDataRow(
            DataTable table,
            ref int id,
            string columnName,
            IDictionary<string, object> columnValues)
        {
            foreach (KeyValuePair<string, object> data in columnValues)
            {
                DataRow row = table.NewRow();

                // Preddefinovanie hodnôt stĺpcov - v DataTable sú NULL hodnoty, ale Access to ako NULL nevloží.
                row["ColBool"] = false;
                row["ColShortText"] = string.Empty;
                row["ColLongText"] = string.Empty;

                row["Id"] = id;
                row["ColNote"] = data.Key;
                row[columnName] = data.Value;
                table.Rows.Add(row);
                id++;
            }
        }

        private IBulkActionDataReader CreateIDataReaderDataSource()
        {
            List<DataItem> data = new List<DataItem>();
            List<string> columnNames = new List<string>(new string[] { "Id", "ColNote" });
            int id = 1;

            foreach (KeyValuePair<string, Dictionary<string, object>> rawItem in _rawData)
            {
                AddBulkInsertDataItem(data, ref id, rawItem.Key, rawItem.Value);
                columnNames.Add(rawItem.Key);
            }

            return new EnumerableDataReader<DataItem>(data, columnNames);
        }

        private void AddBulkInsertDataItem(
            List<DataItem> data,
            ref int id,
            string columnName,
            IDictionary<string, object> columnValues)
        {
            foreach (KeyValuePair<string, object> value in columnValues)
            {
                DataItem item = new DataItem()
                {
                    Id = id,
                    ColNote = value.Key,
                };
                typeof(DataItem).GetProperty(columnName).SetValue(item, value.Value);
                data.Add(item);
                id++;
            }
        }

        #endregion

    }
}
