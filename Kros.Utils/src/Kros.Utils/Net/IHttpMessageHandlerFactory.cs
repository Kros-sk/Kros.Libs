using System.Net.Http;

namespace Kros.Net
{
    /// <summary>
    /// Factory to create <see cref="HttpMessageHandler"/> used in Kros.Net services.
    /// </summary>
    public interface IHttpMessageHandlerFacotry
    {
        /// <summary>
        /// Creates <see cref="HttpMessageHandler"/> instance.
        /// </summary>
        HttpMessageHandler CreateHttpMessageHandler();
    }
}
