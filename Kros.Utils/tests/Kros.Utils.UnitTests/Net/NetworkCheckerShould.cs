using FluentAssertions;
using Kros.Net;
using System;
using System.IO;
using Xunit;

namespace Kros.Utils.UnitTests.Net
{
    public class NetworkCheckerShould
    {

        #region Nested classes

        private class FakeWebClient : IWebClient
        {
            private readonly bool _throwException;

            public FakeWebClient(bool throwExpcetion)
            {
                _throwException = throwExpcetion;
            }

            public void Dispose()
            {
            }

            public Stream OpenRead(string address)
            {
                if (_throwException)
                {
                    throw new Exception();
                }
                else
                {
                    return new MemoryStream();
                }
            }
        }

        private class TestNetworkChecker : NetworkChecker
        {
            public TestNetworkChecker(bool throwException)
                : base("www.kros.sk", 2, 200)
            {
                ThrowException = throwException;
            }

            public bool CheckNetworkResponse { get; set; } = true;

            public bool ThrowException { get; set; }

            internal override IWebClient CreateWebClient() => new FakeWebClient(ThrowException);

            internal override bool CheckNetwork() => CheckNetworkResponse;
        }

        #endregion

        #region Tests

        [Fact]
        public void ReturnTrueIfInternetIsAvailable()
        {
            var checker = new TestNetworkChecker(throwException: false);

            checker.IsNetworkAvailable().Should().BeTrue();
        }

        [Fact]
        public void ReturnFalseIfInternetIsNotAvailable()
        {
            var checker = new TestNetworkChecker(throwException: true);

            checker.IsNetworkAvailable().Should().BeFalse();
        }

        [Fact]
        public void CacheLastSuccess()
        {
            var checker = new TestNetworkChecker(throwException: false);

            using (DateTimeProvider.InjectActualDateTime(new DateTime(2017, 10, 11, 15, 3, 3, 10)))
            {
                var available = checker.IsNetworkAvailable();
            }

            using (DateTimeProvider.InjectActualDateTime(new DateTime(2017, 10, 11, 15, 3, 3, 209)))
            {
                checker.ThrowException = true;
                checker.IsNetworkAvailable().Should().BeTrue();
            }
        }

        [Fact]
        public void NotUseCacheIfExpired()
        {
            var checker = new TestNetworkChecker(throwException: false);

            using (DateTimeProvider.InjectActualDateTime(new DateTime(2017, 10, 11, 15, 3, 3, 10)))
            {
                var available = checker.IsNetworkAvailable();
            }

            using (DateTimeProvider.InjectActualDateTime(new DateTime(2017, 10, 11, 15, 3, 3, 211)))
            {
                checker.ThrowException = true;
                checker.IsNetworkAvailable().Should().BeFalse();
            }
        }

        [Fact]
        public void CheckLocalNetwork()
        {
            var checker = new TestNetworkChecker(throwException: false);
            checker.CheckNetworkResponse = false;

            checker.IsNetworkAvailable().Should().BeFalse();
        }

        #endregion

    }
}
