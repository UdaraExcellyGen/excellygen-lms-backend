using ExcellyGenLMS.Core.Entities.Learner;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IUserActivityLogRepository
    {
        Task AddAsync(UserActivityLog log);
        Task<IEnumerable<UserActivityLog>> GetRecentActivityForUserAsync(string userId, DateTime startDate);

        // THIS IS THE NEW METHOD FOR CLEANUP
        Task<int> PruneOldLogsAsync(DateTime cutoffDate);
    }
}