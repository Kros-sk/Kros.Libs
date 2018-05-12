using FluentAssertions;
using Kros.Data.MsAccess;
using Kros.UnitTests;
using Xunit;

namespace Kros.Utils.MsAccess.UnitTests
{
    public class MsAccessTestHelperShould
    {
        [Fact]
        public void CreateDatabaseWhenDatabasePathIsUsed()
        {
            using (var helper = new MsAccessTestHelper(ProviderType.Jet, @"./Resources/MsAccessTestHelper.mdb"))
            {
                var connection = helper.Connection;

                connection.Should().NotBeNull();
            }
        }
    }
}
