using FluentAssertions;
using Kros.Data.BulkActions.SqlServer;
using System.Data.SqlClient;
using Xunit;

namespace Kros.Utils.UnitTests.Data.BulkInsert
{
    public class SqlServerBulkInsertFactoryShould
    {
        [Fact]
        public void CreateSqlServerBulkInsertByConnection()
        {
            using (var conn = new SqlConnection())
            {
                var factory = new SqlServerBulkInsertFactory(conn);
                var bulkInsert = factory.GetBulkInsert(SqlBulkCopyOptions.UseInternalTransaction) as SqlServerBulkInsert;

                bulkInsert.BulkCopyOptions.Should().Be(SqlBulkCopyOptions.UseInternalTransaction);
            }
        }
    }
}
