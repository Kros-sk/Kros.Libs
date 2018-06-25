using Kros.KORM.Metadata.Attribute;
using Kros.KORM.Query.Sql;
using Kros.KORM.Query.Sql.MsAccess;
using Kros.KORM.UnitTests.Query.Sql;
using System.Linq;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests.Query.Sql
{
    public class LinqStringFunctionMsAccessTranslator : LinqTranslatorTestBase
    {
        //Podporované funkcie
        //  * StartsWith
        //  * EndWiths
        //  * Contains
        //  * IsNullOrEmpty
        //  * ToUpper
        //  * ToLower
        //  * Replace
        //  * SubString
        //  * Trim
        //V budúcnosti
        //  / IndexOf
        //  / Remove
        //  / Concat

        /// <summary>
        /// Create visitor for translate query to SQL.
        /// </summary>
        protected override ISqlExpressionVisitor CreateVisitor() =>
            new MsAccessQuerySqlGenerator(Database.DatabaseMapper);

        [Fact]
        public void TranslateToUpperMethod()
        {
            var query = Query<Person>().Where(p => p.Name.ToUpper() == "JOHN");

            AreSame(query, "SELECT Id, FirstName, LastName FROM People" +
                           " WHERE ((UCASE(FirstName) = @1))", "JOHN");
        }

        [Fact]
        public void TranslateToLowerMethod()
        {
            var query = Query<Person>().Where(p => "john" == p.Name.ToLower());

            AreSame(query, "SELECT Id, FirstName, LastName FROM People" +
                           " WHERE ((@1 = LCASE(FirstName)))", "john");
        }

        [Fact]
        public void TranslateSubstringMethod()
        {
            var query = Query<Person>().Where(p => p.Name.Substring(1, 2) == "oh");

            AreSame(query, "SELECT Id, FirstName, LastName FROM People" +
                            " WHERE ((MID(FirstName, @1 + 1, @2) = @3))", 1, 2, "oh");
        }

        [Fact]
        public void TranslateSubstringToEndMethod()
        {
            var query = Query<Person>().Where(p => p.Name.Substring(2) == "hn");

            AreSame(query, "SELECT Id, FirstName, LastName FROM People" +
                           " WHERE ((MID(FirstName, @1 + 1, @2) = @3))", 2, 8000, "hn");
        }

        [Fact]
        public void TranslateTrimEndMethod()
        {
            var query = Query<Person>().Where(p => p.Name.Trim() == "John");

            AreSame(query, "SELECT Id, FirstName, LastName FROM People" +
                           " WHERE ((TRIM(FirstName) = @1))", "John");
        }

        [Alias("People")]
        public new class Person
        {
            public int Id { get; set; }
            [Alias("FirstName")]
            public string Name { get; set; }
            public string LastName { get; set; }
        }
    }
}
