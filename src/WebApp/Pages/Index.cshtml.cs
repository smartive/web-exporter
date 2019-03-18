using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smartive.Core.Database.Repositories;

namespace WebApp.Pages
{
    /// <summary>
    /// PageModel for `/`.
    /// </summary>
    /// <inheritdoc />
    public class Index : PageModel
    {
        private readonly ILogger<Index> _logger;
        private readonly ICrudRepository<WebCheck> _webChecks;
        private readonly WebCheckResults _results;

        /// <summary>
        /// Create a new index page model.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="webChecks">WebChecks</param>
        /// <param name="results">WebCheckResults</param>
        public Index(ILogger<Index> logger, ICrudRepository<WebCheck> webChecks, WebCheckResults results)
        {
            _logger = logger;
            _webChecks = webChecks;
            _results = results;
        }

        /// <summary>
        /// List of registered web-checks with results.
        /// </summary>
        public IEnumerable<(WebCheck check, WebCheckResult result)> WebChecks { get; set; }

        /// <summary>
        /// Get.
        /// </summary>
        /// <returns>Task</returns>
        public async Task OnGetAsync()
        {
            _logger.LogTrace("GET: Index");
            WebChecks = _results
                .AtomicGetResultsForChecks(await _webChecks.AsQueryable().Include(wc => wc.Labels).ToListAsync())
                .OrderBy(result => result.Result?.Result)
                .ThenBy(result => result.Check.Name);
        }

        /// <summary>
        /// Delete webcheck
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> OnGetDeleteWebCheckAsync(int id)
        {
            _logger.LogTrace($"GET: Index - handler DeleteWebCheck - id {id}");
            await _webChecks.DeleteById(id);
            return RedirectToPage("Index");
        }
    }
}
