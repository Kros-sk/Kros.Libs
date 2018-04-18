using System;
using System.IO;

namespace Kros.Net
{
    /// <summary>
    /// Interface popisujúci WebClient-a. Aby sme v testoch mohli podvrhovať vlastné implementácie.
    /// </summary>
    public interface IWebClient : IDisposable
    {
        /// <summary>
        /// Opens a readable stream for the data downloaded from a resource with the URI.
        /// </summary>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A System.IO.Stream used to read data from a resource.</returns>
        Stream OpenRead(string address);
    }
}
