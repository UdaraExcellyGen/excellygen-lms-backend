using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IUserBadgeService
    {
        Task<List<BadgeDto>> GetUserBadgesAsync(string userId);
        Task<UserBadgeSummaryDto> GetUserBadgeSummaryAsync(string userId);
    }
}