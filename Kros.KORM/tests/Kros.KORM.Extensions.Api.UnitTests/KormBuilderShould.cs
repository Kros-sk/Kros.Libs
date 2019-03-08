using FluentAssertions;
using Kros.Data;
using Kros.KORM.Extensions.Asp;
using Kros.KORM.Migrations;
using Kros.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Configuration;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class KormBuilderShould : SqlServerDatabaseTestBase
    {
        protected override string BaseConnectionString
            => IntegrationTestConfig.ConnectionString;

        [Fact]
        public void InitDatabaseForIdGenerator()
        {
            KormBuilder kormBuilder = CreateKormBuilder();
            kormBuilder.InitDatabaseForIdGenerator();

            CheckTableAndProcedure();
        }

        private KormBuilder CreateKormBuilder()
        {
            var connectionSettings = new ConnectionStringSettings(
                "Default",
                ServerHelper.Connection.ConnectionString,
                "System.Data.SqlClient");

            var kormBuilder = new KormBuilder(new ServiceCollection(), connectionSettings);

            return kormBuilder;
        }

        [Fact]
        public void ThrowExceptionWhenAddMigrationsWithoutConfigurationSection()
        {
            var kormBuilder = CreateKormBuilder();
            var (configuration, _) = ConfigurationHelper.CreateHelpers("missingsection");

            Action action = () =>
            {
                kormBuilder.AddKormMigrations(configuration);
            };

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*Configuration section 'KormMigrations' is missing.*");
        }

        [Fact]
        public void AddMigrationsToContainer()
        {
            var kormBuilder = CreateKormBuilder();
            var (configuration, _) = ConfigurationHelper.CreateHelpers("standard");

            kormBuilder.AddKormMigrations(configuration);

            kormBuilder.Services.BuildServiceProvider()
                .GetService<IMigrationsRunner>()
                .Should().NotBeNull();
        }

        [Fact]
        public void ExecuteMigrations()
        {
            var kormBuilder = CreateKormBuilder();
            var (configuration, _) = ConfigurationHelper.CreateHelpers("standard");

            kormBuilder.AddKormMigrations(configuration);
            var migrationRunner = Substitute.For<IMigrationsRunner>();
            kormBuilder.Services.AddSingleton<IMigrationsRunner>(migrationRunner);

            kormBuilder.Migrate();

            migrationRunner.Received().MigrateAsync();
        }

        [Fact]
        public void NotExecuteMigrationsWhenAutoUpgrateIsOff()
        {
            var kormBuilder = CreateKormBuilder();
            var (configuration, _) = ConfigurationHelper.CreateHelpers("autoupdateoff");

            kormBuilder.AddKormMigrations(configuration);
            var migrationRunner = Substitute.For<IMigrationsRunner>();
            kormBuilder.Services.AddSingleton<IMigrationsRunner>(migrationRunner);

            kormBuilder.Migrate();

            migrationRunner.DidNotReceive().MigrateAsync();
        }

        private void CheckTableAndProcedure()
        {
            using (ConnectionHelper.OpenConnection(ServerHelper.Connection))
            using (var cmd = ServerHelper.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Count(*) FROM sys.tables WHERE name = 'IdStore' AND type = 'U'";
                ((int)cmd.ExecuteScalar())
                    .Should().Be(1);

                cmd.CommandText = "SELECT Count(*) FROM sys.procedures WHERE name = 'spGetNewId' AND type = 'P'";
                ((int)cmd.ExecuteScalar())
                    .Should().Be(1);
            }
        }
    }
}
