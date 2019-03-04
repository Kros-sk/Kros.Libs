using Kros.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Kros.KORM.Migrations.Providers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Kros.KORM.Migrations
{
    /// <summary>
    /// Runner for execution database migrations.
    /// </summary>
    public class MigrationsRunner : IMigrationsRunner
    {
        private readonly IDatabase _database;
        private readonly MigrationOptions _migrationOptions;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="database">Database connection.</param>
        /// <param name="migrationOptions">Migration options</param>
        public MigrationsRunner(
            IDatabase database,
            MigrationOptions migrationOptions)
        {
            _database = Check.NotNull(database, nameof(database));
            _migrationOptions = Check.NotNull(migrationOptions, nameof(migrationOptions));
        }

        /// <inheritdoc />
        public async Task MigrateAsync()
        {
            await InitMigrationsHistoryTable();

            var lastMigration = GetLastMigrationInfo() ?? new Migration() { MigrationId = 0 };
            var migrationScripts = _migrationOptions.Providers.SelectMany(p => p.GetScripts().Select(s => new
            {
                Script = s,
                Provider = p
            }))
            .OrderBy(p => p.Script.Id)
            .Where(p => p.Script.Id > lastMigration.MigrationId)
            .ToList();

            if (migrationScripts.Any())
            {
                using (var transaction = _database.BeginTransaction())
                {
                    Migration newMigration = await CreateNewMigrationInfo(migrationScripts.Select(p => p.Script));

                    foreach (var scriptInfo in migrationScripts)
                    {
                        var script = await scriptInfo.Provider.LoadScriptAsync(scriptInfo.Script);
                        await ExecuteScript(script);
                    }

                    await UpdateLastMigrationInfo(newMigration);

                    transaction.Commit();
                }
            }
        }

        private async Task<Migration> CreateNewMigrationInfo(IEnumerable<ScriptInfo> migrationScripts)
        {
            var newMigration = new Migration()
            {
                MigrationId = migrationScripts.Last().Id,
                MigrationName = migrationScripts.Last().Name,
                Updated = null,
                ProductInfo = Assembly.GetEntryAssembly().FullName
            };

            var dbSet = _database.Query<Migration>()
                .AsDbSet();
            dbSet.Add(newMigration);
            await dbSet.CommitChangesAsync();

            return newMigration;
        }

        private async Task UpdateLastMigrationInfo(Migration newMigration)
        {
            newMigration.Updated = DateTime.Now;
            var dbSet = _database.Query<Migration>()
                .AsDbSet();
            dbSet.Edit(newMigration);

            await dbSet.CommitChangesAsync();
        }

        private async Task ExecuteScript(string script)
        {
            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(script);

            foreach (string line in lines.Where(p => p.Length > 0))
            {
                await _database.ExecuteNonQueryAsync(line);
            }
        }

        private Migration GetLastMigrationInfo()
            => _database.Query<Migration>()
            .OrderByDescending(p => p.MigrationId)
            .FirstOrDefault();

        private async Task InitMigrationsHistoryTable()
        {
            var sql = $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{Migration.TableName}' AND type = 'U')" +
                    Environment.NewLine + GetSqlIdGeneratorTableScript();
            await _database.ExecuteNonQueryAsync(sql);
        }

        private string GetSqlIdGeneratorTableScript() =>
            GetResourceContent("Kros.KORM.Extensions.Asp.Resources.MigrationsHistoryTableScript.sql");

        private static string GetResourceContent(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
