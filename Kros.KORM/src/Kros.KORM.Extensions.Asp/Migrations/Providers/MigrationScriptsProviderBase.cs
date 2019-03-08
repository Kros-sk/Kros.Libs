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
            const string idNameSeparator = "_";
            var scriptPaths = GetScriptPaths();
            var startIndex = FolderFullPath.Length;

            return scriptPaths
                .Where(r => r.StartsWith(FolderFullPath) && r.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                .Select(r =>
                {
                    var splits = r.Substring(startIndex + 1, r.Length - startIndex - extension.Length - 1)
                        .Split(idNameSeparator);

                    return new ScriptInfo(this)
                    {
                        Id = long.Parse(splits[0]),
                        Name = splits[1],
                        Path = r
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
