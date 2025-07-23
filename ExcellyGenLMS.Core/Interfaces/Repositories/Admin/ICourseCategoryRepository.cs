using ExcellyGenLMS.Core.Entities.Admin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface ICourseCategoryRepository
    {
        Task<List<CourseCategory>> GetAllCategoriesAsync(bool includeDeleted = false);
        Task<CourseCategory?> GetCategoryByIdAsync(string id);
        Task<CourseCategory> CreateCategoryAsync(CourseCategory category);
        Task<CourseCategory> UpdateCategoryAsync(CourseCategory category);
        Task DeleteCategoryAsync(string id);
        Task<CourseCategory?> RestoreCategoryAsync(string id);
        Task<bool> HasActiveCoursesAsync(string categoryId);
        Task<CourseCategory> ToggleCategoryStatusAsync(string id);
        Task<int> GetCoursesCountByCategoryIdAsync(string categoryId);
        Task<int> GetActiveLearnersCountByCategoryIdAsync(string categoryId);
        Task<TimeSpan?> GetAverageCourseDurationByCategoryIdAsync(string categoryId);

        // NEW METHOD: Check if user has active enrollments in this category
        Task<bool> HasActiveEnrollmentsAsync(string categoryId, string userId);

        // NEW METHOD: Get categories where user has enrollments (for learner filtering)
        Task<List<string>> GetCategoryIdsWithUserEnrollmentsAsync(string userId);
    }
}