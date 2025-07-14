using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class LeaderboardDto
    {
        public List<LeaderboardEntryDto> Entries { get; set; } = new List<LeaderboardEntryDto>();
        public UserRankDto? CurrentUserRank { get; set; }
    }

    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public required string UserId { get; set; }
        public required string Name { get; set; }
        public string? Title { get; set; } // Added to match frontend component
        public string? Avatar { get; set; }
        public int Points { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class UserRankDto
    {
        public int Rank { get; set; }
        public int Points { get; set; }
    }
}