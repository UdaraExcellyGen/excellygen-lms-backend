using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ICourseCategoryService
    {
        Task<List<CourseCategoryDto>> GetAllCategoriesAsync(bool includeDeleted = false);
        Task<CourseCategoryDto> GetCategoryByIdAsync(string id);
        Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto createCategoryDto);
        Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto updateCategoryDto);
        Task DeleteCategoryAsync(string id); // This will now perform a soft delete
        Task<CourseCategoryDto> RestoreCategoryAsync(string id); // ADDED: To restore a category
        Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id);

        // NEW METHOD: Get categories accessible to learners (includes inactive with enrollments)
        Task<List<CourseCategoryDto>> GetLearnerAccessibleCategoriesAsync(string userId);

        // NEW METHOD: Check if user has enrollments in a specific category
        Task<bool> HasUserEnrollmentsInCategoryAsync(string userId, string categoryId);
    }
}