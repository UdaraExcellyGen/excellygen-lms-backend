// ExcellyGenLMS.Application/Services/Learner/LearnerStatsService.cs
using ExcellyGenLMS.Application.DTOs;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class LearnerStatsService : ILearnerStatsService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseCategoryRepository _categoryRepository;

        public LearnerStatsService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository,
            ICourseCategoryRepository categoryRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<OverallLmsStatsDto> GetOverallLmsStatsAsync()
        {
            // Get all categories
            var categories = await _categoryRepository.GetAllCategoriesAsync();

            // Calculate total courses across all categories
            int totalCoursesCount = 0;
            foreach (var category in categories)
            {
                // Use existing method to get course count by category
                totalCoursesCount += await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
            }

            // Get total active learners
            var totalActiveLearners = await _enrollmentRepository.GetTotalUniqueActiveLearnersCountAsync();

            // Get all users for counting roles
            var allUsers = await _userRepository.GetAllUsersAsync();

            // Get total active coordinators
            var totalActiveCoordinators = allUsers
                .Count(u => u.Status == "active" && u.Roles.Contains("CourseCoordinator"));

            // Get total project managers
            var totalProjectManagers = allUsers
                .Count(u => u.Status == "active" && u.Roles.Contains("ProjectManager"));

            // Get total active categories
            var totalCategories = categories.Count(c => c.Status == "active");

            // Get average course duration
            var avgDuration = await _courseRepository.GetOverallAverageCourseDurationAsync();
            var avgDurationString = avgDuration.HasValue
                ? $"{Math.Round(avgDuration.Value.TotalHours)} hours"
                : "N/A";

            return new OverallLmsStatsDto
            {
                TotalCategories = totalCategories,
                TotalPublishedCourses = totalCoursesCount, // Use the calculated sum of all courses
                TotalActiveLearners = totalActiveLearners,
                TotalActiveCoordinators = totalActiveCoordinators,
                TotalProjectManagers = totalProjectManagers,
                AverageCourseDurationOverall = avgDurationString
            };
        }
    }
}