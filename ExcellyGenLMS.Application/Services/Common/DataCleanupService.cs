using ExcellyGenLMS.Core.Interfaces.Repositories.Learner; // Corrected using statement
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Common
{
    public interface IDataCleanupService
    {
        Task PruneOldActivityLogsAsync();
    }

    public class DataCleanupService : IDataCleanupService
    {
        private readonly IUserActivityLogRepository _activityLogRepository; // Corrected
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(IUserActivityLogRepository activityLogRepository, ILogger<DataCleanupService> logger) // Corrected
        {
            _activityLogRepository = activityLogRepository; // Corrected
            _logger = logger;
        }

        public async Task PruneOldActivityLogsAsync()
        {
            var retentionPeriodInWeeks = 4;
            var cutoffDate = DateTime.UtcNow.AddDays(-7 * retentionPeriodInWeeks);

            _logger.LogInformation("Starting to prune user activity logs older than {CutoffDate}", cutoffDate);

            try
            {
                // This now correctly calls the repository, which contains the database logic.
                var rowsAffected = await _activityLogRepository.PruneOldLogsAsync(cutoffDate);

                _logger.LogInformation("Successfully pruned {RowsAffected} old user activity logs.", rowsAffected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while pruning old user activity logs.");
            }
        }
    }
}