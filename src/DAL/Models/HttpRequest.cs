using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore.Internal;

namespace DAL.Models
{
    /// <summary>
    /// Mapping class for the HttpRequestMessage.
    /// </summary>
    public class HttpRequest
    {
        /// <summary>
        /// Create a request.
        /// </summary>
        public HttpRequest()
        {
        }

        /// <summary>
        /// Create a request from a request message.
        /// </summary>
        /// <param name="request">The http request message.</param>
        public HttpRequest(HttpRequestMessage request)
        {
            Method = request.Method.Method;
            Uri = request.RequestUri.ToString();
            Headers = request.Headers
                .Select(
                    header => new HttpMinimalHeader { Name = header.Key, Value = header.Value.Join() })
                .ToList();
        }

        /// <summary>
        /// Returns the string representation of the http method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Used url for the request.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// List of headers used by the request.
        /// </summary>
        public IReadOnlyList<HttpMinimalHeader> Headers { get; set; }
    }
}
