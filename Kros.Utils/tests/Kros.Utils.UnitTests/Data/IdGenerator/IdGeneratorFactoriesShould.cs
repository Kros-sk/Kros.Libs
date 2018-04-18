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

                action.ShouldThrow<InvalidOperationException>()
                    .WithMessage("IIdGeneratorFactory for connection type 'CustomConnection' is not registered.");
            }
        }

        [Fact]
        public void ThrowExceptionWhenAdoClientNameIsNotRegistered()
        {
            Action action = () => { var factory = IdGeneratorFactories.GetFactory("constring", "System.Data.CustomClient"); };

            action.ShouldThrow<InvalidOperationException>()
                .WithMessage("IIdGeneratorFactory for ADO client 'System.Data.CustomClient' is not registered.");
        }

        private class CustomConnection : DbConnection
        {
            public override string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override string Database => throw new NotImplementedException();

            public override string DataSource => throw new NotImplementedException();

            public override string ServerVersion => throw new NotImplementedException();

            public override ConnectionState State => throw new NotImplementedException();

            public override void ChangeDatabase(string databaseName)
            {
                throw new NotImplementedException();
            }

            public override void Close()
            {
                throw new NotImplementedException();
            }

            public override void Open()
            {
                throw new NotImplementedException();
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                throw new NotImplementedException();
            }

            protected override DbCommand CreateDbCommand()
            {
                throw new NotImplementedException();
            }
        }
    }
}
