using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    /// <summary>
    /// This service is now considered deprecated. 
    /// All badge-related logic is handled by BadgesAndRewardsService.
    /// This file is kept to prevent build errors in other parts of the application that may still reference it.
    /// </summary>
    public class UserBadgeService : IUserBadgeService
    {
        public UserBadgeService()
        {
            // The constructor is empty as this service no longer has responsibilities.
        }
    }
}