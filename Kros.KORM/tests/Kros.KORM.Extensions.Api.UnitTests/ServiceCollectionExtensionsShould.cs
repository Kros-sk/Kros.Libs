using FluentAssertions;
using Kros.KORM.Extensions.Asp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class ServiceCollectionExtensionsShould
    {
        [Fact]
        public void AddKormToContainer()
        {
            var (configuration, services) = CreateHelpers("standard");

            services.AddKorm(configuration);

            services.BuildServiceProvider()
                .GetService<IDatabase>()
                .Should().NotBeNull();
        }

        [Fact]
        public void ThrowExceptionWhenConfigurationSectionIsMissing()
        {
            var (configuration, services) = CreateHelpers("missingsection");

            Action action = () =>
            {
                services.AddKorm(configuration);
            };

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*Configuration section 'ConnectionString' is missing.*");
        }

        #region Helpers

        private static IConfigurationRoot GetConfiguration(string configName)
            => new ConfigurationBuilder().AddJsonFile($"appsettings.{configName}.json").Build();

        private static (IConfigurationRoot configuration, IServiceCollection services) CreateHelpers(string configName)
            => (GetConfiguration(configName), new ServiceCollection());

        #endregion
    }
}
