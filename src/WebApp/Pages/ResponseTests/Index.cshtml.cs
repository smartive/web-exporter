using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smartive.Core.Database.Repositories;
using WebApp.Metrics;

namespace WebApp.Pages.ResponseTests
{
    /// <summary>
    /// PageModel for `/responsetests/{id}`
    /// </summary>
    public class Index : PageModel
    {
        private const string RegexPattern = @"(\/\*\*(.|\n|\r|\t)*?.*Description(.|\n|\r|\t)*?\*\/+?)";

        private const string Description = @"/**
 * Description:
 *
 * This is the test function that will be executed in chakra.
 * When the function throws an error or does not
 * return 'true', the test is assumed to be failing.
 *
 * Included libraries for simplified usage:
 *   - lodash v4.17.11
 *   - jsonpath v1 (https://www.npmjs.com/package/jsonpath)
 *
 * Logging:
 *   - logger global variable (example below)
 *
 * @example Simple status code check:
 * responseTest((req, res) => res.statusCode === 200);
 *
 * @example Check if body contains xy:
 * responseTest((request, response) => {
 *   const body = JSON.parse(response.content);
 *   return _.get(body, 'foo.bar[0].xy') === 'hello world';
 * });
 *
 * @example Logging:
 * responseTest((req, res) => {
 *   logger.info('hello world');
 *   return true;
 * });
 */
";

        private readonly ILogger<Index> _logger;
        private readonly ICrudRepository<ResponseTest> _tests;
        private readonly ICrudRepository<WebCheck> _webChecks;
        private readonly WebCheckExecutor _executor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="tests">ResponseTests Repository</param>
        /// <param name="webChecks">WebCheck Repository</param>
        /// <param name="executor">WebCheckExecutor</param>
        public Index(
            ILogger<Index> logger,
            ICrudRepository<ResponseTest> tests,
            ICrudRepository<WebCheck> webChecks,
            WebCheckExecutor executor)
        {
            _logger = logger;
            _tests = tests;
            _webChecks = webChecks;
            _executor = executor;
        }

        /// <summary>
        /// The response test.
        /// </summary>
        [BindProperty]
        public ResponseTest ResponseTest { get; set; } = new ResponseTest();

        /// <summary>
        /// Script for the test.
        /// </summary>
        [BindProperty]
        public string TestScript
        {
            get => $"{Description}{ResponseTest.Script}";
            set => ResponseTest.Script = Regex
                .Replace(
                    value,
                    RegexPattern,
                    string.Empty,
                    RegexOptions.Multiline | RegexOptions.ECMAScript)
                .TrimStart();
        }

        /// <summary>
        /// Temporary serialized data while executing the test.
        /// </summary>
        [TempData]
        public string ExecutedTestResultString { get; set; }

        /// <summary>
        /// Result that can be shown in the detail page.
        /// </summary>
        public (HttpRequest request, HttpResponse response, WebCheckResult result) ExecutedTestResult { get; set; }

        /// <summary>
        /// Get.
        /// </summary>
        /// <param name="checkId">WebCheckId</param>
        /// <param name="id">Optional Id to get.</param>
        /// <returns>Action result</returns>
        public async Task<IActionResult> OnGetAsync(int checkId, int? id)
        {
            _logger.LogTrace($"GET: Index with checkId {checkId} and id {id}");
            ResponseTest.WebCheckId = checkId;

            if (!id.HasValue)
            {
                return Page();
            }

            ResponseTest = await _tests.GetById(id.Value);
            if (ExecutedTestResultString != null)
            {
                ExecutedTestResult = JsonConvert
                    .DeserializeObject<(HttpRequest request, HttpResponse response, WebCheckResult result)>(
                        ExecutedTestResultString);
            }

            return Page();
        }

        /// <summary>
        /// Delete a response test.
        /// </summary>
        /// <param name="checkId">Id of the web-check</param>
        /// <param name="id">Id of the test</param>
        /// <returns>Task</returns>
        public async Task<IActionResult> OnGetDeleteTestAsync(int checkId, int id)
        {
            _logger.LogTrace($"GET: Index - handler Delete Test - with checkId {checkId} and id {id}");
            await _tests.DeleteById(id);
            return RedirectToPage("../Detail", new { id = checkId });
        }

        /// <summary>
        /// Post.
        /// </summary>
        /// <returns>Action Result</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogTrace("POST: Index");

            if (!ModelState.IsValid)
            {
                _logger.LogDebug("Model-state is not valid, return page.");
                return Page();
            }

            await _tests.Save(ResponseTest);
            return RedirectToPage("../Detail", new { id = ResponseTest.WebCheckId });
        }

        /// <summary>
        /// Post with handler ExecuteTest.
        /// </summary>
        /// <returns>Action Result</returns>
        public async Task<IActionResult> OnPostExecuteTestAsync()
        {
            _logger.LogTrace("POST: Index - handler execute test");

            if (!ModelState.IsValid)
            {
                _logger.LogDebug("Model-state is not valid, return page.");
                return Page();
            }

            await _tests.Save(ResponseTest);

            var check = await _webChecks
                .AsQueryable()
                .Include(wc => wc.ResponseTests)
                .Include(wc => wc.RequestHeaders)
                .SingleAsync(wc => wc.Id == ResponseTest.WebCheckId);

            ExecutedTestResultString = JsonConvert.SerializeObject(await _executor.Execute(check));

            return RedirectToPage("Index", new { id = ResponseTest.Id, checkId = ResponseTest.WebCheckId });
        }
    }
}
