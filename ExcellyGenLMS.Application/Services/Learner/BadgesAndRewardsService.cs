// Path: ExcellyGenLMS.Application/Services/Learner/BadgesAndRewardsService.cs

using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class BadgesAndRewardsService : IBadgesAndRewardsService
    {
        private readonly IBadgeRepository _badgeRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IForumThreadRepository _forumThreadRepository;
        private readonly IThreadCommentRepository _threadCommentRepository;
        private readonly IThreadComReplyRepository _threadComReplyRepository;
        private readonly ILeaderboardRepository _leaderboardRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILearnerNotificationService _notificationService; // Already injected
        private readonly ILogger<BadgesAndRewardsService> _logger;

        public BadgesAndRewardsService(
            IBadgeRepository badgeRepository,
            IQuizAttemptRepository quizAttemptRepository,
            IEnrollmentRepository enrollmentRepository,
            IForumThreadRepository forumThreadRepository,
            IThreadCommentRepository threadCommentRepository,
            IThreadComReplyRepository threadComReplyRepository,
            ILeaderboardRepository leaderboardRepository,
            ICourseRepository courseRepository,
            ILearnerNotificationService notificationService,
            ILogger<BadgesAndRewardsService> logger)
        {
            _badgeRepository = badgeRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _enrollmentRepository = enrollmentRepository;
            _forumThreadRepository = forumThreadRepository;
            _threadCommentRepository = threadCommentRepository;
            _threadComReplyRepository = threadComReplyRepository;
            _leaderboardRepository = leaderboardRepository;
            _courseRepository = courseRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IEnumerable<BadgeDto>> GetBadgesAndRewardsAsync(string userId)
        {
            var allBadges = await _badgeRepository.GetAllBadgesAsync();
            var userBadges = (await _badgeRepository.GetUserBadgesByUserIdAsync(userId)).ToDictionary(ub => ub.BadgeId);

            var result = new List<BadgeDto>();

            foreach (var badge in allBadges)
            {
                userBadges.TryGetValue(badge.Id, out var userBadge);

                (int currentProgress, bool isUnlocked) = await CalculateProgress(userId, badge);

                if (isUnlocked && userBadge == null)
                {
                    var newUserBadge = new UserBadge { UserId = userId, BadgeId = badge.Id, IsClaimed = false, DateEarned = null };
                    await _badgeRepository.AddUserBadgeAsync(newUserBadge);
                    userBadges[badge.Id] = newUserBadge;

                    
                    // TO TRIGGER THE NOTIFICATION
                    
                    await _notificationService.CreateBadgeUnlockedNotificationAsync(userId, badge.Title);
                    
                }

                result.Add(new BadgeDto
                {
                    Id = badge.Id,
                    Title = badge.Title,
                    Description = badge.Description,
                    HowToEarn = badge.HowToEarn,
                    IconPath = badge.IconPath,
                    CurrentProgress = currentProgress,
                    TargetProgress = badge.TargetProgress,
                    IsUnlocked = isUnlocked,
                    IsClaimed = userBadge?.IsClaimed ?? false,
                    DateEarned = userBadge?.DateEarned?.ToString("o"),
                    Category = badge.Category,
                    Color = badge.Color
                });
            }

            return result;
        }

        
        public async Task<BadgeDto> ClaimBadgeAsync(string userId, string badgeId)
        {
            var badge = (await _badgeRepository.GetAllBadgesAsync()).FirstOrDefault(b => b.Id == badgeId)
                ?? throw new KeyNotFoundException("Badge not found.");

            (int currentProgress, bool isUnlocked) = await CalculateProgress(userId, badge);

            if (!isUnlocked)
            {
                throw new InvalidOperationException("You have not unlocked this badge yet.");
            }

            var userBadge = await _badgeRepository.GetUserBadgeAsync(userId, badgeId);
            if (userBadge == null)
            {
                userBadge = new UserBadge { UserId = userId, BadgeId = badgeId, IsClaimed = true, DateEarned = DateTime.UtcNow };
                await _badgeRepository.AddUserBadgeAsync(userBadge);
            }
            else if (!userBadge.IsClaimed)
            {
                userBadge.IsClaimed = true;
                userBadge.DateEarned = DateTime.UtcNow;
                await _badgeRepository.UpdateUserBadgeAsync(userBadge);
            }

            return new BadgeDto
            {
                Id = badge.Id,
                Title = badge.Title,
                Description = badge.Description,
                HowToEarn = badge.HowToEarn,
                IconPath = badge.IconPath,
                CurrentProgress = currentProgress,
                TargetProgress = badge.TargetProgress,
                IsUnlocked = true,
                IsClaimed = true,
                DateEarned = userBadge.DateEarned?.ToString("o"),
                Category = badge.Category,
                Color = badge.Color
            };
        }

        private async Task<(int, bool)> CalculateProgress(string userId, Badge badge)
        {
            int progress = 0;
            switch (badge.Id)
            {
                
                case "perfectionist":
                    var attempts = (await _quizAttemptRepository.GetAttemptsByUserIdAsync(userId))
                                   .Where(a => a.IsCompleted && a.CompletionTime.HasValue)
                                   .OrderBy(a => a.CompletionTime).ToList();
                    int consecutivePerfectScores = 0;
                    foreach (var attempt in attempts)
                    {
                        if (attempt.TotalQuestions > 0 && attempt.CorrectAnswers == attempt.TotalQuestions)
                        {
                            consecutivePerfectScores++;
                            if (consecutivePerfectScores >= badge.TargetProgress) break;
                        }
                        else
                        {
                            consecutivePerfectScores = 0;
                        }
                    }
                    progress = consecutivePerfectScores;
                    break;
                case "helping-hand":
                    var comments = await _threadCommentRepository.GetAllAsync(c => c.CommentorId == userId);
                    var replies = await _threadComReplyRepository.GetAllAsync(r => r.CommentorId == userId);
                    progress = comments.Count() + replies.Count();
                    break;
                case "top-performer":
                    var leaderboard = await _leaderboardRepository.GetUserPointsAsync();
                    var userRank = leaderboard.OrderByDescending(u => u.TotalPoints)
                                              .ToList()
                                              .FindIndex(u => u.UserId == userId) + 1;
                    if (userRank == 1 && leaderboard.Any()) progress = 1;
                    break;
                case "explorer":
                    var completedEnrollments = (await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId))
                                              .Where(e => e.Status == "completed" && e.Course != null && e.Course.Category != null);
                    progress = completedEnrollments.Select(e => e.Course!.CategoryId).Distinct().Count();
                    break;
                case "fast-learner":
                    var fastEnrollments = (await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId))
                                            .Where(e => e.Status == "completed" && e.Course != null && e.CompletionDate.HasValue)
                                            .ToList();
                    int fastCoursesCount = 0;
                    foreach (var en in fastEnrollments)
                    {
                        if (en.Course!.EstimatedTime > 0)
                        {
                            var timeTaken = en.CompletionDate!.Value - en.EnrollmentDate;
                            var estimatedTime = TimeSpan.FromHours(en.Course!.EstimatedTime);
                            if (timeTaken.TotalHours > 0 && timeTaken.TotalHours < (estimatedTime.TotalHours * 0.5))
                            {
                                fastCoursesCount++;
                            }
                        }
                    }
                    progress = fastCoursesCount;
                    break;
                case "daily-learner":
                    var completionDates = (await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId))
                                          .Where(e => e.CompletionDate.HasValue)
                                          .Select(e => e.CompletionDate!.Value.Date)
                                          .Distinct()
                                          .OrderBy(d => d)
                                          .ToList();
                    int consecutiveDays = 0;
                    if (completionDates.Any())
                    {
                        consecutiveDays = 1;
                        for (int i = 1; i < completionDates.Count; i++)
                        {
                            if (completionDates[i] == completionDates[i - 1].AddDays(1))
                            {
                                consecutiveDays++;
                            }
                            else if (completionDates[i] != completionDates[i - 1])
                            {
                                consecutiveDays = 1;
                            }
                            if (consecutiveDays >= badge.TargetProgress) break;
                        }
                    }
                    progress = consecutiveDays;
                    break;
                default:
                    progress = 0;
                    break;
            }
            return (Math.Min(progress, badge.TargetProgress), progress >= badge.TargetProgress);
        }
    }
}