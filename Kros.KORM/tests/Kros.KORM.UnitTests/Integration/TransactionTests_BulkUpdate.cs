using FluentAssertions;
using Kros.Data.SqlServer;
using Kros.KORM.Query;
using Kros.KORM.UnitTests.Base;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Kros.KORM.UnitTests.Integration
{
    public partial class TransactionTests
    {
        #region SQL Scripts

        private static string InsertIntoTestTable =
            @"INSERT INTO Invoices 
                     VALUES (1, '', '')
              INSERT INTO Invoices
                     VALUES (2, 'bulk', 'update')
              INSERT INTO Invoices
                     VALUES (3, 'lorem' ,'ipsum')";

        #endregion

        #region Bulk Update Tests Edited Items

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkUpdateWasCalled()
        {
            ImplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(CreateTestData(), null, BulkUpdateEditItems);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkUpdateActionWasCalled()
        {
            ImplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(
                CreateActionTestData(), ExecuteInTempTable, BulkUpdateEditItems);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkUpdateEnumerableWasCalled()
        {
            ImplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(CreateTestData(), null, BulkUpdateEnumerableItems);
        }

        [Fact]
        public void ImplicitTransactionShould_CommitDataWhenBulkUpdateEnumerableActionWasCalled()
        {
            ImplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(
                CreateActionTestData(), ExecuteInTempTable, BulkUpdateEnumerableItems);
        }

        private void ImplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(
            IEnumerable<Invoice> expectedData,
            Action<IDbConnection, IDbTransaction, string> action,
            Action<IDbSet<Invoice>, Action<IDbConnection, IDbTransaction, string>> dbSetAction)
        {
            using (var korm = CreateDatabaseWithData())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSetAction.Invoke(dbSet, action);

                DatabaseShouldContainInvoices(korm.ConnectionString, expectedData);
            }
        }

        [Fact]
        public void ImplicitTransactionShould_BulkUpdateThrowsException()
        {
            using (var korm = CreateDatabaseWithData())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();
                IEnumerable<Invoice> items = null;
                Action bulkUpdateAction = () => dbSet.BulkUpdate(items);

                bulkUpdateAction.Should().Throw<ArgumentNullException>();
            }
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkUpdateWasCalled()
        {
            ExplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(CreateTestData(), null, BulkUpdateEditItems);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkUpdateActionWasCalled()
        {
            ExplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(
                CreateActionTestData(), ExecuteInTempTable, BulkUpdateEditItems);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkUpdateEnumerableWasCalled()
        {
            ExplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(CreateTestData(), null, BulkUpdateEnumerableItems);
        }

        [Fact]
        public void ExplicitTransactionShould_CommitDataWhenBulkUpdateEnumerableActionWasCalled()
        {
            ExplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(
                CreateActionTestData(), ExecuteInTempTable, BulkUpdateEnumerableItems);
        }

        private void ExplicitTransactionCommitDataWhenBulkUpdateWasCalledCore(
            IEnumerable<Invoice> expectedData,
            Action<IDbConnection, IDbTransaction, string> action,
            Action<IDbSet<Invoice>, Action<IDbConnection, IDbTransaction, string>> dbSetAction)
        {
            using (var korm = CreateDatabaseWithData())
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSetAction.Invoke(dbSet, action);

                transaction.Commit();

                DatabaseShouldContainInvoices(korm.ConnectionString, expectedData);
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkUpdateWithNoCommit()
        {
            NotChangeDataWhenBulkUpdateWithNoCommitCore(null, BulkUpdateEditItems);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkUpdateActionWithNoCommit()
        {
            NotChangeDataWhenBulkUpdateWithNoCommitCore(ExecuteInTempTable, BulkUpdateEditItems);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkUpdateEnumerableWithNoCommit()
        {
            NotChangeDataWhenBulkUpdateWithNoCommitCore(null, BulkUpdateEnumerableItems);
        }

        [Fact]
        public void ExplicitTransactionShould_NotChangeDataWhenBulkUpdateEnumerableActionWithNoCommit()
        {
            NotChangeDataWhenBulkUpdateWithNoCommitCore(ExecuteInTempTable, BulkUpdateEnumerableItems);
        }

        private void NotChangeDataWhenBulkUpdateWithNoCommitCore(
            Action<IDbConnection, IDbTransaction, string> action,
            Action<IDbSet<Invoice>, Action<IDbConnection, IDbTransaction, string>> dbSetAction)
        {
            using (var korm = CreateDatabaseWithData())
            {
                using (var transaction = korm.BeginTransaction())
                {
                    var dbSet = korm.Query<Invoice>().AsDbSet();

                    dbSetAction.Invoke(dbSet, action);
                }

                DatabaseShouldContainInvoices(korm.ConnectionString, CreateOriginalTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenRollbackWasCallAfterBulkUpdate()
        {
            NotCloseMasterConnectionWhenRollbackWasCallAfterBulkUpdateCore(null, BulkUpdateEditItems);
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenRollbackWasCallAfterBulkUpdateAction()
        {
            NotCloseMasterConnectionWhenRollbackWasCallAfterBulkUpdateCore(ExecuteInTempTable, BulkUpdateEditItems);
        }

        private void NotCloseMasterConnectionWhenRollbackWasCallAfterBulkUpdateCore(
            Action<IDbConnection, IDbTransaction, string> action,
            Action<IDbSet<Invoice>, Action<IDbConnection, IDbTransaction, string>> dbSetAction)
        {
            using (var database = CreateDatabaseWithData())
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSetAction.Invoke(dbSet, action);

                transaction.Rollback();

                database.Connection.State.Should().Be(ConnectionState.Open);
                korm.Query<Invoice>().Should().BeEquivalentTo(CreateOriginalTestData());
                DatabaseShouldContainInvoices(database.ConnectionString, CreateOriginalTestData());
            }
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenCommitWasCallAfterBulkUpdate()
        {
            NotCloseMasterConnectionWhenCommitWasCallAfterBulkUpdateCore(CreateTestData(), null, BulkUpdateEditItems);
        }

        [Fact]
        public void ExplicitTransactionShould_NotCloseMasterConnectionWhenCommitWasCallAfterBulkUpdateAction()
        {
            NotCloseMasterConnectionWhenCommitWasCallAfterBulkUpdateCore(
                CreateActionTestData(), ExecuteInTempTable, BulkUpdateEditItems);
        }

        private void NotCloseMasterConnectionWhenCommitWasCallAfterBulkUpdateCore(
            IEnumerable<Invoice> expectedData,
            Action<IDbConnection, IDbTransaction, string> action,
            Action<IDbSet<Invoice>, Action<IDbConnection, IDbTransaction, string>> dbSetAction)
        {
            using (var database = CreateDatabaseWithData())
            using (var korm = new Database(database.ConnectionString, SqlServerDataHelper.ClientId))
            using (var transaction = korm.BeginTransaction())
            {
                var dbSet = korm.Query<Invoice>().AsDbSet();

                dbSetAction.Invoke(dbSet, action);

                transaction.Commit();

                database.Connection.State.Should().Be(ConnectionState.Open);
                DatabaseShouldContainInvoices(database.ConnectionString, expectedData);
            }
        }

        #endregion

        #region Helpers

        private IEnumerable<Invoice> CreateOriginalTestData() =>
            new List<Invoice>() {
                new Invoice() { Id = 1, Code = "", Description = ""},
                new Invoice() { Id = 2, Code = "bulk", Description = "update"},
                new Invoice() { Id = 3, Code = "lorem", Description = "ipsum"} };

        private IEnumerable<Invoice> CreateActionTestData() =>
            new List<Invoice>() {
                new Invoice() { Id = 1, Code = "0001", Description = "Updated in temp."},
                new Invoice() { Id = 2, Code = "0002", Description = "Updated in temp."},
                new Invoice() { Id = 3, Code = "0002", Description = "Updated in temp."} };

        private TestDatabase CreateDatabaseWithData() =>
            CreateDatabase(new string[] { CreateTable_TestTable, InsertIntoTestTable }) as DatabaseTestBase.TestDatabase;

        private void BulkUpdateEditItems(
            IDbSet<Invoice> dbSet,
            Action<IDbConnection, IDbTransaction, string> action)
        {
            dbSet.Edit(CreateTestData());

            if (action == null)
            {
                dbSet.BulkUpdate();
            }
            else
            {
                dbSet.BulkUpdate(action);
            }
        }

        private void BulkUpdateEnumerableItems(
            IDbSet<Invoice> dbSet,
            Action<IDbConnection, IDbTransaction, string> action)
        {
            if (action == null)
            {
                dbSet.BulkUpdate(CreateTestData());
            }
            else
            {
                dbSet.BulkUpdate(CreateTestData(), action);
            }
        }

        private static void ExecuteInTempTable(IDbConnection connection, IDbTransaction transaction, string tempTable)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = $"UPDATE [{tempTable}] SET [Description] = 'Updated in temp.'";
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
