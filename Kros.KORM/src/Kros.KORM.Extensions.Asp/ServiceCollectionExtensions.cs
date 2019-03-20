using Kros.Extensions;
using Kros.KORM.Extensions.Asp.Properties;
using Kros.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Extensions for registering <see cref="IDatabase"/> into DI container.
    /// </summary>
    /// <example>
    /// "ConnectionString": {
    ///   "ProviderName": "System.Data.SqlClient",
    ///   "ConnectionString": "Server=servername\\instancename;Initial Catalog=database;Persist Security Info=False;"
    /// }
    /// </example>
    public static class ServiceCollectionExtensions
    {
        private const string ConnectionStringSectionName = "ConnectionString";

        /// <summary>
        /// Register KORM into DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>
        /// If 'ConnectionString' section is missing in configuration file.
        /// </item>
        /// <item>
        /// If <see cref="ConnectionStringSettings.ConnectionString"/> or
        /// <see cref="ConnectionStringSettings.ProviderName"/> are not filled.
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>;
        /// </exception>
        public static KormBuilder AddKorm(this IServiceCollection services, IConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));
            Check.NotNull(services, nameof(services));

            return AddKorm(services, configuration.GetSection(ConnectionStringSectionName));
        }

        /// <summary>
        /// Register KORM into DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>
        /// If <paramref name="configurationSection"/> doesn't exist in configuration file.
        /// </item>
        /// <item>
        /// If <see cref="ConnectionStringSettings.ConnectionString"/> or
        /// <see cref="ConnectionStringSettings.ProviderName"/> are not filled.
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// When <paramref name="services"/> or <paramref name="configurationSection"/> is null;
        /// </exception>
        public static KormBuilder AddKorm(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(configurationSection, nameof(configurationSection));

            if (!configurationSection.Exists())
            {
                throw new InvalidOperationException(
                    string.Format(Resources.ConfigurationSectionIsMissing, configurationSection.Key));
            }

            var connectionString = configurationSection.Get<ConnectionStringSettings>();
            CheckOptions(connectionString);

            services.AddScoped<IDatabase>((serviceProvider) =>
            {
                return new Database(connectionString);
            });

            return new KormBuilder(services, connectionString);
        }

        private static void CheckOptions(ConnectionStringSettings connectionString)
        {
            if (connectionString.ConnectionString.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(Resources.ConnectionStringIsRequired);
            }
            if (connectionString.ProviderName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(Resources.ProviderNameIsRequired);
            }
        }
    }
}
