using FluentAssertions;
using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query.Providers;
using Kros.KORM.Query.Sql;
using Kros.KORM.Query.Sql.MsAccess;
using Kros.KORM.UnitTests.Query.Sql;
using System;
using System.Linq;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests.Query.Sql
{
    public class LinqFunctionTranslatorShould : LinqTranslatorTestBase
    {
        #region nested Types

        [Alias("People")]
        public new class Person
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        #endregion

        /// <summary>
        /// Create visitor for translate query to SQL.
        /// </summary>
        protected override ISqlExpressionVisitor CreateVisitor() =>
            new MsAccessQuerySqlGenerator(Database.DatabaseMapper);

        [Fact]
        public void TranslateSkipMethod()
        {
            var query = Query<Person>()
                .Skip(10)
                .OrderBy(p => p.Id);

            AreSame(
                query,
                new QueryInfo(
                    "SELECT Id, FirstName, LastName FROM People ORDER BY Id ASC",
                    new LimitOffsetDataReader(0, 10)),
                null);
        }

        [Fact]
        public void TranslateSkipWithTakeMethod()
        {
            var query = Query<Person>()
                .Skip(10)
                .Take(5)
                .OrderBy(p => p.Id);

            AreSame(
                query,
                new QueryInfo(
                    "SELECT Id, FirstName, LastName FROM People ORDER BY Id ASC",
                    new LimitOffsetDataReader(5, 10)),
                null);
        }

        [Fact]
        public void TranslateSkipMethodAndCondition()
        {
            var query = Query<Person>()
                .Where(p => p.Id == 5)
                .Skip(10)
                .OrderBy(p => p.Id);

            AreSame(
                query,
                new QueryInfo(
                    "SELECT Id, FirstName, LastName FROM People WHERE ((Id = @1)) ORDER BY Id ASC",
                    new LimitOffsetDataReader(0, 10)),
                5);
        }

        [Fact]
        public void TranslateSkipWithTakeMethodAndCondition()
        {
            var query = Query<Person>()
                .Where(p => p.Id == 5)
                .Skip(10)
                .Take(5)
                .OrderBy(p => p.Id);

            AreSame(
                query,
                new QueryInfo(
                    "SELECT Id, FirstName, LastName FROM People WHERE ((Id = @1)) ORDER BY Id ASC",
                    new LimitOffsetDataReader(5, 10)),
                5);
        }

        [Fact]
        public void TranslateSkipWithTakeAndConditionAndComplexOrderBy()
        {
            var query = Query<Person>()
                .Where(p => p.Id > 5)
                .Skip(10)
                .Take(5)
                .OrderBy(p => p.Id)
                .ThenByDescending(p => p.FirstName);

            AreSame(
                query,
                new QueryInfo(
                    "SELECT Id, FirstName, LastName FROM People WHERE ((Id > @1)) ORDER BY Id ASC, FirstName DESC",
                    new LimitOffsetDataReader(5, 10)),
                5);
        }

        [Fact]
        public void ThrowInvalidOperationExceptionWhenUsedSkipWithoutOrderBy()
        {
            var visitor = CreateVisitor();
            var query = Query<Person>()
                .Skip(10);
            Action action = () => visitor.GenerateSql(query.Expression);

            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void TranslateAnyMethod()
        {
            var query = Query<Person>();
            var item = query.Any();

            WasGeneratedSameSql(query, @"SELECT TOP 1 IIF(EXISTS(SELECT '' FROM People), 1, 0) FROM People");
        }

        [Fact]
        public void TranslateAnyMethodWithCondition()
        {
            var query = Query<Person>();
            var item = query.Any(p => p.Id > 5);

            WasGeneratedSameSql(query,
                            @"SELECT TOP 1 IIF(EXISTS(SELECT '' FROM People WHERE ((Id > @1))), 1, 0) FROM People", 5);
        }

        [Fact]
        public void TranslateAnyAfterWhereMethod()
        {
            var query = Query<Person>();
            var item = query.Where(p => p.Id > 5).Any();

            WasGeneratedSameSql(query,
                            @"SELECT TOP 1 IIF(EXISTS(SELECT '' FROM People WHERE ((Id > @1))), 1, 0) FROM People", 5);
        }

        [Fact]
        public void CreateCorrectOrderByIfItIsSpecifiedByStringAndAlsoByExpression()
        {
            var query = Query<Person>()
                .OrderBy("Id DESC")
                .OrderByDescending(p => p.FirstName)
                .OrderBy(p => p.LastName);

            AreSame(
                query,
                "SELECT Id, FirstName, LastName FROM People ORDER BY Id DESC, FirstName DESC, LastName ASC");
        }

        [Fact]
        public void NotThrowWhenUsedSkipWithStringOrderBy()
        {
            var visitor = CreateVisitor();
            var query = Query<Person>()
                .OrderBy("Id DESC")
                .Skip(10);
            Action action = () => visitor.GenerateSql(query.Expression);

            action.Should().NotThrow();
        }
    }
}
