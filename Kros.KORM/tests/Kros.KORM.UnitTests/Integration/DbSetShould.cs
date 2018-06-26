using FluentAssertions;
using Kros.Data.SqlServer;
using Kros.KORM.Converter;
using Kros.KORM.Metadata;
using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query;
using Kros.KORM.UnitTests.Base;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kros.KORM.UnitTests.Integration
{
    public class DbSetShould : DatabaseTestBase
    {
        #region Nested Classes

        [Alias("LimitOffsetTest")]
        private class LimitOffsetTestData
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        [Alias("People")]
        private class Person
        {
            [Key(AutoIncrementMethodType.Custom)]
            public int Id { get; set; }

            public int Age { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            [Converter(typeof(AddressConverter))]
            public List<string> Address { get; set; }
        }

        [Alias("People")]
        private class Foo
        {
            [Key(AutoIncrementMethodType.None)]
            public int Id { get; set; }
        }

        private class AddressConverter : IConverter
        {
            public object Convert(object value) =>
                value != null ? value.ToString().Split('#').ToList() : new List<string>();

            public object ConvertBack(object value) =>
                value is List<string> address && address.Count > 0 ? string.Join("#", address) : null;
        }

        #endregion

        #region SQL Scripts

        private const string Table_TestTable = "People";

        private static string CreateTable_TestTable =
$@"CREATE TABLE[dbo].[{Table_TestTable}] (
    [Id] [int] NOT NULL,
    [Age] [int] NULL,
    [FirstName] [nvarchar] (50) NULL,
    [LastName] [nvarchar] (50) NULL,
    [Address] [nvarchar] (50) NULL
) ON[PRIMARY];";

        private static string InsertDataScript =
$@"INSERT INTO {Table_TestTable} VALUES (1, 18, 'John', 'Smith', 'London');
INSERT INTO {Table_TestTable} VALUES (1, 22, 'Kilie', 'Bistrol', 'London');";

        private const string Table_LimitOffsetTest = "LimitOffsetTest";

        private static string CreateTable_LimitOffsetTest =
$@"CREATE TABLE[dbo].[{Table_LimitOffsetTest}] (
    [Id] [int] NOT NULL,
    [Value] [nvarchar](50) NULL
) ON[PRIMARY];";

        private static string InsertLimitOffsetDataScript =
$@"INSERT INTO {Table_LimitOffsetTest} VALUES (1, 'one');
INSERT INTO {Table_LimitOffsetTest} VALUES (2, 'two');
INSERT INTO {Table_LimitOffsetTest} VALUES (3, 'three');
INSERT INTO {Table_LimitOffsetTest} VALUES (4, 'four');
INSERT INTO {Table_LimitOffsetTest} VALUES (5, 'fice');
INSERT INTO {Table_LimitOffsetTest} VALUES (6, 'six');
INSERT INTO {Table_LimitOffsetTest} VALUES (7, 'seven');
INSERT INTO {Table_LimitOffsetTest} VALUES (8, 'eight');
INSERT INTO {Table_LimitOffsetTest} VALUES (9, 'nine');
INSERT INTO {Table_LimitOffsetTest} VALUES (10, 'ten');
INSERT INTO {Table_LimitOffsetTest} VALUES (11, 'eleven');
INSERT INTO {Table_LimitOffsetTest} VALUES (12, 'twelve');
INSERT INTO {Table_LimitOffsetTest} VALUES (13, 'thirteen');
INSERT INTO {Table_LimitOffsetTest} VALUES (14, 'fourteen');
INSERT INTO {Table_LimitOffsetTest} VALUES (15, 'fifteen');
INSERT INTO {Table_LimitOffsetTest} VALUES (16, 'sixteen');
INSERT INTO {Table_LimitOffsetTest} VALUES (17, 'seventeen');
INSERT INTO {Table_LimitOffsetTest} VALUES (18, 'eighteen');
INSERT INTO {Table_LimitOffsetTest} VALUES (19, 'nineteen');
INSERT INTO {Table_LimitOffsetTest} VALUES (20, 'twenty');";

        #endregion

        #region Insert Data

        [Fact]
        public void InsertData()
        {
            InsertDataCore();
        }

        [Fact]
        public void InsertDataSynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                InsertDataCore();
            });
        }

        [Fact]
        public async Task InsertDataAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable))
            {
                IDbSet<Person> dbSet = GetDbSetForCommitInsert(korm);

                await dbSet.CommitChangesAsync();

                AssertData(korm);
            }
        }

        private void InsertDataCore()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable))
            {
                IDbSet<Person> dbSet = GetDbSetForCommitInsert(korm);

                dbSet.CommitChanges();

                AssertData(korm);
            }
        }

        private static IDbSet<Person> GetDbSetForCommitInsert(IDatabase korm)
        {
            var dbSet = korm.Query<Person>().AsDbSet();

            dbSet.Add(GetData());
            return dbSet;
        }

        #endregion

        #region Update data

        [Fact]
        public void UpdateData()
        {
            UpdateDataCore();
        }

        [Fact]
        public void UpdateDataSynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                UpdateDataCore();
            });
        }

        [Fact]
        public async Task UpdateDataAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = GetDbSetForUpdate(korm);

                await dbSet.CommitChangesAsync();

                AssertData(korm);
            }
        }

        private void UpdateDataCore()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = GetDbSetForUpdate(korm);

                dbSet.CommitChanges();

                AssertData(korm);
            }
        }

        private static IDbSet<Person> GetDbSetForUpdate(IDatabase korm)
        {
            var dbSet = korm.Query<Person>().AsDbSet();

            dbSet.Edit(GetData());
            return dbSet;
        }

        #endregion

        #region Delete Data

        [Fact]
        public void DeleteData()
        {
            DeleteDataCore();
        }

        [Fact]
        public void DeleteDataSynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                DeleteDataCore();
            });
        }

        [Fact]
        public async Task DeleteDataAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = korm.Query<Person>().AsDbSet();
                dbSet.Delete(new List<Person>() {
                    new Person() { Id = 1 },
                    new Person() { Id = 2 } });

                await dbSet.CommitChangesAsync();

                korm.Query<Person>().Count().Should().Be(0);
            }
        }

        private void DeleteDataCore()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = korm.Query<Person>().AsDbSet();
                dbSet.Delete(new List<Person>() {
                    new Person() { Id = 1 },
                    new Person() { Id = 2 } });

                dbSet.CommitChanges();

                korm.Query<Person>().Count().Should().Be(0);
            }
        }

        #endregion

        #region Bulk Insert

        [Fact]
        public async Task BulkInsertDataAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable))
            {
                IDbSet<Person> dbSet = korm.Query<Person>().AsDbSet();
                dbSet.Add(GetData());

                await dbSet.BulkInsertAsync();

                AssertData(korm);
            }
        }

        [Fact]
        public async Task BulkInsertDataDirectlyAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable))
            {
                IDbSet<Person> dbSet = korm.Query<Person>().AsDbSet();

                await dbSet.BulkInsertAsync(GetData());

                AssertData(korm);
            }
        }

        [Fact]
        public void BulkInsertDataSynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                using (var korm = CreateDatabase(CreateTable_TestTable))
                {
                    IDbSet<Person> dbSet = korm.Query<Person>().AsDbSet();
                    dbSet.Add(GetData());

                    dbSet.BulkInsert();

                    AssertData(korm);
                }
            });
        }

        [Fact]
        public void BulkInsertDataDirectlySynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                using (var korm = CreateDatabase(CreateTable_TestTable))
                {
                    IDbSet<Person> dbSet = korm.Query<Person>().AsDbSet();

                    dbSet.BulkInsert(GetData());

                    AssertData(korm);
                }
            });
        }

        #endregion

        #region Bulk Update

        [Fact]
        public void BulkUpdateDataSynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
                {
                    var dbSet = korm.Query<Person>().AsDbSet();

                    dbSet.Edit(GetData());

                    dbSet.BulkUpdate();

                    AssertData(korm);
                }
            });
        }

        [Fact]
        public async Task BulkUpdateDataAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                dbSet.Edit(GetData());

                await dbSet.BulkUpdateAsync();

                AssertData(korm);
            }
        }

        [Fact]
        public void BulkUpdateDataWithActionSynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
                {
                    var dbSet = korm.Query<Person>().AsDbSet();

                    dbSet.Edit(GetData());

                    dbSet.BulkUpdate((c, t, s) => { });

                    AssertData(korm);
                }
            });
        }

        [Fact]
        public async Task BulkUpdateDataWithActionAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                dbSet.Edit(GetData());

                await dbSet.BulkUpdateAsync((c, t, s) => { });

                AssertData(korm);
            }
        }

        [Fact]
        public void BulkUpdateDataDirectlySynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
                {
                    var dbSet = korm.Query<Person>().AsDbSet();

                    dbSet.BulkUpdate(GetData());

                    AssertData(korm);
                }
            });
        }

        [Fact]
        public async Task BulkUpdateDataDirectlyAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                await dbSet.BulkUpdateAsync(GetData());

                AssertData(korm);
            }
        }

        [Fact]
        public void BulkUpdateDataWithActionDirectlySynchronouslyWithoutDeadLock()
        {
            AsyncContext.Run(() =>
            {
                using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
                {
                    var dbSet = korm.Query<Person>().AsDbSet();

                    dbSet.BulkUpdate(GetData(), (c, t, s) => { });

                    AssertData(korm);
                }
            });
        }

        [Fact]
        public async Task BulkUpdateDataWithActionDirectlyAsync()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable, InsertDataScript))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                await dbSet.BulkUpdateAsync(GetData(), (c, t, s) => { });

                AssertData(korm);
            }
        }

        private static IEnumerable<Person> GetData()
        {
            var data = new List<Person>();

            data.Add(new Person()
            {
                Id = 1,
                FirstName = "Milan",
                LastName = "Martiniak",
                Age = 32,
                Address = new List<string>() { "Petzvalova", "Pekna", "Zelena" }
            });

            data.Add(new Person()
            {
                Id = 2,
                FirstName = "Peter",
                LastName = "Juráček",
                Age = 14,
                Address = new List<string>() { "Novozámocká" }
            });

            return data;
        }

        #endregion

        #region Primary Keys

        [Fact]
        public void GeneratePrimaryKey()
        {
            OnGeneratePrimaryKey(dbSet => dbSet.CommitChanges());
        }

        [Fact]
        public void GeneratePrimaryKeyWhenBulkInsertIsCall()
        {
            OnGeneratePrimaryKey(dbSet => dbSet.BulkInsert());
        }

        private void OnGeneratePrimaryKey(Action<IDbSet<Person>> commitAction)
        {
            using (var korm = CreateDatabase(CreateTable_TestTable,
                            SqlServerIdGenerator.GetIdStoreTableCreationScript(),
                            SqlServerIdGenerator.GetStoredProcedureCreationScript()))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                var sourcePeople = new List<Person>() {
                    new Person() { FirstName = "Milan" },
                    new Person() { FirstName = "Peter" },
                    new Person() { FirstName = "Milada" }
                };

                dbSet.Add(sourcePeople);

                commitAction(dbSet);

                var id = 1;
                foreach (var item in sourcePeople)
                {
                    item.Id.Should().Be(id++);
                }

                var people = korm.Query<Person>().OrderBy(p => p.Id);

                var sourceEnumerator = sourcePeople.GetEnumerator();
                id = 1;
                foreach (var item in people)
                {
                    sourceEnumerator.MoveNext();
                    var source = sourceEnumerator.Current;

                    item.Id.Should().Be(id++);
                    item.FirstName.Should().Be(source.FirstName);
                }
            }
        }

        [Fact]
        public void DoNotGeneratePrimaryKeyIfFilled()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable,
               SqlServerIdGenerator.GetIdStoreTableCreationScript(),
               SqlServerIdGenerator.GetStoredProcedureCreationScript()))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                var sourcePeople = new List<Person>() {
                    new Person() { Id = 5,  FirstName = "Milan" },
                    new Person() { Id = 7, FirstName = "Peter" },
                    new Person() { Id = 9, FirstName = "Milada" }
                };

                dbSet.Add(sourcePeople);

                dbSet.CommitChanges();

                var id = 5;
                foreach (var item in sourcePeople)
                {
                    item.Id.Should().Be(id);
                    id += 2;
                }

                var people = korm.Query<Person>().OrderBy(p => p.Id);

                var sourceEnumerator = sourcePeople.GetEnumerator();
                id = 5;
                foreach (var item in people)
                {
                    sourceEnumerator.MoveNext();
                    var source = sourceEnumerator.Current;

                    item.Id.Should().Be(id);
                    item.FirstName.Should().Be(source.FirstName);
                    id += 2;
                }
            }
        }

        [Fact]
        public void DoNotGeneratePrimaryKeyIfKeyIsNotAutoIncrement()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable,
               SqlServerIdGenerator.GetIdStoreTableCreationScript(),
               SqlServerIdGenerator.GetStoredProcedureCreationScript()))
            {
                var dbSet = korm.Query<Foo>().AsDbSet();

                var sourcePeople = new List<Foo>() {
                    new Foo(),
                    new Foo(),
                    new Foo(),
                };

                dbSet.Add(sourcePeople);

                dbSet.CommitChanges();

                sourcePeople.Select(p => p.Id).Should().BeEquivalentTo(new int[] { 0, 0, 0 });

                var people = korm.Query<Person>().AsEnumerable();
                people.Select(p => p.Id).Should().BeEquivalentTo(new int[] { 0, 0, 0 });
            }
        }

        [Fact]
        public void IteratedThroughItemsOnlyOnceWhenGeneratePrimaryKeys()
        {
            using (var korm = CreateDatabase(CreateTable_TestTable,
               SqlServerIdGenerator.GetIdStoreTableCreationScript(),
               SqlServerIdGenerator.GetStoredProcedureCreationScript()))
            {
                var dbSet = korm.Query<Person>().AsDbSet();

                var iterationCount = 0;
                IEnumerable<Person> SourceItems()
                {
                    iterationCount++;
                    yield return new Person() { Id = 5, FirstName = "Milan" };
                }
                var sourcePeople = SourceItems();

                dbSet.BulkInsert(sourcePeople);

                iterationCount.Should().Be(1);
            }
        }

        #endregion

        #region Limit/Offset

        [Fact]
        public void ReturnOnlyFirstNRows()
        {
            using (IDatabase korm = CreateDatabase(CreateTable_LimitOffsetTest, InsertLimitOffsetDataScript))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() {  Id = 1, Value = "one" },
                    new LimitOffsetTestData() {  Id = 2, Value = "two" },
                    new LimitOffsetTestData() {  Id = 3, Value = "three" }
                });

                List<LimitOffsetTestData> data = korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Take(3)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [Fact]
        public void SkipFirstNRows()
        {
            using (IDatabase korm = CreateDatabase(CreateTable_LimitOffsetTest, InsertLimitOffsetDataScript))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() {  Id = 18, Value = "eighteen" },
                    new LimitOffsetTestData() {  Id = 19, Value = "nineteen" },
                    new LimitOffsetTestData() {  Id = 20, Value = "twenty" }
                });

                List<LimitOffsetTestData> data = korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(17)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [Fact]
        public void SkipFirstNRowsAndReturnNextMRows()
        {
            using (IDatabase korm = CreateDatabase(CreateTable_LimitOffsetTest, InsertLimitOffsetDataScript))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() {  Id = 6, Value = "six" },
                    new LimitOffsetTestData() {  Id = 7, Value = "seven" },
                    new LimitOffsetTestData() {  Id = 8, Value = "eight" }
                });

                List<LimitOffsetTestData> data = korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(5)
                    .Take(3)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [Fact]
        public void ReturnNoRowsWhenSkipIsTooBig()
        {
            using (IDatabase korm = CreateDatabase(CreateTable_LimitOffsetTest, InsertLimitOffsetDataScript))
            {
                var expectedData = new List<LimitOffsetTestData>();

                List<LimitOffsetTestData> data = korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(100)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [Fact]
        public void ReturnAllRemainigRowsWhenTakeIsTooBig()
        {
            using (IDatabase korm = CreateDatabase(CreateTable_LimitOffsetTest, InsertLimitOffsetDataScript))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() {  Id = 19, Value = "nineteen" },
                    new LimitOffsetTestData() {  Id = 20, Value = "twenty" },
                });

                List<LimitOffsetTestData> data = korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(18)
                    .Take(100)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        #endregion

        #region Helpers

        private static void AssertData(IDatabase korm)
        {
            var person = korm.Query<Person>().FirstOrDefault(p => p.Id == 1);

            person.Should().NotBeNull();
            person.Id.Should().Be(1);
            person.Age.Should().Be(32);
            person.FirstName.Should().Be("Milan");
            person.LastName.Should().Be("Martiniak");
            person.Address.Should().BeEquivalentTo(new List<string>() { "Petzvalova", "Pekna", "Zelena" });
        }

        #endregion
    }
}