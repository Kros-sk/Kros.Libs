﻿using FluentAssertions;
using Kros.Data.SqlServer;
using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query;
using Kros.KORM.UnitTests.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        public void ImplicitTransactionShould_CommitDataOnClosedConnection()
        {
            DoTestOnClosedConnection(ImplicitTransactionCommitData, CreateDatabase);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataOnOpenedConnection()
        {
            DoTestOnOpenedConnection(ImplicitTransactionCommitData, CreateDatabase);
        }

        private void ImplicitTransactionCommitData(TestDatabase korm)
        {
            var dbSet = korm.Query<Invoice>().AsDbSet();

            dbSet.Add(CreateTestData());
            dbSet.CommitChanges();

            DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkInsertWasCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection(ImplicitTransactionBulkInsert, CreateDatabase);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkInsertWasCalledOnClosedConnection()
        {
            DoTestOnClosedConnection(ImplicitTransactionBulkInsert, CreateDatabase);
        }

        private void ImplicitTransactionBulkInsert(TestDatabase database)
        {
            ImplicitTransactionBulkInsertCommit(database, BulkInsertAddItems);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkInsertEnumerableWasCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection((db) => ImplicitTransactionBulkInsertCommit(db, BulkInsertEnumerableItems), CreateDatabase);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkInsertEnumerableWasCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ImplicitTransactionBulkInsertCommit(db, BulkInsertEnumerableItems), CreateDatabase);
        }

        private void ImplicitTransactionBulkInsertCommit(TestDatabase korm, Action<IDbSet<Invoice>> action)
        {
            var dbSet = korm.Query<Invoice>().AsDbSet();

            action.Invoke(dbSet);

            DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
        }

        [Fact]
        public void ImplicitTransactionShould_BulkInsertThrowsExceptionOnOpenedConnection()
        {
            DoTestOnOpenedConnection(ImplicitTransactionBulkInsertThrowsException, CreateDatabase);
        }

        [Fact]
        public void ImplicitTransactionShould_BulkInsertThrowsExceptionOnClosedConnection()
        {
            DoTestOnClosedConnection(ImplicitTransactionBulkInsertThrowsException, CreateDatabase);
        }

        private void ImplicitTransactionBulkInsertThrowsException(TestDatabase korm)
        {
            var dbSet = korm.Query<Invoice>().AsDbSet();

            Action bulkInsertAction = () => dbSet.BulkInsert(null);
            bulkInsertAction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataOnOpenedConnection()
        {
            DoTestOnOpenedConnection(ExplicitTransactionShould_CommitData, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataOnClosedConnection()
        {
            DoTestOnClosedConnection(ExplicitTransactionShould_CommitData, CreateDatabase);
        }

        private void ExplicitTransactionShould_CommitData(TestDatabase korm)
        {
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
        public void ExplicitTransactionShould_CommitDataWhenBulkInsertWasCalledOnOpenedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionBulkInsertCommit(db, BulkInsertAddItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkInsertWasCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionBulkInsertCommit(db, BulkInsertAddItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkInsertEnumerableWasCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection((db) => ExplicitTransactionBulkInsertCommit(db, BulkInsertEnumerableItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkInsertEnumerableWasCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionBulkInsertCommit(db, BulkInsertEnumerableItems),
                                     CreateDatabase);
        }

        private void ExplicitTransactionBulkInsertCommit(TestDatabase korm, Action<IDbSet<Invoice>> action)
        {
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                action.Invoke(dbSet);

                transaction.Commit();

                DatabaseShouldContainInvoices(korm.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataOnOpenedConnection()
        {
            DoTestOnOpenedConnection(ExplicitTransactionShould_RollbackData, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataOnClosedConnection()
        {
            DoTestOnClosedConnection(ExplicitTransactionShould_RollbackData, CreateDatabase);
        }

        private void ExplicitTransactionShould_RollbackData(TestDatabase korm)
        {
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
        public void ExplicitTransactionShould_RollbackDataWhenBulkInsertWasCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection((db) => ExplicitTransactionRollbackBulkInsertCore(db, BulkInsertAddItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataWhenBulkInsertWasCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionRollbackBulkInsertCore(db, BulkInsertAddItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataWhenBulkInsertEnumerableWasCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection((db) => ExplicitTransactionRollbackBulkInsertCore(db, BulkInsertEnumerableItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackDataWhenBulkInsertEnumerableWasCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionRollbackBulkInsertCore(db, BulkInsertEnumerableItems),
                                     CreateDatabase);
        }

        private void ExplicitTransactionRollbackBulkInsertCore(TestDatabase korm, Action<IDbSet<Invoice>> action)
        {
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                action.Invoke(dbSet);

                transaction.Rollback();

                DatabaseShouldBeEmpty(korm);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkInsertCommitWasNotCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection((db) => ExplicitTransactionBulkInsertCommitNotCalled(db, BulkInsertAddItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkInsertCommitWasNotCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionBulkInsertCommitNotCalled(db, BulkInsertAddItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkInsertEnumerableCommitWasNotCalledOnOpenedConnection()
        {
            DoTestOnOpenedConnection((db) => ExplicitTransactionBulkInsertCommitNotCalled(db, BulkInsertEnumerableItems),
                                     CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkInsertEnumerableCommitWasNotCalledOnClosedConnection()
        {
            DoTestOnClosedConnection((db) => ExplicitTransactionBulkInsertCommitNotCalled(db, BulkInsertEnumerableItems),
                                     CreateDatabase);
        }

        private void ExplicitTransactionBulkInsertCommitNotCalled(TestDatabase database, Action<IDbSet<Invoice>> action)
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
        public void DataShould_BeAccessibleFromTransactionOnOpenedConnection()
        {
            DoTestOnOpenedConnection(DataAccessibleFromTransaction, CreateDatabase);
        }

        [Fact]
        public void DataShould_BeAccessibleFromTransactionOnClosedConnection()
        {
            DoTestOnClosedConnection(DataAccessibleFromTransaction, CreateDatabase);
        }

        private void DataAccessibleFromTransaction(TestDatabase korm)
        {
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
        public void ExplicitTransactionShould_RollbackMultipleCommitOnOpenedConnection()
        {
            DoTestOnOpenedConnection(ExplicitTransactionRollbackMultipleCommits, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_RollbackMultipleCommitOnClosedConnection()
        {
            DoTestOnClosedConnection(ExplicitTransactionRollbackMultipleCommits, CreateDatabase);
        }

        private void ExplicitTransactionRollbackMultipleCommits(TestDatabase korm)
        {
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
        public void ExplicitTransactionShould_CloseMasterConnectionWhenCommitWasCalled()
        {
            DoTestOnClosedConnection(ExplicitTransactionCommit, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenCommitWasCalled()
        {
            DoTestOnOpenedConnection(ExplicitTransactionCommit, CreateDatabase);
        }


        private void ExplicitTransactionCommit(TestDatabase database)
        {
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                transaction.Commit();

                DatabaseShouldContainInvoices(database.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_CloseMasterConnectionWhenRollbackWasCalled()
        {
            DoTestOnClosedConnection(ExplicitTransactionRollback, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenRollbackWasCalled()
        {
            DoTestOnOpenedConnection(ExplicitTransactionRollback, CreateDatabase);
        }

        private void ExplicitTransactionRollback(TestDatabase database)
        {
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.CommitChanges();

                transaction.Rollback();

                korm.Query<Invoice>().Should().BeEmpty();
                DatabaseShouldBeEmpty(database);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenRollbackWasCalledAfterBulkInsert()
        {
            DoTestOnOpenedConnection(ExplicitTransactionRollbackAfterBulkInsert, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_CloseMasterConnectionWhenRollbackWasCalledAfterBulkInsert()
        {
            DoTestOnClosedConnection(ExplicitTransactionRollbackAfterBulkInsert, CreateDatabase);
        }

        private void ExplicitTransactionRollbackAfterBulkInsert(TestDatabase database)
        {
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.BulkInsert();

                transaction.Rollback();

                korm.Query<Invoice>().Should().BeEmpty();
                DatabaseShouldBeEmpty(database);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_CloseMasterConnectionWhenCommitWasCalledAfterBulkInsert()
        {
            DoTestOnClosedConnection(ExplicitTransactionCommitAfterBulkInsert, CreateDatabase);
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenCommitWasCalledAfterBulkInsert()
        {
            DoTestOnOpenedConnection(ExplicitTransactionCommitAfterBulkInsert, CreateDatabase);
        }

        private void ExplicitTransactionCommitAfterBulkInsert(TestDatabase database)
        {
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSet.Add(CreateTestData());
                dbSet.BulkInsert();

                transaction.Commit();

                DatabaseShouldContainInvoices(database.ConnectionString, CreateTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_ThrowCommandTimeoutExceptionWhenIsSetTooSmall()
        {
            using (var database = CreateAndInitDatabase(CreateProcedure_WaitForTwoSeconds))
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
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
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
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
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                transaction.CommandTimeout = 3;
                Action commit = () => { korm.ExecuteStoredProcedure<Object>("WaitForTwoSeconds"); };

                commit.Should().NotThrow<SqlException>();
            }
        }

        #endregion

        #region Helpers

        private void DoTestOnClosedConnection(Action<TestDatabase> testAction, Func<TestDatabase> createDatabaseAction)
        {
            using (var database = createDatabaseAction())
            {
                testAction(database);
                database.Connection.State.Should().Be(ConnectionState.Closed);
            }
        }

        private void DoTestOnOpenedConnection(Action<TestDatabase> testAction, Func<TestDatabase> createDatabaseAction)
        {
            using (var database = createDatabaseAction())
            {
                database.Connection.Open();
                testAction(database);
                database.Connection.State.Should().Be(ConnectionState.Open);
            }
        }

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
            using (var korm = new Database(connectionString, SqlServerDataHelper.ClientId))
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
