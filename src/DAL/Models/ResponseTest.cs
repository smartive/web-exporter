using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Smartive.Core.Database.Models;

namespace DAL.Models
{
    /// <summary>
    /// Test that is executed after a web-check. The response of the request will be tested
    /// against all registered response tests of a check.
    /// </summary>
    public class ResponseTest : Base
    {
        /// <summary>
        /// Default value for the script field.
        /// </summary>
        private const string DefaultScript = @"responseTest(
  (request, response) => {
    return response.statusCode === 200;
  },
);
";

        /// <summary>
        /// Name of the test
        /// </summary>
        /// <example>Return status code</example>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// JavaScript that is executed for the test. The type definition of the script function is:
        /// `(request, response) => boolean`.
        /// </summary>
        /// <example>
        /// responseTest(
        ///   (request, response) => {
        ///    return response.statusCode === 200;
        ///   },
        /// );
        /// </example>
        [Required]
        public string Script { get; set; } = DefaultScript;

        /// <summary>
        /// Reference to the web-check.
        /// </summary>
        [Required]
        public int WebCheckId { get; set; }

        /// <summary>
        /// Web-Check navigation property.
        /// </summary>
        [JsonIgnore]
        public WebCheck WebCheck { get; set; }
    }
}
