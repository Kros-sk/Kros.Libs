using Kros.Utils;

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
            throw new System.NotImplementedException();
        }
    }
}
