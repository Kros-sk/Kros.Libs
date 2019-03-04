using Kros.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kros.KORM.Migrations.Providers
{
    /// <summary>
    /// Migration scripts provider, which load scripts from assembly.
    /// </summary>
    public class AssemblyMigrationScriptsProvider : MigrationScriptsProviderBase
    {
        private const string DefaultResourceNamespace = "Sql_scripts";
        private readonly Assembly _assembly;
        private readonly string _resourceNamespace;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="assembly">Assembly, which contains embedded script resources.</param>
        /// <param name="resourceNamespace">Full namespace, where are placed embedded scripts.</param>
        public AssemblyMigrationScriptsProvider(Assembly assembly, string resourceNamespace)
        {
            _assembly = Check.NotNull(assembly, nameof(assembly));
            _resourceNamespace = Check.NotNullOrWhiteSpace(resourceNamespace, nameof(resourceNamespace));
        }

        /// <inheritdoc />
        protected override string FolderFullPath => _resourceNamespace;

        /// <summary>
        /// Create defaut <see cref="AssemblyMigrationScriptsProvider"/>, which load script from executing assembly.
        /// </summary>
        /// <returns><see cref="AssemblyMigrationScriptsProvider"/>.</returns>
        internal static AssemblyMigrationScriptsProvider Default()
        {
            var assembly = Assembly.GetCallingAssembly();

            return new AssemblyMigrationScriptsProvider(assembly, $"{assembly.GetName().Name}.{DefaultResourceNamespace}");
        }

        /// <inheritdoc/>
        public override async Task<string> GetScriptAsync(ScriptInfo scriptInfo)
        {
            var resourceStream = _assembly.GetManifestResourceStream(scriptInfo.Path);
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <inheritdoc/>
        protected override IEnumerable<string> GetScriptPaths() => _assembly.GetManifestResourceNames();
    }
}