using FluentAssertions;
using Kros.Data.MsAccess;
using Kros.KORM.Metadata.Attribute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kros.KORM.MsAccess.UnitTests.Integration
{
    public class DatabaseShould
    {
        #region Nested Classes

        [Alias("LimitOffsetTest")]
        private class LimitOffsetTestData
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region SQL Scripts

        private const string Table_LimitOffsetTest = "LimitOffsetTest";

        private static string[] LimitOffsetInitScripts = new[]
        {
            $"CREATE TABLE [{Table_LimitOffsetTest}] ([Id] LONG NOT NULL, [Value] VARCHAR(50) NULL)",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (1, 'one')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (2, 'two')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (3, 'three')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (4, 'four')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (5, 'fice')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (6, 'six')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (7, 'seven')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (8, 'eight')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (9, 'nine')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (10, 'ten')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (11, 'eleven')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (12, 'twelve')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (13, 'thirteen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (14, 'fourteen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (15, 'fifteen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (16, 'sixteen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (17, 'seventeen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (18, 'eighteen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (19, 'nineteen')",
            $"INSERT INTO [{Table_LimitOffsetTest}] VALUES (20, 'twenty')"
        };

        #endregion

        #region Limit/Offset

        [SkippableFact]
        public void ReturnOnlyFirstNRows_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            ReturnOnlyFirstNRowsCore(ProviderType.Ace);
        }

        [SkippableFact]
        public void ReturnOnlyFirstNRows_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            ReturnOnlyFirstNRowsCore(ProviderType.Jet);
        }

        private void ReturnOnlyFirstNRowsCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, LimitOffsetInitScripts))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() { Id = 1, Value = "one" },
                    new LimitOffsetTestData() { Id = 2, Value = "two" },
                    new LimitOffsetTestData() { Id = 3, Value = "three" }
                });

                List<LimitOffsetTestData> data = helper.Korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Take(3)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [SkippableFact]
        public void SkipFirstNRows_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            SkipFirstNRowsCore(ProviderType.Ace);
        }

        [SkippableFact]
        public void SkipFirstNRows_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            SkipFirstNRowsCore(ProviderType.Jet);
        }

        private void SkipFirstNRowsCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, LimitOffsetInitScripts))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() { Id = 18, Value = "eighteen" },
                    new LimitOffsetTestData() { Id = 19, Value = "nineteen" },
                    new LimitOffsetTestData() { Id = 20, Value = "twenty" }
                });

                List<LimitOffsetTestData> data = helper.Korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(17)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [SkippableFact]
        public void SkipFirstNRowsAndReturnNextMRows_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            SkipFirstNRowsAndReturnNextMRowsCore(ProviderType.Ace);
        }

        [SkippableFact]
        public void SkipFirstNRowsAndReturnNextMRows_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            SkipFirstNRowsAndReturnNextMRowsCore(ProviderType.Jet);
        }

        private void SkipFirstNRowsAndReturnNextMRowsCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, LimitOffsetInitScripts))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() { Id = 6, Value = "six" },
                    new LimitOffsetTestData() { Id = 7, Value = "seven" },
                    new LimitOffsetTestData() { Id = 8, Value = "eight" }
                });

                List<LimitOffsetTestData> data = helper.Korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(5)
                    .Take(3)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [SkippableFact]
        public void ReturnNoRowsWhenSkipIsTooBig_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            ReturnNoRowsWhenSkipIsTooBigCore(ProviderType.Ace);
        }

        [SkippableFact]
        public void ReturnNoRowsWhenSkipIsTooBig_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            ReturnNoRowsWhenSkipIsTooBigCore(ProviderType.Jet);
        }

        private void ReturnNoRowsWhenSkipIsTooBigCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, LimitOffsetInitScripts))
            {
                var expectedData = new List<LimitOffsetTestData>();

                List<LimitOffsetTestData> data = helper.Korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(100)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        [SkippableFact]
        public void ReturnAllRemainigRowsWhenTakeIsTooBig_Ace()
        {
            Helpers.SkipTestIfAceProviderNotAvailable();
            ReturnAllRemainigRowsWhenTakeIsTooBigCore(ProviderType.Ace);
        }

        [SkippableFact]
        public void ReturnAllRemainigRowsWhenTakeIsTooBig_Jet()
        {
            Helpers.SkipTestIfJetProviderNotAvailable();
            ReturnAllRemainigRowsWhenTakeIsTooBigCore(ProviderType.Jet);
        }

        private void ReturnAllRemainigRowsWhenTakeIsTooBigCore(ProviderType provider)
        {
            using (var helper = Helpers.CreateDatabase(provider, LimitOffsetInitScripts))
            {
                var expectedData = new List<LimitOffsetTestData>(new[] {
                    new LimitOffsetTestData() { Id = 19, Value = "nineteen" },
                    new LimitOffsetTestData() { Id = 20, Value = "twenty" },
                });

                List<LimitOffsetTestData> data = helper.Korm.Query<LimitOffsetTestData>()
                    .OrderBy(item => item.Id)
                    .Skip(18)
                    .Take(100)
                    .ToList();

                data.Should().BeEquivalentTo(expectedData);
            }
        }

        #endregion
    }
}
