using Kros.Utils;
using System;
using System.IO;
using System.Reflection;

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
        public void Migrate()
        {
            InitMigrationsHistoryTable();
            // ----- 1. Skontroluj či existuje tabuľka
            // ----- 1.a Ak neexsituje
            // -----     - založ tabuľku
            //2. Získaj info o poslednej migrácií
            //3. Získaj zoznam migrácií
            //4. Zisti či treba migrovať?
            //  4.a - Ak nie tak koniec
            //5. Zapíš info o migrácií
            //6. Vykonaj všetky prislúchajúce migrácie
            //7. Zapíš dátum dokončenia
        }

        private void InitMigrationsHistoryTable()
        {
            var sql = $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{_migrationOptions.HistoryTableName}' AND type = 'U')" +
                    Environment.NewLine + GetSqlIdGeneratorTableScript();
            _database.ExecuteNonQuery(sql);
        }

        private string GetSqlIdGeneratorTableScript() =>
            GetResourceContent("Kros.KORM.Extensions.Asp.Resources.MigrationsHistoryTableScript.sql")
            .Replace("#TABLENAME#", _migrationOptions.HistoryTableName);

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
