using ExcellyGenLMS.Application.DTOs.Learner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IBadgesAndRewardsService
    {
        Task<IEnumerable<BadgeDto>> GetBadgesAndRewardsAsync(string userId);
        Task<BadgeDto> ClaimBadgeAsync(string userId, string badgeId);
    }
}