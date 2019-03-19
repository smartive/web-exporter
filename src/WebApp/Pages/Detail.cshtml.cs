using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smartive.Core.Database.Repositories;
using WebApp.Metrics;

namespace WebApp.Pages
{
    /// <summary>
    /// PageModel for `/detail/{id}`
    /// </summary>
    public class Detail : PageModel
    {
        private readonly ILogger<Detail> _logger;
        private readonly ICrudRepository<WebCheck> _webChecks;
        private readonly ICrudRepository<Label> _labels;
        private readonly ICrudRepository<RequestHeader> _headers;
        private readonly ICrudRepository<ResponseTest> _tests;
        private readonly WebCheckExecutor _executor;
        private readonly WebCheckResults _results;

        /// <summary>
        /// Crate a new detail page model.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="webChecks">WebChecks Repository</param>
        /// <param name="labels">Labels Repository</param>
        /// <param name="headers">Headers Repository</param>
        /// <param name="tests">ResponseTest Repository</param>
        /// <param name="executor">Test Executor</param>
        /// <param name="results">Results</param>
        public Detail(
            ILogger<Detail> logger,
            ICrudRepository<WebCheck> webChecks,
            ICrudRepository<Label> labels,
            ICrudRepository<RequestHeader> headers,
            ICrudRepository<ResponseTest> tests,
            WebCheckExecutor executor,
            WebCheckResults results)
        {
            _logger = logger;
            _webChecks = webChecks;
            _labels = labels;
            _headers = headers;
            _tests = tests;
            _executor = executor;
            _results = results;
        }

        /// <summary>
        /// Defines if the given web-check model is new or not.
        /// </summary>
        public bool IsNew => WebCheck.Id == default;

        /// <summary>
        /// The web-check in question.
        /// </summary>
        [BindProperty]
        public WebCheck WebCheck { get; set; } = new WebCheck
        {
            Labels = new List<Label>(),
            RequestHeaders = new List<RequestHeader>()
        };

        /// <summary>
        /// Corresponding result of the check (if any).
        /// </summary>
        public WebCheckResult Result { get; private set; }

        /// <summary>
        /// List of response tests.
        /// </summary>
        public IEnumerable<(int Id, string Name)> ResponseTests { get; private set; }

        /// <summary>
        /// Get.
        /// </summary>
        /// <param name="id">Optional Id to get.</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            _logger.LogTrace($"GET: Detail with id {id}");
            if (!id.HasValue)
            {
                return Page();
            }

            WebCheck = await _webChecks
                .AsQueryable()
                .Include(wc => wc.Labels)
                .Include(wc => wc.RequestHeaders)
                .SingleOrDefaultAsync(wc => wc.Id == id);

            if (WebCheck == null)
            {
                _logger.LogDebug($"WebCheck with id {id} not found. Redirect.");
                return RedirectToPage("Index");
            }

            ResponseTests = (await _tests
                    .AsQueryable()
                    .Where(t => t.WebCheckId == WebCheck.Id)
                    .Select(t => new { t.Id, t.Name })
                    .ToListAsync())
                .Select(
                    result => (
                        result.Id, result.Name));

            Result = _results.AtomicGetResultForCheck(WebCheck);

            return Page();
        }

        /// <summary>
        /// Get Execute Test
        /// </summary>
        /// <param name="id">Id of the test</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> OnGetExecuteTestAsync(int id)
        {
            _logger.LogTrace($"GET: Detail - handler execute test - with id {id}");

            var check = await _webChecks
                .AsQueryable()
                .Include(wc => wc.RequestHeaders)
                .Include(wc => wc.ResponseTests)
                .SingleOrDefaultAsync(wc => wc.Id == id);

            if (check == null)
            {
                _logger.LogWarning($"WebCheck with id '{id}' not found.");
                return RedirectToPage("Index");
            }

            _results.AtomicSetResult((await _executor.Execute(check)).result);

            return RedirectToPage(new { id });
        }

        /// <summary>
        /// Post.
        /// </summary>
        /// <returns>Action Result</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogTrace("POST: Detail");

            if (!ModelState.IsValid)
            {
                _logger.LogDebug("Model-state is not valid, return page.");
                return Page();
            }

            using (var transaction = await _webChecks.BeginTransaction())
            {
                var webCheck = await _webChecks.Save(WebCheck);
                await _labels.SynchronizeCollection(
                    labels => labels.Where(label => label.WebCheckId == webCheck.Id),
                    webCheck.Labels ?? new List<Label>());
                await _headers.SynchronizeCollection(
                    headers => headers.Where(header => header.WebCheckId == webCheck.Id),
                    webCheck.RequestHeaders ?? new List<RequestHeader>());

                try
                {
                    transaction.Commit();
                    _logger.LogDebug($"WebCheck '{webCheck.Name}' saved.");
                    return RedirectToPage(new { id = webCheck.Id });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "There was an exception during save / sync of a webcheck");
                    return RedirectToPage("Index");
                }
            }
        }
    }
}
