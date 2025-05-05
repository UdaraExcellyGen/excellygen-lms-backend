using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Common;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class UserBadgeService : IUserBadgeService
    {
        private readonly IUserBadgeRepository _userBadgeRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<UserBadgeService> _logger;

        public UserBadgeService(
            IUserBadgeRepository userBadgeRepository,
            IFileService fileService,
            ILogger<UserBadgeService> logger)
        {
            _userBadgeRepository = userBadgeRepository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<List<BadgeDto>> GetUserBadgesAsync(string userId)
        {
            var userBadges = await _userBadgeRepository.GetUserBadgesAsync(userId);

            return userBadges.Select(ub => new BadgeDto
            {
                Id = ub.Badge.Id,
                Name = ub.Badge.Name,
                Description = ub.Badge.Description ?? string.Empty,
                Icon = ub.Badge.Icon ?? string.Empty,
                Color = ub.Badge.Color ?? "#BF4BF6",
                ImageUrl = _fileService.GetFullImageUrl(ub.Badge.ImagePath),
                EarnedDate = ub.EarnedDate
            }).ToList();
        }

        public async Task<UserBadgeSummaryDto> GetUserBadgeSummaryAsync(string userId)
        {
            var totalBadges = await _userBadgeRepository.GetUserBadgeCountAsync(userId);
            var badgesThisMonth = await _userBadgeRepository.GetUserBadgeCountThisMonthAsync(userId);
            var recentBadges = await _userBadgeRepository.GetUserRecentBadgesAsync(userId, 3);

            var recentBadgeDtos = recentBadges.Select(ub => new BadgeDto
            {
                Id = ub.Badge.Id,
                Name = ub.Badge.Name,
                Description = ub.Badge.Description ?? string.Empty,
                Icon = ub.Badge.Icon ?? string.Empty,
                Color = ub.Badge.Color ?? "#BF4BF6",
                ImageUrl = _fileService.GetFullImageUrl(ub.Badge.ImagePath),
                EarnedDate = ub.EarnedDate
            }).ToList();

            return new UserBadgeSummaryDto
            {
                TotalBadges = totalBadges,
                ThisMonth = badgesThisMonth,
                RecentBadges = recentBadgeDtos
            };
        }
    }
}