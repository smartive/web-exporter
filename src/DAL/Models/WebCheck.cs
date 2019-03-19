using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Smartive.Core.Database.Models;

namespace DAL.Models
{
    /// <summary>
    /// Web-check that is periodically executed and tested. When the metrics endpoint is called,
    /// all current registered web-checks are returned with their last saved results.
    /// </summary>
    public class WebCheck : Base
    {
        /// <summary>
        /// Name of the check.
        /// </summary>
        /// <example>Check our api</example>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Url that is called.
        /// </summary>
        [Required]
        [Url]
        public string Url { get; set; }

        /// <summary>
        /// <see cref="HttpMethod"/> that is used to call the <see cref="Url"/>.
        /// </summary>
        [Required]
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        /// List of associated request headers.
        /// </summary>
        public IList<RequestHeader> RequestHeaders { get; set; }

        /// <summary>
        /// List of associated metric labels.
        /// </summary>
        public IList<Label> Labels { get; set; }

        /// <summary>
        /// List of associated response tests.
        /// </summary>
        public IList<ResponseTest> ResponseTests { get; set; }

        /// <summary>
        /// Creates and returns a http request message for this web-check.
        /// </summary>
        [JsonIgnore]
        public HttpRequestMessage HttpRequest
        {
            get
            {
                var request = new HttpRequestMessage
                {
                    Method = SystemHttpMethod,
                    RequestUri = new Uri(Url ?? "https://smartive.ch")
                };

                foreach (var header in (RequestHeaders ?? new List<RequestHeader>()).Where(
                    header => !string.IsNullOrWhiteSpace(header.Name)))
                {
                    request.Headers.Add(header.Name, header.Value);
                }

                if (!request.Headers.Contains("Accept"))
                {
                    request.Headers.Add("Accept", "*/*");
                }

                return request;
            }
        }

        private System.Net.Http.HttpMethod SystemHttpMethod
        {
            get
            {
                switch (Method)
                {
                    case HttpMethod.Put:
                        return System.Net.Http.HttpMethod.Put;
                    case HttpMethod.Post:
                        return System.Net.Http.HttpMethod.Post;
                    case HttpMethod.Delete:
                        return System.Net.Http.HttpMethod.Delete;
                    case HttpMethod.Head:
                        return System.Net.Http.HttpMethod.Head;
                    default:
                        return System.Net.Http.HttpMethod.Get;
                }
            }
        }
    }
}
