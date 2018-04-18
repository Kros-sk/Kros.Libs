using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query.Sql;
using Kros.KORM.Query.Sql.MsAccess;
using Kros.KORM.UnitTests.Query.Sql;
using System.Linq;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests.Query.Sql
{
    public class LinqFunctionTranslatorShould: LinqTranslatorTestBase
    {
        /// <summary>
        /// Create visitor for translate query to SQL.
        /// </summary>
        protected override ISqlExpressionVisitor CreateVisitor() =>
            new MsAccessQuerySqlGenerator(Database.DatabaseMapper);

        [Fact]
        public void TranslateAnyMethod()
        {
            var query = Query<Person>();
            var item = query.Any();

            WasGeneratedSameSql(query,
                            @"SELECT TOP 1 IIF(EXISTS(SELECT '' FROM People), 1, 0) FROM People");
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

        [Alias("People")]
        public class Person
        {
            public int Id { get; set; }
        }
    }
}
