using ExcellyGenLMS.Core.Entities.Learner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IBadgeRepository
    {
        Task<IEnumerable<Badge>> GetAllBadgesAsync();
        Task<IEnumerable<UserBadge>> GetUserBadgesByUserIdAsync(string userId);
        Task<UserBadge?> GetUserBadgeAsync(string userId, string badgeId);
        Task<UserBadge> AddUserBadgeAsync(UserBadge userBadge);
        Task UpdateUserBadgeAsync(UserBadge userBadge);
    }
}