using FluentAssertions;
using Kros.Data;
using System;
using System.Data.SqlClient;
using Xunit;

namespace Kros.KORM.UnitTests
{
    public class DatabaseShould
    {
        [Fact]
        public void ThrowExceptionWhenActiveConnectionIsNull()
        {
            SqlConnection connection = null;
            Action action = () =>
            {
                IDatabase database = new Database(connection);
            };

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HasActiveConnectionWithDefaultModelBuilder()
        {
            var connection = new SqlConnection();
            IDatabase database = new Database(connection);

            database.ModelBuilder.Should().NotBeNull();
        }

        [Fact]
        public void CreateQuery()
        {
            var connection = new SqlConnection();
            IDatabase database = new Database(connection);

            database.Query<Person>().Should().NotBeNull();
        }

        [Fact]
        public void InitForIdGenerator()
        {
            using (var connection = new SqlConnection())
            using (var database = new Database(connection))
            {
                var idGeneratorFactory = NSubstitute.Substitute.For<IIdGeneratorFactory>();

                IdGeneratorFactories.Register<SqlConnection>(
                    "Sytem.Data.Fake",
                    (conn) => idGeneratorFactory,
                    (connString) => idGeneratorFactory);

                database.InitDatabaseForIdGenerator();
            }
        }

        private class Person
        {
        }
    }
}