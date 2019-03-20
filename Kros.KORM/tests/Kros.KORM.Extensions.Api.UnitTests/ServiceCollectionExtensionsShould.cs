using FluentAssertions;
using Kros.KORM.Extensions.Asp;
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
            var (configuration, services) = ConfigurationHelper.CreateHelpers("standard");

            services.AddKorm(configuration);

            services.BuildServiceProvider()
                .GetService<IDatabase>()
                .Should().NotBeNull();
        }

        [Fact]
        public void ThrowExceptionWhenConfigurationSectionIsMissing()
        {
            var (configuration, services) = ConfigurationHelper.CreateHelpers("missingsection");

            Action action = () =>
            {
                services.AddKorm(configuration);
            };

            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("*Configuration section 'ConnectionString' is missing.*");
        }
    }
}
