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
        Task<CourseCategory> ToggleCategoryStatusAsync(string id);
        Task<bool> HasActiveCoursesAsync(string categoryId);
        Task<int> GetCoursesCountByCategoryIdAsync(string categoryId);
        Task<int> GetActiveLearnersCountByCategoryIdAsync(string categoryId);
        Task<TimeSpan?> GetAverageCourseDurationByCategoryIdAsync(string categoryId);
    }
}