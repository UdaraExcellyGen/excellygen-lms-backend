using ExcellyGenLMS.Application.DTOs;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class LearnerStatsService : ILearnerStatsService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly IUserActivityLogRepository _activityLogRepository;

        public LearnerStatsService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository,
            ICourseCategoryRepository categoryRepository,
            IUserActivityLogRepository activityLogRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _activityLogRepository = activityLogRepository;
        }

        public async Task<OverallLmsStatsDto> GetOverallLmsStatsAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            int totalCoursesCount = 0;
            foreach (var category in categories)
            {
                totalCoursesCount += await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
            }
            var totalActiveLearners = await _enrollmentRepository.GetTotalUniqueActiveLearnersCountAsync();
            var allUsers = await _userRepository.GetAllUsersAsync();
            var totalActiveCoordinators = allUsers.Count(u => u.Status == "active" && u.Roles.Contains("CourseCoordinator"));
            var totalProjectManagers = allUsers.Count(u => u.Status == "active" && u.Roles.Contains("ProjectManager"));
            var totalCategories = categories.Count(c => c.Status == "active");
            var avgDuration = await _courseRepository.GetOverallAverageCourseDurationAsync();
            var avgDurationString = avgDuration.HasValue ? $"{Math.Round(avgDuration.Value.TotalHours)} hours" : "N/A";

            return new OverallLmsStatsDto
            {
                TotalCategories = totalCategories,
                TotalPublishedCourses = totalCoursesCount,
                TotalActiveLearners = totalActiveLearners,
                TotalActiveCoordinators = totalActiveCoordinators,
                TotalProjectManagers = totalProjectManagers,
                AverageCourseDurationOverall = avgDurationString
            };
        }

        public async Task<IEnumerable<DailyScreenTimeDto>> GetWeeklyScreenTimeAsync(string userId)
        {
            TimeZoneInfo sriLankaZone;
            try
            {
                // For Windows servers
                sriLankaZone = TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // For Linux/macOS servers
                sriLankaZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
            }

            // This is the critical fix: We use the *actual* current date in Sri Lanka.
            // The hardcoded test date has been removed.
            DateTime todayInSriLanka = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, sriLankaZone).Date;

            var dayOfWeek = todayInSriLanka.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)todayInSriLanka.DayOfWeek;
            var startOfWeek = todayInSriLanka.AddDays(-(dayOfWeek - 1));

            var activityLogs = await _activityLogRepository.GetRecentActivityForUserAsync(userId, startOfWeek);

            var dailyMinutes = activityLogs
                .GroupBy(log => TimeZoneInfo.ConvertTimeFromUtc(log.ActivityTimestamp, sriLankaZone).Date)
                .ToDictionary(group => group.Key, group => group.Count());

            var result = new List<DailyScreenTimeDto>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startOfWeek.AddDays(i);
                var dto = new DailyScreenTimeDto
                {
                    Day = currentDate.ToString("ddd", CultureInfo.InvariantCulture),
                    FullDate = currentDate.ToString("ddd, MMM d", CultureInfo.InvariantCulture),
                    IsToday = (currentDate == todayInSriLanka)
                };

                if (currentDate <= todayInSriLanka)
                {
                    dailyMinutes.TryGetValue(currentDate, out int minutes);
                    dto.TotalMinutes = minutes;
                }
                else
                {
                    dto.TotalMinutes = null;
                }
                result.Add(dto);
            }
            return result;
        }
    }
}