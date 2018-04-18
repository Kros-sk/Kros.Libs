using FluentAssertions;
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

            action.ShouldThrow<ArgumentNullException>();
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

        private class Person
        { }

    }
}