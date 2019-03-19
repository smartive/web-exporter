using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DAL.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus.Advanced;
using Prometheus.Advanced.DataContracts;
using Smartive.Core.Database.Repositories;

namespace WebApp.Metrics
{
    /// <summary>
    /// Collector for prometheus metrics.
    /// </summary>
    public class PrometheusCollector : ICollector
    {
        private const string Namespace = "web_exporter_webcheck";
        private readonly ILogger<PrometheusCollector> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates the collector
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="serviceProvider">Provider.</param>
        public PrometheusCollector(ILogger<PrometheusCollector> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Name of the collector.
        /// </summary>
        public string Name => nameof(PrometheusCollector);

        /// <summary>
        /// Endpoint specific labels.
        /// </summary>
        public string[] LabelNames => new string[] { };

        /// <summary>
        /// Collect method that gets all metrics and returns them.
        /// </summary>
        /// <returns>List of metrics</returns>
        public IEnumerable<MetricFamily> Collect()
        {
            _logger.LogTrace("/metrics endpoint is probed.");
            return CollectAsync().Result;
        }

        private async Task<IEnumerable<MetricFamily>> CollectAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var repo = scope.ServiceProvider.GetService<ICrudRepository<WebCheck>>();
                var results = scope.ServiceProvider.GetService<WebCheckResults>();

                var webChecks = await repo
                    .AsQueryable()
                    .Include(wc => wc.Labels)
                    .ToListAsync();

                var executedChecks = results
                    .AtomicGetResultsForChecks(webChecks)
                    .Where(tuple => tuple.Result != null)
                    .ToList();

                var total = new MetricFamily
                {
                    name = $"{Namespace}_total",
                    type = MetricType.GAUGE,
                    help = "Counts the total amount of actually registered web-checks in the system.",
                    metric = { new Metric { gauge = new Gauge { value = executedChecks.Count } } }
                };

                var failTotal = new MetricFamily
                {
                    name = $"{Namespace}_failures_total",
                    type = MetricType.GAUGE,
                    help = "Counts the total amount of failed web-checks.",
                    metric =
                    {
                        new Metric { gauge = new Gauge { value = executedChecks.Count(wc => !wc.Result.Result) } }
                    }
                };

                var callDuration = new MetricFamily
                {
                    name = $"{Namespace}_duration_milliseconds",
                    type = MetricType.GAUGE,
                    help = "Indicates how long (in ms) the request took to get an answer."
                };

                var statusCode = new MetricFamily
                {
                    name = $"{Namespace}_status_code",
                    type = MetricType.GAUGE,
                    help = "The returned http status code."
                };

                var testResult = new MetricFamily
                {
                    name = $"{Namespace}_result",
                    type = MetricType.GAUGE,
                    help = "Contains the result of the web-check. 1: true, 0: false."
                };

                var executionTime = new MetricFamily
                {
                    name = $"{Namespace}_timestamp",
                    type = MetricType.COUNTER,
                    help = "Indicates when the test was executed last."
                };

                foreach (var (check, result) in executedChecks)
                {
                    var labels = new List<LabelPair>
                        {
                            new LabelPair { name = "id", value = check.Id.ToString() },
                            new LabelPair { name = "name", value = check.Name },
                            new LabelPair { name = "url", value = check.Url },
                            new LabelPair { name = "method", value = check.Method.ToString() }
                        }
                        .Concat(check.Labels.Select(l => new LabelPair { name = l.Name, value = l.Value }))
                        .ToList();

                    callDuration.metric.Add(
                        new Metric
                        {
                            gauge = new Gauge { value = result.Duration },
                            label = labels
                        });

                    statusCode.metric.Add(
                        new Metric
                        {
                            gauge = new Gauge { value = result.StatusCode },
                            label = labels
                        });

                    testResult.metric.Add(
                        new Metric
                        {
                            gauge = new Gauge { value = result.Result ? 1 : 0 },
                            label = labels.Concat(
                                    new[]
                                    {
                                        new LabelPair
                                        {
                                            name = "failure_reason", value = result.Reasons.Join("\n")
                                        }
                                    })
                                .ToList()
                        });

                    executionTime.metric.Add(
                        new Metric
                        {
                            counter = new Counter { value = new DateTimeOffset(result.Timestamp).ToUnixTimeSeconds() },
                            label = labels
                        });
                }

                stopWatch.Stop();

                var duration = new MetricFamily
                {
                    name = $"{Namespace}_probe_duration_milliseconds",
                    type = MetricType.GAUGE,
                    help = "Indicates the duration of the collection and calculation of the metrics.",
                    metric = { new Metric { gauge = new Gauge { value = stopWatch.ElapsedMilliseconds } } }
                };

                return new[]
                {
                    total,
                    duration,
                    failTotal,
                    callDuration,
                    statusCode,
                    testResult,
                    executionTime
                };
            }
        }
    }
}
