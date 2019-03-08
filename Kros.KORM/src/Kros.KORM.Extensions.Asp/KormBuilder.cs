using Kros.Data;
using Kros.KORM.Extensions.Asp.Properties;
using Kros.KORM.Migrations;
using Kros.KORM.Migrations.Middleware;
using Kros.KORM.Migrations.Providers;
using Kros.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Builder for initialization of <see cref="IDatabase"/>.
    /// </summary>
    public class KormBuilder
    {
        private ConnectionStringSettings _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="KormBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The connection string settings.</param>
        public KormBuilder(IServiceCollection services, ConnectionStringSettings connectionString)
        {
            Services = Check.NotNull(services, nameof(services));
            _connectionString = Check.NotNull(connectionString, nameof(connectionString));
        }

        /// <summary>
        /// Gets the service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Initializes database for using Id generator.
        /// </summary>
        /// <returns>This instance.</returns>
        public KormBuilder InitDatabaseForIdGenerator()
        {
            var factory = IdGeneratorFactories.GetFactory(_connectionString.ConnectionString, _connectionString.ProviderName);

            using (var idGenerator = factory.GetGenerator(string.Empty))
            {
                idGenerator.InitDatabaseForIdGenerator();
            }

            return this;
        }

        private const string MigrationSectionName = "KormMigrations";
        private const string ConnectionStringSectionName = "ConnectionString";
        private const string AutoMigrateSectionName = "AutoMigrate";
        private bool _autoMigrate = false;

        /// <summary>
        /// Adds configuration for <see cref="MigrationsMiddleware"/> into <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="setupAction">Setup migration options.</param>
        /// <returns>This instance of <see cref="KormBuilder"/>.</returns>
        public KormBuilder AddKormMigrations(
            IConfiguration configuration,
            Action<MigrationOptions> setupAction = null)
        {
            IConfigurationSection migrationsConfig = GetMigrationsSection(configuration);
            _autoMigrate = migrationsConfig.GetValue(AutoMigrateSectionName, false);
            var connectionString = migrationsConfig
                .GetSection(ConnectionStringSectionName).Get<ConnectionStringSettings>();

            Services
                .AddMemoryCache()
                .AddTransient((Func<IServiceProvider, IMigrationsRunner>)((s) =>
                {
                    var database = new Database(connectionString);

                    MigrationOptions options = SetupMigrationOptions(setupAction);

                    return new MigrationsRunner(database, options);
                }));

            return this;
        }

        private static MigrationOptions SetupMigrationOptions(Action<MigrationOptions> setupAction)
        {
            MigrationOptions options = new MigrationOptions();

            if (setupAction != null)
            {
                setupAction.Invoke(options);
            }
            else
            {
                options.AddScriptsProvider(AssemblyMigrationScriptsProvider.GetEntryAssemblyProvider());
            }

            return options;
        }

        private static IConfigurationSection GetMigrationsSection(IConfiguration configuration)
        {
            var migrationsConfig = configuration.GetSection(MigrationSectionName);
            if (!migrationsConfig.Exists())
            {
                throw new InvalidOperationException(
                    string.Format(Resources.ConfigurationSectionIsMissing, MigrationSectionName));
            }

            return migrationsConfig;
        }

        /// <summary>
        /// Execute database migration.
        /// </summary>
        public void Migrate()
        {
            if (_autoMigrate)
            {
                Services.BuildServiceProvider()
                    .GetService<IMigrationsRunner>()
                    .MigrateAsync()
                    .Wait();
            }
        }
    }
}
