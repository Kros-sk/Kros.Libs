using Kros.Utils;
using System;
using System.Net;

namespace Kros.Net
{
    /// <summary>
    /// Trieda určená na testovanie dostupnosti internetového spojenia.
    /// </summary>
    /// <remarks>
    /// Nestačí testovať pomocou <see cref="System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()"/>,
    /// pretože táto metóda zistí či je počítač v sieti. Nezistí či je reálne dostupný internet.
    /// Taktiež netestujeme pomocou pingovania, pretože niektorí správcovia zakazujú ping.
    ///
    /// Takže testujeme pomocou dotazu na konkrétnu službu.
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
        /// Initializes a new instance of the <see cref="NetworkChecker"/> class.
        /// </summary>
        /// <param name="testingAddress">Webová adresa služby, ktorú testujeme.</param>
        public NetworkChecker(string testingAddress)
            : this(testingAddress, DefaultRequestTimeout, DefaultResponseCacheExpiration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkChecker"/> class.
        /// </summary>
        /// <param name="serviceAddress">Webová adresa služby, ktorú testujeme.</param>
        /// <param name="requestTimeout">
        /// Maximálna doba čakania na odpoveď zo server. Keď uplynie a odpoveď sme nedostali, tak považujeme,
        /// že internet/služba nie je dostupný.
        /// V milisekundách.
        /// </param>
        /// <param name="responseCacheExpiration">
        /// čas v milisekundách, ktorý udáva ako dlho sa bude používať zapamätaná informácia o tom, že ide internet.
        /// </param>
        public NetworkChecker(string serviceAddress, int requestTimeout, int responseCacheExpiration)
        {
            Check.NotNullOrWhiteSpace(serviceAddress, nameof(serviceAddress));
            Check.GreaterOrEqualThan(requestTimeout, 0, nameof(requestTimeout));
            Check.GreaterOrEqualThan(responseCacheExpiration, 0, nameof(responseCacheExpiration));

            ServiceAddress = serviceAddress;
            RequestTimeout = requestTimeout;
            ResponseCacheExpiration = responseCacheExpiration;
        }

        #endregion

        #region NetworkChecker

        /// <summary>
        /// Webová adresa služby, ktorú testujeme.
        /// </summary>
        public string ServiceAddress { get; }

        /// <summary>
        /// Maximálna doba čakania na odpoveď zo server. Keď uplynie a odpoveď sme nedostali, tak považujeme,
        /// že internet/služba nie je dostupný. Hodnota je v milisekundách.</summary>
        public int RequestTimeout { get; }

        /// <summary>
        /// Hodnota, ktorá udáva ako dlho sa bude používať zapamätaná informácia o tom, že internet je dostupný. V milisekundách.
        /// </summary>
        public int ResponseCacheExpiration { get; }

        /// <summary>
        /// Skontroluje či je internet (konkrétna služba <see cref="ServiceAddress"/>) k dispozícií.
        /// Kladnú odpoveď kešuje a reálny test prevedie len pokiaľ uplynul čas platnosti keše
        /// <see cref="ResponseCacheExpiration"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/>, ak je (respektíve v danom časovom intervale bol dostupný internet).
        /// <see langword="false"/> ak internet nie je dostupný.
        /// </returns>
        public bool IsNetworkAvailable() =>
            CheckNetwork() && (HasCachedResponse() || CheckService());

        internal virtual bool CheckNetwork() =>
            System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

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

        internal virtual IWebClient CreateWebClient() =>
            new KrosWebClient(RequestTimeout);

        #endregion

    }
}
