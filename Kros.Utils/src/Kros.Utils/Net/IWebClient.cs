using System;
using System.IO;

namespace Kros.Net
{
    /// <summary>
    /// Simple interface describing a web client for using in tests.
    /// </summary>
    public interface IWebClient : IDisposable
    {
        /// <summary>
        /// Opens a readable stream for the data downloaded from a resource with the URI.
        /// </summary>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A <see cref="Stream"/> used to read data from a resource.</returns>
        Stream OpenRead(string address);
    }
}
