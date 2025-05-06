using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class UserBadgeRepository : IUserBadgeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserBadgeRepository> _logger;

        public UserBadgeRepository(
            ApplicationDbContext context,
            ILogger<UserBadgeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<UserBadge>> GetUserBadgesAsync(string userId)
        {
            return await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.EarnedDate)
                .ToListAsync();
        }

        public async Task<int> GetUserBadgeCountAsync(string userId)
        {
            return await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .CountAsync();
        }

        public async Task<int> GetUserBadgeCountThisMonthAsync(string userId)
        {
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            return await _context.UserBadges
                .Where(ub => ub.UserId == userId && ub.EarnedDate >= firstDayOfMonth)
                .CountAsync();
        }


        public async Task<List<UserBadge>> GetUserRecentBadgesAsync(string userId, int count = 3)
        {
            return await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.EarnedDate)
                .Take(count)
                .ToListAsync();
        }
    }
}