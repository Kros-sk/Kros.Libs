using FluentAssertions;
using Kros.KORM.Migrations.Providers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests.Migrations.Providers
{
    public class AssemblyMigrationScriptsProviderShould
    {
        [Fact]
        public void GetScriptsFromDefaultNamespace()
        {
            string GetNamespace(string fileName)
                => $"Kros.KORM.Extensions.Api.UnitTests.Sql_scripts.{fileName}.sql";

            var provider = AssemblyMigrationScriptsProvider.Default();
            var scripts = provider.GetScripts().ToList();

            scripts.Count.Should().Be(3);
            scripts[0].Should()
                .BeEquivalentTo(
                new ScriptInfo(provider)
                {
                    Id = 20190228001,
                    Name = "InitDatabase",
                    Path = GetNamespace("20190228001_InitDatabase")
                });
            scripts[1].Should()
                .BeEquivalentTo(
                new ScriptInfo(provider)
                {
                    Id = 20190301001,
                    Name = "AddPeopleTable",
                    Path = GetNamespace("20190301001_AddPeopleTable")
                });
            scripts[2].Should()
                .BeEquivalentTo(
                new ScriptInfo(provider)
                {
                    Id = 20190301002,
                    Name = "AddProjectTable",
                    Path = GetNamespace("20190301002_AddProjectTable")
                });
        }

        [Fact]
        public void GetScriptFromDefinedAsemblyAndNamespace()
        {
            string GetNamespace(string fileName)
                => $"Kros.KORM.Extensions.Api.UnitTests.Resources.AnotherSqlScripts.{fileName}.sql";

            var provider = new AssemblyMigrationScriptsProvider(
                Assembly.GetExecutingAssembly(),
                "Kros.KORM.Extensions.Api.UnitTests.Resources.AnotherSqlScripts");
            var scripts = provider.GetScripts().ToList();

            scripts.Count.Should().Be(2);
            scripts[0].Should()
                .BeEquivalentTo(
                new ScriptInfo(provider)
                {
                    Id = 20190227001,
                    Name = "InitDatabase",
                    Path = GetNamespace("20190227001_InitDatabase")
                });
            scripts[1].Should()
                .BeEquivalentTo(
                new ScriptInfo(provider)
                {
                    Id = 20190227002,
                    Name = "AddProjectTable",
                    Path = GetNamespace("20190227002_AddProjectTable")
                });
        }

        [Fact]
        public async Task LoadScript()
        {
            var provider = AssemblyMigrationScriptsProvider.Default();
            var script = await provider.GetScriptAsync(new ScriptInfo(provider)
            {
                Id = 20190228001,
                Name = "InitDatabase",
                Path = "Kros.KORM.Extensions.Api.UnitTests.Sql_scripts.20190228001_InitDatabase.sql"
            });

            var expected = await GetStringFromResourceFileAsync(
                "Kros.KORM.Extensions.Api.UnitTests.Sql_scripts.20190228001_InitDatabase.sql");

            script.Should().Be(expected);
        }

        private static async Task<string> GetStringFromResourceFileAsync(string resourceFile)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceFile);
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
