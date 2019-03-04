using Kros.Utils;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Kros.KORM.Migrations.Providers
{
    /// <summary>
    /// Migration scripts provider, which load scripts from disk.
    /// </summary>
    public class FileMigrationScriptsProvider : MigrationScriptsProviderBase
    {
        private readonly string _folderPath;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="folderPath">Path to folder with migration scripts.</param>
        public FileMigrationScriptsProvider(string folderPath)
        {
            _folderPath = Check.NotNullOrWhiteSpace(folderPath, nameof(folderPath));
        }

        /// <inheritdoc/>
        protected override string FolderFullPath => _folderPath;

        /// <inheritdoc/>
        public override async Task<string> GetScriptAsync(ScriptInfo scriptInfo)
            => await File.ReadAllTextAsync(scriptInfo.Path);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetScriptPaths() => Directory.GetFiles(_folderPath);
    }
}