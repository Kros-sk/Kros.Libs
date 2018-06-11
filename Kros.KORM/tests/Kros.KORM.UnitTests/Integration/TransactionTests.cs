using FluentAssertions;
using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query;
using Kros.KORM.UnitTests.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Kros.KORM.UnitTests.Integration
{
    public partial class TransactionTests : DatabaseTestBase
    {
        #region SQL Scripts

        private static string CreateTable_TestTable =
$@"CREATE TABLE [dbo].[Invoices](
    [Id] [int] NOT NULL,
    [Code] [nvarchar](10) NOT NULL,
    [Description] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )
) ON [PRIMARY]
";

        private static string CreateProcedure_WaitForTwoSeconds =
$@" CREATE PROCEDURE [dbo].[WaitForTwoSeconds] AS 
    BEGIN
        SET NOCOUNT ON;
        WAITFOR DELAY '00:00:02';
    END";

        #endregion


        #region Tests

        [Fact]
        public void ImplicitTransactionShould_CommitData()
        {
            using (var korm = CreateDatabase())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkInsertWasCalled()
        {
            ImplicitTransactionShould_CommitDataWhenBulkInsertWasCalledCore(BulkInsertAddItems);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkInsertEnumerableWasCalled()
        {
            ImplicitTransactionShould_CommitDataWhenBulkInsertWasCalledCore(BulkInsertEnumerableItems);
        }

        private void ImplicitTransactionShould_CommitDataWhenBulkInsertWasCalledCore(Action<IDbSet<Invoice>> action)
        {
            using (var korm = CreateDatabase())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                action.Invoke(dbSet);

                DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ImplicitTransactionShould_BulkInsertThrowsException()
        {
            using (var korm = CreateDatabase())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                Action bulkInsertAction = () => dbSet.BulkInsert(null);
                bulkInsertAction.Should().Throw<ArgumentNullException>();
            }
        }

        [Fact]
        public void ExplicitTransactionShould_CommitData()
        {
            using (var korm = CreateDatabase())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                transaction.Commit();

                DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkInsertWasCalled()
        {
            ExplicitTransactionShould_CommitDataWhenBulkInsertWasCalledCore(BulkInsertAddItems);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkInsertEnumerableWasCalled()
        {
            ExplicitTransactionShould_CommitDataWhenBulkInsertWasCalledCore(BulkInsertEnumerableItems);
        }

        private void ExplicitTransactionShould_CommitDataWhenBulkInsertWasCalledCore(Action<IDbSet<Invoice>> action)
        {
            using (var korm = CreateDatabase())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                action.Invoke(dbSet);

                transaction.Commit();

                DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackData()
        {
            using (var korm = CreateDatabase())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                transaction.Rollback();

                DatabaseShouldBeEmpty(korm);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataWhenBulkInsertWasCalled()
        {
            ExplicitTransactionShould_RollbackDataWhenBulkInsertWasCalledCore(BulkInsertAddItems);
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataWhenBulkInsertEnumerableWasCalled()
        {
            ExplicitTransactionShould_RollbackDataWhenBulkInsertWasCalledCore(BulkInsertEnumerableItems);
        }

        private void ExplicitTransactionShould_RollbackDataWhenBulkInsertWasCalledCore(Action<IDbSet<Invoice>> action)
        {
            using (var korm = CreateDatabase())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                action.Invoke(dbSet);

                transaction.Rollback();

                DatabaseShouldBeEmpty(korm);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkInsertCommitWasNotCalled()
        {
            ExplicitTransactionShould_NotChangeDataWhenBulkInsertCommitWasNotCalledCore(BulkInsertAddItems);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkInsertEnumerableCommitWasNotCalled()
        {
            ExplicitTransactionShould_NotChangeDataWhenBulkInsertCommitWasNotCalledCore(BulkInsertEnumerableItems);
        }

        private void ExplicitTransactionShould_NotChangeDataWhenBulkInsertCommitWasNotCalledCore(
            Action<IDbSet<Invoice>> action)
        {
            using (var korm = CreateDatabase())
            {
                using (var transaction = korm.BeginTransaction())
                {
                    var dbSet = korm.Query<Invoice>().AsDbSet();

                    action.Invoke(dbSet);
                }
                DatabaseShouldBeEmpty(korm);
            }
        }

        [Fact]
        public void DataShould_BeAccessibleFromTransaction()
        {
            using (var korm = CreateDatabase())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                DatabaseShouldContainInvoices(korm, CreateTestData());

                transaction.Rollback();

                korm.Query<Invoice>().Should().BeEmpty();
                DatabaseShouldBeEmpty(korm);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackMultipleCommit()
        {
            using (var korm = CreateDatabase())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                DatabaseShouldContainInvoices(korm, CreateTestData());

                dbSet.Add(new Invoice() { Id = 4, Code = "0004", Description = "Item 4" });
                dbSet.CommitChanges();

                transaction.Rollback();

                korm.Query<Invoice>().Should().BeEmpty();
                DatabaseShouldBeEmpty(korm);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenCommitWasCall()
        {
            using (var database = CreateDatabase())
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                transaction.Commit();

                database.Connection.State.Should().Be(ConnectionState.Open);
                DatabaseShouldContainInvoices(database.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenRollbackWasCall()
        {
            using (var database = CreateDatabase())
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                transaction.Rollback();

                database.Connection.State.Should().Be(ConnectionState.Open);
                korm.Query<Invoice>().Should().BeEmpty();
                DatabaseShouldBeEmpty(database);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenRollbackWasCalledAfterBulkInsert()
        {
            using (var database = CreateDatabase())
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.BulkInsert();

                transaction.Rollback();

                database.Connection.State.Should().Be(ConnectionState.Open);
                korm.Query<Invoice>().Should().BeEmpty();
                DatabaseShouldBeEmpty(database);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenCommitWasCalledAfterBulkInsert()
        {
            using (var database = CreateDatabase())
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.BulkInsert();

                transaction.Commit();

                database.Connection.State.Should().Be(ConnectionState.Open);
                DatabaseShouldContainInvoices(database.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_ThrowCommandTimeoutExceptionWhenIsSetTooSmall()
        {
            using (var database = CreateAndInitDatabase(CreateProcedure_WaitForTwoSeconds))
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var transaction = korm.BeginTransaction())
            {
                transaction.CommandTimeout = 1;

                string sql = @"EXEC WaitForTwoSeconds";
                Action commit = () => { korm.ExecuteScalar(sql); };

                commit.Should().Throw<SqlException>().Which.Message.Contains("Timeout");
            }
        }

        [Fact]
        public void ExplicitTransactionShould_ThrowInvalidOperationExceptionWhenCommandTimeoutSetForNestedTransaction()
        {
            using (var database = CreateAndInitDatabase(CreateProcedure_WaitForTwoSeconds))
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var mainTransaction = korm.BeginTransaction())
            {
                using (var nestedTransaction = korm.BeginTransaction())
                {
                    mainTransaction.CommandTimeout = 1;
                    Action setCommandTimeout = () => { nestedTransaction.CommandTimeout = 3; };

                    setCommandTimeout.Should().Throw<InvalidOperationException>();
                }
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotThrowCommandTimeoutExceptionWhenIsSetSufficient()
        {
            using (var database = CreateAndInitDatabase(CreateProcedure_WaitForTwoSeconds))
            using (var korm = new Database(database.ConnectionString, "System.Data.SqlClient"))
            using (var transaction = korm.BeginTransaction())
            {
                transaction.CommandTimeout = 3;
                Action commit = () => { korm.ExecuteStoredProcedure<Object>("WaitForTwoSeconds"); };

                commit.Should().NotThrow<SqlException>();
            }
        }

        #endregion


        #region Helpers

        private void DatabaseShouldContainInvoices(Database korm, IEnumerable<Invoice> expected)
        {
            korm.Query<Invoice>().Should().BeEquivalentTo(expected);
        }

        private void DatabaseShouldBeEmpty(TestDatabase korm)
        {
            DatabaseShouldContainInvoices(korm.ConnectionString, new List<Invoice>());
        }

        private void DatabaseShouldContainInvoices(string connectionString, IEnumerable<Invoice> expected)
        {
            using (var korm = new Database(connectionString, "System.Data.SqlClient"))
            {
                DatabaseShouldContainInvoices(korm, expected);
            }
        }

        private IEnumerable<Invoice> CreateTestData() =>
            new List<Invoice>() {
                new Invoice() { Id = 1, Code = "0001", Description = "Item 1"},
                new Invoice() { Id = 2, Code = "0002", Description = "Item 2"},
                new Invoice() { Id = 3, Code = "0002", Description = "Item 3"} };

        private TestDatabase CreateDatabase() => CreateDatabase(CreateTable_TestTable) as DatabaseTestBase.TestDatabase;
        private TestDatabase CreateAndInitDatabase(string initScript)
            => CreateDatabase(CreateTable_TestTable, initScript) as DatabaseTestBase.TestDatabase;

        private void BulkInsertAddItems(IDbSet<Invoice> dbSet)
        {
            dbSet.Add(CreateTestData());
            dbSet.BulkInsert();
        }

        private void BulkInsertEnumerableItems(IDbSet<Invoice> dbSet)
        {
            dbSet.BulkInsert(CreateTestData());
        }

        [Alias("Invoices")]
        public class Invoice
        {
            public int Id { get; set; }

            public string Code { get; set; }

            public string Description { get; set; }
        }

        #endregion

    }
}
