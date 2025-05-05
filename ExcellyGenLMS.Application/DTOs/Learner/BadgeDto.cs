using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class BadgeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? EarnedDate { get; set; }
    }

    public class UserBadgeSummaryDto
    {
        public int TotalBadges { get; set; }
        public int ThisMonth { get; set; }
        public List<BadgeDto> RecentBadges { get; set; } = new List<BadgeDto>();
    }
}