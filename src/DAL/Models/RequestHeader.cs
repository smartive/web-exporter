using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Smartive.Core.Database.Models;

namespace DAL.Models
{
    /// <summary>
    /// Class for request headers of a web-check.
    /// </summary>
    public class RequestHeader : Base
    {
        /// <summary>
        /// Name of the request header.
        /// </summary>
        /// <example>Authorization</example>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Value of the request header.
        /// </summary>
        /// <example>Basic Zm9vOmJhcg==</example>
        [Required]
        public string Value { get; set; }

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
