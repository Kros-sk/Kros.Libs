using Kros.Data;
using Kros.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Builder for initialization <see cref="IDatabase"/>.
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
    }
}
