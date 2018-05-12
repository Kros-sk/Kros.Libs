using Kros.Utils;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Kros.Net
{
    /// <summary>
    /// Class dedicated for simple testing of internet connectivity.
    /// </summary>
    /// <remarks>
    /// It is not sufficient to test connectivity using <see cref="NetworkInterface.GetIsNetworkAvailable()"/>, because that
    /// method just checks, if the computer is in some network. It does not check if internet is really available.
    /// Internet availability is not checked using ping (<see cref="Ping"/>), because this method is often blocked.
    /// The availability is tested using a request to specific service.
    /// </remarks>
    public class NetworkChecker
    {
        #region Nested classes

        private class KrosWebClient : WebClient, IWebClient
        {
            public KrosWebClient(int timeout)
            {
                Timeout = timeout;
            }

            public int Timeout { get; }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest request = base.GetWebRequest(uri);
                request.Timeout = Timeout;

                return request;
            }
        }

        #endregion

        #region Fields

        private const int DefaultRequestTimeout = 1 * 1000;
        private const int DefaultResponseCacheExpiration = 3 * 60 * 1000;
        private DateTime _lastSuccessResponseTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkChecker"/> with address <paramref name="serviceAddress"/>
        /// for requests.
        /// </summary>
        /// <param name="serviceAddress">Wen address for requests checking internet availability.</param>
        public NetworkChecker(string serviceAddress)
            : this(serviceAddress, DefaultRequestTimeout, DefaultResponseCacheExpiration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkChecker"/> with address <paramref name="serviceAddress"/>
        /// and aditional parameters.
        /// </summary>
        /// <param name="serviceAddress">Wen address for requests checking internet availability.</param>
        /// <param name="requestTimeout">Maximum time for waiting for the response from server. If the response will not
        /// came in this time, we consider that the internet is not available. Value is in milliseconds.</param>
        /// <param name="responseCacheExpiration">Time in milliseconds during which the last response will be remembered
        /// and so no requests to <paramref name="serviceAddress"/> will be performed.
        /// </param>
        public NetworkChecker(string serviceAddress, int requestTimeout, int responseCacheExpiration)
        {
            ServiceAddress = Check.NotNullOrWhiteSpace(serviceAddress, nameof(serviceAddress));
            RequestTimeout = Check.GreaterOrEqualThan(requestTimeout, 0, nameof(requestTimeout));
            ResponseCacheExpiration = Check.GreaterOrEqualThan(responseCacheExpiration, 0, nameof(responseCacheExpiration));
        }

        #endregion

        #region NetworkChecker

        /// <summary>
        /// Web address to which requests are made to check internet availability.
        /// </summary>
        public string ServiceAddress { get; }

        /// <summary>
        /// Maximum time for waiting for the response from server. If the response will not
        /// came in this time, we consider that the internet is not available. Value is in milliseconds.
        /// </summary>
        public int RequestTimeout { get; }

        /// <summary>
        /// Time in milliseconds during which the last response will be remembered
        /// and so no other requests to <see cref="ServiceAddress"/> will be performed.
        /// </summary>
        public int ResponseCacheExpiration { get; }

        /// <summary>
        /// Checks if the internet (specifically the service at the address <see cref="ServiceAddress"/>) is available.
        /// Positive response is cached for the time specified in <see cref="ResponseCacheExpiration"/>,
        /// so another request to the service is made after this time.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if internet (service) is available <see langword="false"/> otherwise.
        /// </returns>
        public bool IsNetworkAvailable() => CheckNetwork() && (HasCachedResponse() || CheckService());

        internal virtual bool CheckNetwork() => NetworkInterface.GetIsNetworkAvailable();

        private bool HasCachedResponse() =>
            _lastSuccessResponseTime.AddMilliseconds(ResponseCacheExpiration) >= DateTimeProvider.Now;

        private bool CheckService()
        {
            try
            {
                using (var wc = CreateWebClient())
                using (var stream = wc.OpenRead(ServiceAddress))
                {
                    _lastSuccessResponseTime = DateTimeProvider.Now;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        internal virtual IWebClient CreateWebClient() => new KrosWebClient(RequestTimeout);

        #endregion
    }
}
