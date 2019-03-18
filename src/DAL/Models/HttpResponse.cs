using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace DAL.Models
{
    /// <summary>
    /// Mapping class for http responses.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Duration of the call in ms.
        /// </summary>
        public long CallDuration { get; set; }

        /// <summary>
        /// Status code of the http response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Stringified content of the response (if any).
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// HTTP Reason Phrase
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// List of headers returned by the server.
        /// </summary>
        public IReadOnlyList<HttpMinimalHeader> Headers { get; set; }

        /// <summary>
        /// Convenience accessor for "successful" status code.
        /// </summary>
        public bool IsSuccess => StatusCode < 300;

        /// <summary>
        /// Create a response class from a http response message.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="duration">The duration of the call in ms.</param>
        /// <returns>A task that resolves to the created response.</returns>
        public static async Task<HttpResponse> CreateFromResponse(HttpResponseMessage response, long duration)
        {
            return new HttpResponse
            {
                CallDuration = duration,
                StatusCode = (int)response.StatusCode,
                Reason = response.ReasonPhrase,
                Content = await response.Content.ReadAsStringAsync(),
                Headers = response.Headers
                    .Select(
                        header => new HttpMinimalHeader { Name = header.Key, Value = header.Value.Join() })
                    .ToList()
            };
        }
    }
}
