using FluentAssertions;
using Kros.Data;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Xunit;

namespace Kros.Utils.UnitTests.Data
{
    public class IdGeneratorFactoriesShould
    {
        [Fact]
        public void GetFactoryByConnection()
        {
            using (var conn = new SqlConnection())
            {
                var factory = IdGeneratorFactories.GetFactory(conn);

                factory.Should().NotBeNull();
            }
        }

        [Fact]
        public void GetFactoryByAdoClientName()
        {
            var factory = IdGeneratorFactories.GetFactory("connectionstring", "System.Data.SqlClient");

            factory.Should().NotBeNull();
        }

        [Fact]
        public void ThrowExceptionWhenConnectionIsNotRegisterd()
        {
            using (var conn = new CustomConnection())
            {
                Action action = () => { var factory = IdGeneratorFactories.GetFactory(conn); };

                action.Should().Throw<InvalidOperationException>()
                    .WithMessage("*CustomConnection*");
            }
        }

        [Fact]
        public void ThrowExceptionWhenAdoClientNameIsNotRegistered()
        {
            Action action = () => { var factory = IdGeneratorFactories.GetFactory("constring", "System.Data.CustomClient"); };

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*System.Data.CustomClient*");
        }
    }
}
