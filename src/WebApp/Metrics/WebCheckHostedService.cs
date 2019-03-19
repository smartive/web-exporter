using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAL.Models;
using DataAccess.Repositories;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Smartive.Core.Database.Repositories;

namespace WebApp.Metrics
{
    /// <summary>
    /// Hosted service that runs all registered web-checks in a defined interval.
    /// </summary>
    public class WebCheckHostedService : IHostedService, IDisposable
    {
        private static readonly int Interval = Env.GetInt("PROBE_INTERVAL", 60);
        private readonly ILogger<WebCheckHostedService> _logger;
        private readonly IServiceProvider _services;

        private Timer _timer;
        private Task _webCheckTask;

        /// <summary>
        /// Create the service.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="services">ServiceProvider.</param>
        public WebCheckHostedService(ILogger<WebCheckHostedService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Start.
        /// </summary>
        /// <param name="cancellationToken">Token.</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(PerformWebCheckInterval, null, TimeSpan.Zero, TimeSpan.FromSeconds(Interval));
            _logger.LogInformation("WebCheckExecutor successfully started.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop.
        /// </summary>
        /// <param name="cancellationToken">Token.</param>
        /// <returns>Task</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            _logger.LogInformation("WebCheckExecutor successfully stopped.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void PerformWebCheckInterval(object state)
        {
            if (_webCheckTask != null)
            {
                _logger.LogWarning(
                    $"WebCheck execution overlaps @ {DateTime.Now:yyyy.MM.dd-HH:mm:ss}. Aborting this round.");
                return;
            }

            _logger.LogInformation($"Perform WebChecks @ {DateTime.Now:yyyy.MM.dd-HH:mm:ss}");
            _webCheckTask = PerformWebChecks();
            _webCheckTask.Wait();
            _webCheckTask = null;
            _logger.LogDebug("WebCheck execution finished.");
        }

        private async Task PerformWebChecks()
        {
            using (var scope = _services.CreateScope())
            {
                _logger.LogDebug("Created scope and executing checks now.");
                var repo = scope.ServiceProvider.GetService<ICrudRepository<WebCheck>>();
                var results = scope.ServiceProvider.GetService<WebCheckResults>();
                var executor = scope.ServiceProvider.GetService<WebCheckExecutor>();

                var checks = await repo
                    .AsQueryable()
                    .Include(wc => wc.RequestHeaders)
                    .Include(wc => wc.ResponseTests)
                    .ToListAsync();

                results.AtomicSetResults(
                    (await executor.Execute(checks)).Select(executorResult => executorResult.result));
            }
        }
    }
}
