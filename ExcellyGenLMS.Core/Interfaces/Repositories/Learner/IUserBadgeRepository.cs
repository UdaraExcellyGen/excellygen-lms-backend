using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Learner;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IUserBadgeRepository
    {
        Task<List<UserBadge>> GetUserBadgesAsync(string userId);
        Task<int> GetUserBadgeCountAsync(string userId);
        Task<int> GetUserBadgeCountThisMonthAsync(string userId);
        Task<List<UserBadge>> GetUserRecentBadgesAsync(string userId, int count = 3);
    }
}