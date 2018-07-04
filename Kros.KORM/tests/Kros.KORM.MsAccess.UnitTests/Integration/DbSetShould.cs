using FluentAssertions;
using Kros.Data.MsAccess;
using Kros.KORM.Converter;
using Kros.KORM.Metadata;
using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query;
using Kros.UnitTests;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests.Integration
{
    public class DbSetShould
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

            public string TestLongText { get; set; }
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

        private const string CreateTable_TestTable =
            "CREATE TABLE [People] (\n" +
            "    [Id] LONG NOT NULL,\n" +
            "    [Age] LONG NULL,\n" +
            "    [FirstName] VARCHAR(50) NULL,\n" +
            "    [LastName] VARCHAR(50) NULL,\n" +
            "    [Address] VARCHAR(50) NULL,\n" +
            "    [TestLongText] LONGTEXT NULL\n" +
            ");";

        #endregion

        #region Insert Data

        [SkippableFact]
        public void InsertData_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            InsertDataCore(ProviderType.Ace);
        }

        [SkippableFact]
        public void InsertData_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            InsertDataCore(ProviderType.Jet);
        }

        [SkippableFact]
        public void InsertDataSynchronouslyWithoutDeadLock_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            AsyncContext.Run(() =>
            {
                InsertDataCore(ProviderType.Ace);
            });
        }

        [SkippableFact]
        public void InsertDataSynchronouslyWithoutDeadLock_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            AsyncContext.Run(() =>
            {
                InsertDataCore(ProviderType.Jet);
            });
        }

        [SkippableFact]
        public async Task InsertDataAsync_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            await InsertDataAsyncCore(ProviderType.Ace);
        }

        [SkippableFact]
        public async Task InsertDataAsync_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            await InsertDataAsyncCore(ProviderType.Jet);
        }

        private async Task InsertDataAsyncCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, CreateTable_TestTable))
            {
                IDbSet<Person> dbSet = GetDbSetForCommitInsert(helper.Korm);

                await dbSet.CommitChangesAsync();

                AssertData(helper.Korm);
            }
        }

        private void InsertDataCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, CreateTable_TestTable))
            {
                IDbSet<Person> dbSet = GetDbSetForCommitInsert(helper.Korm);

                dbSet.CommitChanges();

                AssertData(helper.Korm);
            }
        }

        private static IDbSet<Person> GetDbSetForCommitInsert(IDatabase korm)
        {
            var dbSet = korm.Query<Person>().AsDbSet();
            dbSet.Add(GetData());
            return dbSet;
        }

        #endregion

        #region Helpers

        protected MsAccessTestHelper CreateDatabase(ProviderType provider, params string[] initDatabaseScripts)
            => new MsAccessTestHelper(provider, initDatabaseScripts);

        private static IEnumerable<Person> GetData()
        {
            var data = new List<Person>();

            data.Add(new Person()
            {
                Id = 1,
                FirstName = "Milan",
                LastName = "Martiniak",
                Age = 32,
                Address = new List<string>() { "Petzvalova", "Pekna", "Zelena" },
                TestLongText = "Lorem ipsum dolor sit amet 1."
            });

            data.Add(new Person()
            {
                Id = 2,
                FirstName = "Peter",
                LastName = "Juráček",
                Age = 14,
                Address = new List<string>() { "Novozámocká" },
                TestLongText = "Lorem ipsum dolor sit amet 2."
            });

            return data;
        }

        private static void AssertData(IDatabase korm)
        {
            var person = korm.Query<Person>().FirstOrDefault(p => p.Id == 1);

            person.Should().NotBeNull();
            person.Id.Should().Be(1);
            person.Age.Should().Be(32);
            person.FirstName.Should().Be("Milan");
            person.LastName.Should().Be("Martiniak");
            person.Address.Should().BeEquivalentTo(new List<string>() { "Petzvalova", "Pekna", "Zelena" });
            person.TestLongText.Should().Be("Lorem ipsum dolor sit amet 1.");
        }

        #endregion
    }
}
