using Kros.KORM.Query.Sql;
using System.Linq;
using Xunit;

namespace Kros.KORM.UnitTests.Query.Sql
{
    public class LinqTranslatorForSqlServer2008Should : LinqTranslatorTestBase
    {
        #region Tests

        [Fact]
        public void TranslateTakeMethod()
        {
            var query = Query<Person>()
                .Take(5);

            AreSame(query, "SELECT TOP 5 Id, FirstName, LastName, PostAddress FROM People");
        }

        [Fact]
        public void TranslateSkipMethod()
        {
            var query = Query<Person>()
                .Skip(10)
                .OrderBy(p => p.Id);

            const string expectedQuery =
                "WITH Results_CTE AS (SELECT Id, FirstName, LastName, PostAddress, " +
                "ROW_NUMBER() OVER(ORDER BY Id ASC) AS __RowNum__ FROM People) " +
                "SELECT * FROM Results_CTE WHERE __RowNum__ > 10";
            AreSame(query, new QueryInfo(expectedQuery, null), null);
        }

        [Fact]
        public void TranslateSkipWithTakeMethod()
        {
            var query = Query<Person>()
                .Skip(10)
                .Take(5)
                .OrderBy(p => p.Id);

            const string expectedQuery =
                "WITH Results_CTE AS (SELECT Id, FirstName, LastName, PostAddress, " +
                "ROW_NUMBER() OVER(ORDER BY Id ASC) AS __RowNum__ FROM People) " +
                "SELECT * FROM Results_CTE WHERE __RowNum__ > 10 AND __RowNum__ <= 15";
            AreSame(query, new QueryInfo(expectedQuery, null), null);
        }

        [Fact]
        public void TranslateSkipMethodWithCondition()
        {
            var query = Query<Person>()
                .Where(p => p.Id > 5)
                .Skip(10)
                .OrderBy(p => p.Id);

            const string expectedQuery =
                "WITH Results_CTE AS (SELECT Id, FirstName, LastName, PostAddress, " +
                "ROW_NUMBER() OVER(ORDER BY Id ASC) AS __RowNum__ FROM People WHERE ((Id > @1))) " +
                "SELECT * FROM Results_CTE WHERE __RowNum__ > 10";
            AreSame(query, new QueryInfo(expectedQuery, null), 5);
        }

        [Fact]
        public void TranslateSkipWithTakeMethodAndCondition()
        {
            var query = Query<Person>()
                .Where(p => p.Id > 5)
                .Skip(10)
                .Take(5)
                .OrderBy(p => p.Id);

            const string expectedQuery =
                "WITH Results_CTE AS (SELECT Id, FirstName, LastName, PostAddress, " +
                "ROW_NUMBER() OVER(ORDER BY Id ASC) AS __RowNum__ FROM People WHERE ((Id > @1))) " +
                "SELECT * FROM Results_CTE WHERE __RowNum__ > 10 AND __RowNum__ <= 15";
            AreSame(query, new QueryInfo(expectedQuery, null), 5);
        }

        #endregion

        #region Helpers

        protected override ISqlExpressionVisitor CreateVisitor() => new SqlServer2008SqlGenerator(Database.DatabaseMapper);

        #endregion
    }
}
