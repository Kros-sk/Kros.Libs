using FluentAssertions;
using Kros.KORM.Migrations.Providers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kros.KORM.UnitTests.Migrations.Providers
{
    public class AssemblyMigrationScriptsProviderShould
    {
        [Fact]
        public void GetScriptsFromDefaultNamespace()
        {
            string GetNamespace(string fileName)
                => $"Kros.KORM.UnitTests.Sql_scripts.{fileName}.sql";

            AssemblyMigrationScriptsProvider provider = CreateDefaultProvider();
            var scripts = provider.GetScripts().ToList();

            scripts.Count.Should().Be(3);
            scripts.Should().Equals(new[] {
                new ScriptInfo(provider)
                {
                    Id = 20190228001,
                    Name = "InitDatabase",
                    Path = GetNamespace("20190228001_InitDatabase")
                },
                new ScriptInfo(provider)
                {
                    Id = 20190301001,
                    Name = "AddPeopleTable",
                    Path = GetNamespace("20190301001_AddPeopleTable")
                },
                new ScriptInfo(provider)
                {
                    Id = 20190301002,
                    Name = "AddProjectTable",
                    Path = GetNamespace("20190301002_AddProjectTable")
                }
            });
        }

        private static AssemblyMigrationScriptsProvider CreateDefaultProvider()
            => new AssemblyMigrationScriptsProvider(
                Assembly.GetExecutingAssembly(),
                "Kros.KORM.UnitTests.Sql_scripts");

        [Fact]
        public void GetScriptFromDefinedAsemblyAndNamespace()
        {
            string GetNamespace(string fileName)
                => $"Kros.KORM.UnitTests.Resources.AnotherSqlScripts.{fileName}.sql";

            var provider = new AssemblyMigrationScriptsProvider(
                Assembly.GetExecutingAssembly(),
                "Kros.KORM.UnitTests.Resources.AnotherSqlScripts");
            var scripts = provider.GetScripts().ToList();

            scripts.Count.Should().Be(2);
            scripts.Should().Equals(new[]
            {
                new ScriptInfo(provider)
                {
                    Id = 20190227001,
                    Name = "InitDatabase",
                    Path = GetNamespace("20190227001_InitDatabase")
                },
                new ScriptInfo(provider)
                {
                    Id = 20190227002,
                    Name = "AddProjectTable",
                    Path = GetNamespace("20190227002_AddProjectTable")
                }
            });
        }

        [Fact]
        public async Task LoadScript()
        {
            var provider = CreateDefaultProvider();
            var script = await provider.GetScriptAsync(new ScriptInfo(provider)
            {
                Id = 20190228001,
                Name = "InitDatabase",
                Path = "Kros.KORM.UnitTests.Sql_scripts.20190228001_InitDatabase.sql"
            });

            var expected = Properties.Resources._20190228001_InitDatabase;

            script.Should().Be(expected);
        }
    }
}
