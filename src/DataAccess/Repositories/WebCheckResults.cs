using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DAL.Models;

namespace DataAccess.Repositories
{
    /// <summary>
    /// In-Memory representation of the web check results.
    /// Should be used as a singleton!
    /// </summary>
    public class WebCheckResults
    {
        private readonly object _lock = new object();
        private Dictionary<int, WebCheckResult> _results = new Dictionary<int, WebCheckResult>();

        /// <summary>
        /// Atomically set the results.
        /// </summary>
        /// <param name="results">Results.</param>
        public void AtomicSetResults(IEnumerable<WebCheckResult> results)
        {
            lock (_lock)
            {
                _results = results.ToDictionary(result => result.WebCheckId);
            }
        }

        /// <summary>
        /// Atomically set a result.
        /// </summary>
        /// <param name="result">The result.</param>
        public void AtomicSetResult(WebCheckResult result)
        {
            lock (_lock)
            {
                _results[result.WebCheckId] = result;
            }
        }

        /// <summary>
        /// Return a list of web-checks with their corresponding results.
        /// If a test was not executed yet, the result will be null.
        /// </summary>
        /// <param name="checks">List of checks.</param>
        /// <returns>List of tuples with the check and the result or null.</returns>
        public IEnumerable<(WebCheck Check, WebCheckResult Result)> AtomicGetResultsForChecks(
            IEnumerable<WebCheck> checks)
        {
            lock (_lock)
            {
                return checks.Select(check => (check, _results.ContainsKey(check.Id) ? _results[check.Id] : null));
            }
        }

        /// <summary>
        /// Return result for check.
        /// </summary>
        /// <param name="check">The check.</param>
        /// <returns>The result.</returns>
        public WebCheckResult AtomicGetResultForCheck(WebCheck check)
        {
            lock (_lock)
            {
                return _results.ContainsKey(check.Id)
                    ? _results[check.Id]
                    : null;
            }
        }
    }
}
