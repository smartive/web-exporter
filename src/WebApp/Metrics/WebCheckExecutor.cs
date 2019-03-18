using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DAL.Models;
using DotNetEnv;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace WebApp.Metrics
{
    /// <summary>
    /// WebCheckExecutor. Runs a given check and returns the results.
    /// </summary>
    public class WebCheckExecutor
    {
        private static readonly int Interval = Env.GetInt(
            "HTTP_TIMEOUT",
            Env.GetInt("PROBE_INTERVAL", 60));

        private static readonly HttpClient Client = new HttpClient(
            new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            }) { Timeout = TimeSpan.FromSeconds(Interval) };

        private readonly ILogger<WebCheckExecutor> _logger;
        private readonly INodeServices _nodeServices;

        /// <summary>
        /// Create the executor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="nodeServices">NodeServices</param>
        public WebCheckExecutor(
            ILogger<WebCheckExecutor> logger,
            INodeServices nodeServices)
        {
            _logger = logger;
            _nodeServices = nodeServices;
        }

        /// <summary>
        /// Execute the given web-check and return the result.
        /// </summary>
        /// <param name="webCheck">The web-check in question.</param>
        /// <returns>Tuple with request, response and result.</returns>
        public async Task<(HttpRequest request, HttpResponse response, WebCheckResult result)> Execute(
            WebCheck webCheck)
        {
            _logger.LogDebug($"HTTP_TIMEOUT / PROBE_INTERVAL set to {Interval}");
            _logger.LogDebug(
                $"Executing web-check '{webCheck.Name}' (id: {webCheck.Id}) to url '{webCheck.Url}' with {webCheck.ResponseTests?.Count} tests.");

            try
            {
                var watch = new Stopwatch();
                var clientRequest = webCheck.HttpRequest;

                watch.Start();
                var clientResponse = Client.SendAsync(clientRequest).Result;
                watch.Stop();

                var httpRequest = new HttpRequest(clientRequest);
                var httpResponse = await HttpResponse.CreateFromResponse(
                    clientResponse,
                    watch.ElapsedMilliseconds);

                if (webCheck.ResponseTests == null || webCheck.ResponseTests.Count <= 0)
                {
                    _logger.LogInformation(
                        $"WebCheck '{webCheck.Name}' (id: {webCheck.Id}) has no response tests, using status-code '{httpResponse.StatusCode}'.");
                    return (httpRequest, httpResponse, new WebCheckResult
                    {
                        Result = httpResponse.IsSuccess,
                        Reasons = new[] { $"Response status code {httpResponse.StatusCode}" },
                        Duration = httpResponse.CallDuration,
                        StatusCode = httpResponse.StatusCode,
                        WebCheckId = webCheck.Id
                    });
                }

                var result = true;
                var reasons = new List<string>();
                var logs = new List<(string Name, IList<string> Messages)>();

                foreach (var test in webCheck.ResponseTests)
                {
                    try
                    {
                        var sw = new Stopwatch();
                        _logger.LogDebug($"Execute response test '{test.Name}'");
                        sw.Start();
                        var testResult = await _nodeServices.InvokeAsync<NodeResult>(
                            "Node/responseTest.js",
                            httpRequest,
                            httpResponse,
                            test.Script);
                        sw.Stop();
                        _logger.LogDebug(
                            $"ResponseTest '{test.Name}' returned '{testResult}' and needed {sw.ElapsedMilliseconds}ms for execution.");

                        if (testResult.Logs.Length > 0)
                        {
                            _logger.LogInformation($"ResponseTest '{test.Name}' had log messages.");
                            _logger.LogDebug(
                                $"ResponseTest '{test.Name}' wrote the following logs:\n{testResult.Logs.Join("\n")}");
                            logs.Add((test.Name, Messages: testResult.Logs));
                        }

                        if (!testResult.Result)
                        {
                            reasons.Add($"ResponseTest '{test.Name}' returned 'false'");
                        }

                        result = result && testResult.Result;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(
                            e,
                            $"ResponseTest '{test.Name}' of WebCheck '{webCheck.Name}' (id: {webCheck.Id}) threw an Exception.");
                        result = false;
                        reasons.Add($"ResponseTest '{test.Name}' threw an Exception: {e.Message}");
                    }
                }

                _logger.LogInformation(
                    $"WebCheck '{webCheck.Name}' (id: {webCheck.Id}): Call was successful, Test results returned {result}.");
                return (httpRequest, httpResponse, new WebCheckResult
                {
                    Result = result,
                    Reasons = reasons,
                    TestLogMessages = logs,
                    Duration = httpResponse.CallDuration,
                    StatusCode = httpResponse.StatusCode,
                    WebCheckId = webCheck.Id
                });
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning(
                    $"WebCheck '{webCheck.Name}' (id: {webCheck.Id}) ran into timeout after {Interval} seconds.");
                return (null, null, new WebCheckResult
                {
                    Result = false,
                    Reasons = new[] { $"HTTP client timeout after {Interval} seconds" },
                    Duration = Interval * 1000,
                    StatusCode = -1,
                    WebCheckId = webCheck.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"WebCheck '{webCheck.Name}' (id: {webCheck.Id}) produced an exception during the execution.");
                return (null, null, new WebCheckResult
                {
                    Result = false,
                    Reasons = new[] { $"Exception during execution: {ex.Message}" },
                    Duration = -1,
                    StatusCode = -1,
                    WebCheckId = webCheck.Id
                });
            }
        }

        /// <summary>
        /// Execute multiple web-checks and return the results.
        /// </summary>
        /// <param name="webChecks">The web-checks in question.</param>
        /// <returns>List of results.</returns>
        public async Task<IEnumerable<(HttpRequest request, HttpResponse response, WebCheckResult result)>> Execute(
            IEnumerable<WebCheck> webChecks)
        {
            return await Task.WhenAll(webChecks.Select(Execute));
        }

        private class NodeResult
        {
            public bool Result { get; set; }

            public string[] Logs { get; set; }
        }
    }
}
