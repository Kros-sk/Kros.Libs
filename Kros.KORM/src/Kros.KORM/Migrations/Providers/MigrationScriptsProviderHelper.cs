using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kros.KORM.Migrations.Providers
{
    internal static class MigrationScriptsProviderHelper
    {
        public static IEnumerable<ScriptInfo> GetScripts(
            this IMigrationScriptsProvider provider,
            string[] scriptPaths,
            string folder)
        {
            const string extension = ".sql";
            const char idNameSeparator = '_';
            var startIndex = folder.Length;

            return scriptPaths
                .Where(s => s.StartsWith(folder) && s.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .Select(s =>
                {
                    var splits = s.Substring(startIndex + 1, s.Length - startIndex - extension.Length - 1)
                        .Split(idNameSeparator);

                    return new ScriptInfo(provider)
                    {
                        Id = long.Parse(splits[0]),
                        Name = splits[1],
                        Path = s
                    };
                })
                .OrderBy(s => s.Id);
        }
    }
}
