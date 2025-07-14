using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _leaderboardRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(ILeaderboardRepository leaderboardRepository, IFileService fileService, ILogger<LeaderboardService> logger)
        {
            _leaderboardRepository = leaderboardRepository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<LeaderboardDto> GetLeaderboardAsync(string currentUserId)
        {
            _logger.LogInformation("Fetching leaderboard data.");

            var userPoints = await _leaderboardRepository.GetUserPointsAsync();
            var sortedUserPoints = userPoints.OrderByDescending(p => p.TotalPoints).ToList();

            var topUserIds = sortedUserPoints.Select(up => up.UserId).ToList();
            var usersData = await _leaderboardRepository.GetUsersByIdsAsync(topUserIds);
            var usersDict = usersData.ToDictionary(u => u.Id);

            var leaderboardEntries = new List<LeaderboardEntryDto>();
            int rank = 1;
            foreach (var pointEntry in sortedUserPoints)
            {
                if (usersDict.TryGetValue(pointEntry.UserId, out var user))
                {
                    leaderboardEntries.Add(new LeaderboardEntryDto
                    {
                        Rank = rank++,
                        UserId = user.Id,
                        Name = user.Name,
                        Title = user.JobRole,
                        Points = pointEntry.TotalPoints,
                        Avatar = user.Avatar != null ? _fileService.GetFullImageUrl(user.Avatar) : null,
                        IsCurrentUser = user.Id == currentUserId
                    });
                }
            }

            var currentUserRank = leaderboardEntries.FirstOrDefault(e => e.IsCurrentUser);

            var leaderboardDto = new LeaderboardDto
            {
                Entries = leaderboardEntries,
                CurrentUserRank = currentUserRank != null ? new UserRankDto { Rank = currentUserRank.Rank, Points = currentUserRank.Points } : null
            };

            _logger.LogInformation("Successfully generated leaderboard with {EntryCount} entries.", leaderboardEntries.Count);
            return leaderboardDto;
        }
    }
}