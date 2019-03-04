using FluentAssertions;
using Kros.Data;
using Kros.KORM.Migrations;
using Kros.UnitTests;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests.Migrations
{
    public class MigrationsRunnerShould : SqlServerDatabaseTestBase
    {

        #region Sql Scripts

        private static string CreateTable_MigrationHistory =
$@"CREATE TABLE [dbo].[__KormMigrationsHistory](
    [MigrationId] [bigint] NOT NULL,
    [MigrationName] [nvarchar](255) NOT NULL,
    [ProductInfo] [nvarchar](255) NOT NULL,
    [Updated] [datetime2] NULL,
    CONSTRAINT [PK_MigrationHistory] PRIMARY KEY CLUSTERED
    (
        [MigrationId] ASC
    )
) ON [PRIMARY]
";
        private static string CreateTable_People =
$@"CREATE TABLE [dbo].[People](
    [Id] [int] NOT NULL,
    [Name] [nvarchar](255) NOT NULL
    CONSTRAINT [PK_People] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    )
) ON [PRIMARY]
";
        private static string InsertIntoMigrationHistory =
$@"INSERT INTO __KormMigrationsHistory VALUES (20190301001, 'InitDatabase', 'FromUnitTests', '20190301')";

        #endregion

        protected override string BaseConnectionString
            => IntegrationTestConfig.ConnectionString;

        [Theory()]
        [InlineData(null, "__KormMigrationsHistory")]
        [InlineData("__myTableName", "__myTableName")]
        public void ExecuteInitialMigration(string historyTableName, string expectedHistoryTableName)
        {
            using (var helper = CreateHelper(nameof(ExecuteInitialMigration), historyTableName))
            {
                helper.Runner.Migrate();

                TableShouldExist(expectedHistoryTableName);
                TableShouldExist("People");

                DatabaseVersionShouldBe(20190301001, expectedHistoryTableName);
            }
        }

        [Fact]
        public void MigrateToLastVersion()
        {
            using (var helper = CreateHelper(nameof(MigrateToLastVersion)))
            {
                InitDatabase();

                helper.Runner.Migrate();

                TableShouldExist("People");
                TableShouldExist("Projects");
                TableShouldExist("ProjectDetails");
                TableShouldExist("Contacts");

                DatabaseVersionShouldBe(2019030103);
            }
        }

        private void InitDatabase()
        {
            ExecuteCommand((cmd) =>
            {
                foreach (var script in new List<string>() {
                    CreateTable_MigrationHistory ,
                    CreateTable_People,
                    InsertIntoMigrationHistory })
                {
                    cmd.CommandText = script;
                    cmd.ExecuteScalar();
                }
            });
        }

        private void TableShouldExist(string tableName)
        {
            ExecuteCommand((cmd) =>
            {
                cmd.CommandText = $"SELECT Count(*) FROM sys.tables WHERE name = '{tableName}' AND type = 'U'";
                ((int)cmd.ExecuteScalar())
                    .Should().Be(1);
            });
        }

        private void ExecuteCommand(Action<SqlCommand> action)
        {
            using (ConnectionHelper.OpenConnection(ServerHelper.Connection))
            using (var cmd = ServerHelper.Connection.CreateCommand())
            {
                action(cmd);
            }
        }

        private void DatabaseVersionShouldBe(long databaseVersion, string historyTableName = "__KormMigrationsHistory")
        {
            ExecuteCommand((cmd) =>
            {
                cmd.CommandText = $"SELECT TOP 1 MigrationId FROM {historyTableName} ORDER BY MigrationId";
                ((long)cmd.ExecuteScalar())
                    .Should().Be(databaseVersion);
            });
        }

        private Helper CreateHelper(string folderName, string historyTableName = null)
        {
            return new Helper(new Database(ServerHelper.Connection), folderName, historyTableName);
        }

        private class Helper : IDisposable
        {
            private IDatabase _database;

            public Helper(Database database, string folderName, string historyTableName = null)
            {
                _database = database;
                var options = new MigrationOptions();
                if (historyTableName != null)
                {
                    options.HistoryTableName = historyTableName;
                }

                options.AddAssemblyScriptsProvider(
                    Assembly.GetExecutingAssembly(),
                    $"Kros.KORM.Extensions.Api.UnitTests.Resources.ScriptsForRunner.{folderName}");

                Runner = new MigrationsRunner(_database, options);
            }

            public MigrationsRunner Runner { get; set; }

            public void Dispose()
            {
                _database.Dispose();
            }
        }
    }
}
