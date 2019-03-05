using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    internal class ConfigurationHelper
    {
        private static IConfigurationRoot GetConfiguration(string configName)
            => new ConfigurationBuilder().AddJsonFile($"appsettings.{configName}.json").Build();

        internal static (IConfigurationRoot configuration, IServiceCollection services) CreateHelpers(string configName)
            => (GetConfiguration(configName), new ServiceCollection());
    }
}
