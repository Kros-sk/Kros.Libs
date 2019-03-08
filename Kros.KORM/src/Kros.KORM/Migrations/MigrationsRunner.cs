﻿using Kros.Utils;
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

            var lastMigration = GetLastMigrationInfo() ?? Migration.None;
            var migrationScripts = GetMigrationScriptsToExecute(lastMigration).ToList();

            if (migrationScripts.Any())
            {
                await ExecuteMigrationScripts(migrationScripts);
            }
        }

        private async Task ExecuteMigrationScripts(IEnumerable<ScriptInfo> migrationScripts)
        {
            using (var transaction = _database.BeginTransaction())
            {
                foreach (var scriptInfo in migrationScripts)
                {
                    Migration newMigration = await CreateNewMigrationInfo(scriptInfo);

                    var script = await scriptInfo.GetScriptAsync();
                    await ExecuteMigrationScript(script);

                    await UpdateLastMigrationInfo(newMigration);
                }

                transaction.Commit();
            }
        }

        private IEnumerable<ScriptInfo> GetMigrationScriptsToExecute(Migration lastMigration)
            => _migrationOptions.Providers.SelectMany(p => p.GetScripts())
            .OrderBy(p => p.Id)
            .Where(p => p.Id > lastMigration.MigrationId);

        private async Task<Migration> CreateNewMigrationInfo(ScriptInfo scriptInfo)
        {
            var newMigration = new Migration()
            {
                MigrationId = scriptInfo.Id,
                MigrationName = scriptInfo.Name,
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

        private async Task ExecuteMigrationScript(string script)
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
                Environment.NewLine + Properties.Resources.MigrationsHistoryTableScript;

            await _database.ExecuteNonQueryAsync(sql);
        }
    }
}
