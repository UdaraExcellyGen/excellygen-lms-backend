using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    // Helper class to hold point calculation results
    public class UserPoints
    {
        public required string UserId { get; set; }
        public int TotalPoints { get; set; }
    }

    // Helper class to hold user details for the leaderboard
    public class UserLeaderboardInfo
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Avatar { get; set; }
        public required string JobRole { get; set; }
    }

    public interface ILeaderboardRepository
    {
        Task<IEnumerable<UserPoints>> GetUserPointsAsync();
        Task<IEnumerable<UserLeaderboardInfo>> GetUsersByIdsAsync(IEnumerable<string> userIds);
    }
}