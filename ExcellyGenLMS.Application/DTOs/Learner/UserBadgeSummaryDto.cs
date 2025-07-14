using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class UserBadgeSummaryDto
    {
        public int TotalBadges { get; set; }
        public int EarnedCount { get; set; }
        public int ReadyToClaimCount { get; set; }
        public List<BadgeDto> RecentBadges { get; set; } = new List<BadgeDto>();
    }
}