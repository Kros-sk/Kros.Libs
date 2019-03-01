﻿using FluentAssertions;
using Kros.KORM.Migrations.Providers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using static System.IO.Path;

namespace Kros.KORM.Extensions.Api.UnitTests.Migrations.Providers
{
    public class FileMigrationScriptsProviderShould
    {
        private string _folderFullPath;

        public FileMigrationScriptsProviderShould()
        {
            _folderFullPath = Combine(
                GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Resources",
                "ScriptsFromFiles");
        }

        [Fact]
        public void GetScriptFromDefinedAsemblyAndNamespace()
        {
            var provider = new FileMigrationScriptsProvider(_folderFullPath);
            var scripts = provider.GetScripts().ToList();

            scripts.Count.Should().Be(3);
            scripts[0].Should()
                .BeEquivalentTo(
                new ScriptInfo()
                {
                    Id = 20190228001,
                    Name = "InitDatabase",
                    Path = GetFileFullName("20190228001_InitDatabase")
                });
            scripts[1].Should()
                .BeEquivalentTo(
                new ScriptInfo()
                {
                    Id = 20190301001,
                    Name = "AddPeopleTable",
                    Path = GetFileFullName("20190301001_AddPeopleTable")
                });
            scripts[2].Should()
                .BeEquivalentTo(
                new ScriptInfo()
                {
                    Id = 20190301002,
                    Name = "AddProjectTable",
                    Path = GetFileFullName("20190301002_AddProjectTable")
                });
        }

        [Fact]
        public async Task LoadScript()
        {
            var provider = new FileMigrationScriptsProvider(_folderFullPath);
            var script = await provider.LoadScriptAsync(new ScriptInfo()
            {
                Id = 20190228001,
                Name = "InitDatabase",
                Path = GetFileFullName("20190228001_InitDatabase")
            });

            var expected = await File.ReadAllTextAsync(GetFileFullName("20190228001_InitDatabase"));
            script.Should().Be(expected);
        }

        private string GetFileFullName(string fileName)
                => $"{_folderFullPath}\\{fileName}.sql";
    }
}