using ExcellyGenLMS.Application.Services.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Workers
{
    public class ScheduledCleanupWorker : BackgroundService
    {
        private readonly ILogger<ScheduledCleanupWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ScheduledCleanupWorker(ILogger<ScheduledCleanupWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduled Cleanup Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Scheduled Cleanup Worker running at: {time}", DateTimeOffset.Now);

                // Create a new scope to resolve scoped services like the DbContext
                using (var scope = _serviceProvider.CreateScope())
                {
                    var cleanupService = scope.ServiceProvider.GetRequiredService<IDataCleanupService>();
                    await cleanupService.PruneOldActivityLogsAsync();
                }

                // Wait for 24 hours before running again
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Scheduled Cleanup Worker is stopping.");
        }
    }
}