using System;
using System.Collections.Generic;

namespace DAL.Models
{
    /// <summary>
    /// Result summary of a web-check.
    /// </summary>
    public class WebCheckResult
    {
        /// <summary>
        /// Timestamp of the last execution of the check.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Result of the check.
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// The reason(s) for the result.
        /// </summary>
        public IList<string> Reasons { get; set; } = new List<string>();

        /// <summary>
        /// Any output logs of the various response tests.
        /// </summary>
        public IList<(string Name, IList<string> Messages)> TestLogMessages { get; set; } =
            new List<(string Name, IList<string> Messages)>();

        /// <summary>
        /// The duration of the request.
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// Returned status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Id of the corresponding web-check.
        /// </summary>
        public int WebCheckId { get; set; }
    }
}
