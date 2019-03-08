using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kros.KORM.Migrations.Providers
{
    /// <summary>
    /// Base class for migration scripts provider.
    /// </summary>
    public abstract class MigrationScriptsProviderBase : IMigrationScriptsProvider
    {
        /// <summary>
        /// Path to folder with migration scripts.
        /// </summary>
        protected abstract string FolderFullPath { get; }

        /// <inheritdoc />
        public IEnumerable<ScriptInfo> GetScripts()
        {
            const string extension = ".sql";
            const char idNameSeparator = '_';
            var scriptPaths = GetScriptPaths();
            var startIndex = FolderFullPath.Length;

            return scriptPaths
                .Where(s => s.StartsWith(FolderFullPath) && s.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .Select(s =>
                {
                    var splits = s.Substring(startIndex + 1, s.Length - startIndex - extension.Length - 1)
                        .Split(idNameSeparator);

                    return new ScriptInfo(this)
                    {
                        Id = long.Parse(splits[0]),
                        Name = splits[1],
                        Path = s
                    };
                })
                .OrderBy(s => s.Id);
        }

        /// <summary>
        /// Get migration script paths.
        /// </summary>
        /// <returns>Migration script paths.</returns>
        protected abstract IEnumerable<string> GetScriptPaths();

        /// <inheritdoc />
        public abstract Task<string> GetScriptAsync(ScriptInfo scriptInfo);
    }
}
