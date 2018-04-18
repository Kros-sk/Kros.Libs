using FluentAssertions;
using Kros.KORM.Metadata;
using Kros.KORM.Query.Expressions;
using Kros.KORM.Query.Sql;
using Xunit;

namespace Kros.KORM.UnitTests.Query.Sql
{
    public class SqlServerQuerySqlGeneratorShould
    {
        [Fact]
        public void ConvertQueryToSqlServerSyntax()
        {
            var originSql = "select " +
                "FALSE as A, " +
                "True as B, " +
                "Now() as N, " +
                "'A' & 'B' as C, " +
                "IIF(A=TRUE, X, Y), " +
                "UCase(A), " +
                "CDate(42523.600145219905) as E " +
                "FROM PERSON";
            var expected = "select " +
                "0 as A, " +
                "1 as B, " +
                "GetDate() as N, " +
                "'A' + 'B' as C, " +
                "(CASE WHEN A=1 THEN X ELSE Y END), " +
                "UPPER(A), " +
                "CONVERT(DateTime, 42521.6001452199) as E " +
                "FROM PERSON";

            var expression = new SqlExpression(originSql, 10);
            var generator = new SqlServerQuerySqlGenerator(new DatabaseMapper(new ConventionModelMapper()));

            var sql = generator.GenerateSql(expression);

            sql.Should().Be(expected);
        }

        [Fact]
        public void ConvertMultipleIifsToSqlServerSyntax()
        {
            var originSql = "select " +
                "IIF(A=TRUE, X, Y), " +
                "IIF(B = 123, Q, IIF(C = 'Lorem', 'Ipsum', 'Dolor')), " +
                "FROM PERSON";
            var expected = "select " +
                "(CASE WHEN A=1 THEN X ELSE Y END), " +
                "(CASE WHEN B = 123 THEN Q ELSE (CASE WHEN C = 'Lorem' THEN 'Ipsum' ELSE 'Dolor' END) END), " +
                "FROM PERSON";

            var expression = new SqlExpression(originSql, 10);
            var generator = new SqlServerQuerySqlGenerator(new DatabaseMapper(new ConventionModelMapper()));

            var sql = generator.GenerateSql(expression);

            sql.Should().Be(expected);
        }
    }
}
