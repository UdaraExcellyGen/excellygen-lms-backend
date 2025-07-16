using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class UserActivityLogRepository : IUserActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public UserActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserActivityLog log)
        {
            await _context.UserActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetRecentActivityForUserAsync(string userId, DateTime startDate)
        {
            return await _context.UserActivityLogs
                .Where(log => log.UserId == userId && log.ActivityTimestamp.Date >= startDate)
                .ToListAsync();
        }

        // THIS IS THE IMPLEMENTATION OF THE NEW METHOD
        public async Task<int> PruneOldLogsAsync(DateTime cutoffDate)
        {
            // Use a high-performance, direct SQL command for bulk deletion
            return await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM UserActivityLogs WHERE ActivityTimestamp < {0}",
                cutoffDate
            );
        }
    }
}