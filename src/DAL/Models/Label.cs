using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Smartive.Core.Database.Models;

namespace DAL.Models
{
    /// <summary>
    /// Class for metric labels that are added to a web-check.
    /// </summary>
    public class Label : Base
    {
        /// <summary>
        /// Name of the label.
        /// </summary>
        /// <example>Client</example>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Value of the label.
        /// </summary>
        /// <example>ThisCompany</example>
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
