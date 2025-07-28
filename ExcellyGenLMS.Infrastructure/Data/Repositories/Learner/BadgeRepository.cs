using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly ApplicationDbContext _context;

        public BadgeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Badge>> GetAllBadgesAsync()
        {
            return await _context.Badges.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<UserBadge>> GetUserBadgesByUserIdAsync(string userId)
        {
            return await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Badge)
                .ToListAsync();
        }

        public async Task<UserBadge?> GetUserBadgeAsync(string userId, string badgeId)
        {
            return await _context.UserBadges
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badgeId);
        }

        public async Task<UserBadge> AddUserBadgeAsync(UserBadge userBadge)
        {
            _context.UserBadges.Add(userBadge);
            await _context.SaveChangesAsync();
            return userBadge;
        }

        public async Task UpdateUserBadgeAsync(UserBadge userBadge)
        {
            _context.UserBadges.Update(userBadge);
            await _context.SaveChangesAsync();
        }
    }
}