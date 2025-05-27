// ExcellyGenLMS.Core/Interfaces/Repositories/Admin/ICourseCategoryRepository.cs
using ExcellyGenLMS.Core.Entities.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // For TimeSpan

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Admin
{
    public interface ICourseCategoryRepository
    {
        Task<List<CourseCategory>> GetAllCategoriesAsync();
        Task<CourseCategory?> GetCategoryByIdAsync(string id);
        Task<CourseCategory> CreateCategoryAsync(CourseCategory category);
        Task<CourseCategory> UpdateCategoryAsync(CourseCategory category);
        Task DeleteCategoryAsync(string id);
        Task<CourseCategory> ToggleCategoryStatusAsync(string id);
        Task<int> GetCoursesCountByCategoryIdAsync(string categoryId);
        Task<int> GetActiveLearnersCountByCategoryIdAsync(string categoryId);
        Task<TimeSpan?> GetAverageCourseDurationByCategoryIdAsync(string categoryId);
    }
}