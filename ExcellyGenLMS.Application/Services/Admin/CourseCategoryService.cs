using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class CourseCategoryService : ICourseCategoryService
    {
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly ILogger<CourseCategoryService> _logger;

        public CourseCategoryService(ICourseCategoryRepository categoryRepository, ILogger<CourseCategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        private CourseCategoryDto MapToDto(CourseCategory category, int coursesCount, int learnersCount, string avgDuration, bool hasUserEnrollments = false, string accessReason = "active")
        {
            return new CourseCategoryDto
            {
                Id = category.Id,
                Title = category.Title,
                Description = category.Description,
                Icon = category.Icon,
                Status = category.Status,
                IsDeleted = category.IsDeleted,
                DeletedAt = category.DeletedAt,
                TotalCourses = coursesCount,
                ActiveLearnersCount = learnersCount,
                AvgDuration = avgDuration,
                RestoreAt = category.DeletedAt.HasValue ? category.DeletedAt.Value.AddDays(30) : null,
                HasUserEnrollments = hasUserEnrollments,
                AccessReason = accessReason,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = "Admin" // Could be enhanced to show actual creator if you track this
            };
        }

        public async Task<List<CourseCategoryDto>> GetAllCategoriesAsync(bool includeDeleted = false)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync(includeDeleted);
            var categoryDtos = new List<CourseCategoryDto>();

            foreach (var category in categories)
            {
                var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
                var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(category.Id);
                var avgTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(category.Id);
                var avgDuration = avgTimeSpan.HasValue ? $"{Math.Round(avgTimeSpan.Value.TotalHours)} hours" : "N/A";

                categoryDtos.Add(MapToDto(category, coursesCount, activeLearnersCount, avgDuration));
            }
            return categoryDtos;
        }

        // NEW METHOD: Get categories accessible to learners (includes inactive categories with enrollments)
        public async Task<List<CourseCategoryDto>> GetLearnerAccessibleCategoriesAsync(string userId)
        {
            // Get all non-deleted categories
            var allCategories = await _categoryRepository.GetAllCategoriesAsync(includeDeleted: false);

            // Get category IDs where the user has enrollments
            var userEnrolledCategoryIds = await _categoryRepository.GetCategoryIdsWithUserEnrollmentsAsync(userId);

            var accessibleCategories = new List<CourseCategoryDto>();

            foreach (var category in allCategories)
            {
                var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
                var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(category.Id);
                var avgTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(category.Id);
                var avgDuration = avgTimeSpan.HasValue ? $"{Math.Round(avgTimeSpan.Value.TotalHours)} hours" : "N/A";

                var hasUserEnrollments = userEnrolledCategoryIds.Contains(category.Id);
                var isActive = category.Status.Equals("active", StringComparison.OrdinalIgnoreCase);

                // Include category if:
                // 1. It's active and has courses, OR
                // 2. User has enrollments in this category (even if inactive)
                bool shouldInclude = (isActive && coursesCount > 0) || hasUserEnrollments;

                if (shouldInclude)
                {
                    var accessReason = isActive ? "active" : "enrolled";
                    accessibleCategories.Add(MapToDto(category, coursesCount, activeLearnersCount, avgDuration, hasUserEnrollments, accessReason));
                }
            }

            return accessibleCategories.OrderByDescending(c => c.Status == "active" ? 1 : 0) // Active categories first
                                      .ThenByDescending(c => c.TotalCourses)
                                      .ToList();
        }

        public async Task<CourseCategoryDto> GetCategoryByIdAsync(string id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found");
            var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(id);
            var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(id);
            var avgTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(id);
            var avgDuration = avgTimeSpan.HasValue ? $"{Math.Round(avgTimeSpan.Value.TotalHours)} hours" : "N/A";
            return MapToDto(category, coursesCount, activeLearnersCount, avgDuration);
        }

        public async Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto dto)
        {
            var cat = new CourseCategory { Title = dto.Title, Description = dto.Description, Icon = dto.Icon };
            await _categoryRepository.CreateCategoryAsync(cat);
            return MapToDto(cat, 0, 0, "N/A");
        }

        public async Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto dto)
        {
            var cat = await _categoryRepository.GetCategoryByIdAsync(id) ?? throw new KeyNotFoundException();
            cat.Title = dto.Title;
            cat.Description = dto.Description;
            cat.Icon = dto.Icon;
            cat.Status = dto.Status;
            await _categoryRepository.UpdateCategoryAsync(cat);
            return await GetCategoryByIdAsync(id);
        }

        public async Task DeleteCategoryAsync(string id)
        {
            // TEST CASE A-05: Check for active courses/enrollments before deletion
            if (await _categoryRepository.HasActiveCoursesAsync(id))
            {
                throw new InvalidOperationException("This category still contains active courses or learner enrollments. Please move or complete all courses before deletion.");
            }
            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public async Task<CourseCategoryDto> RestoreCategoryAsync(string id)
        {
            var cat = await _categoryRepository.RestoreCategoryAsync(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found in trash");
            return await GetCategoryByIdAsync(cat.Id);
        }

        public async Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id)
        {
            await _categoryRepository.ToggleCategoryStatusAsync(id);
            return await GetCategoryByIdAsync(id);
        }

        // NEW METHOD: Check if user has enrollments in a specific category
        public async Task<bool> HasUserEnrollmentsInCategoryAsync(string userId, string categoryId)
        {
            return await _categoryRepository.HasActiveEnrollmentsAsync(categoryId, userId);
        }
    }
}