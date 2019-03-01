using FluentAssertions;
using Kros.Data;
using Kros.KORM.Extensions.Asp;
using Kros.UnitTests;
using Microsoft.Extensions.DependencyInjection;
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
            var connectionSettings = new ConnectionStringSettings(
                "Default",
                ServerHelper.Connection.ConnectionString,
                "System.Data.SqlClient");

            var kormBuilder = new KormBuilder(new ServiceCollection(), connectionSettings);
            kormBuilder.InitDatabaseForIdGenerator();

            CheckTableAndProcedure();
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
