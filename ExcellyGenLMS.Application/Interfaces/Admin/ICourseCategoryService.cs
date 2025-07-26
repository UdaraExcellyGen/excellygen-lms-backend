using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ICourseCategoryService
    {
        Task<List<CourseCategoryDto>> GetAllCategoriesAsync(bool includeDeleted = false);
        Task<CourseCategoryDto> GetCategoryByIdAsync(string id);
        Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto createCategoryDto, string creatorId); // Corrected signature
        Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto updateCategoryDto);
        Task DeleteCategoryAsync(string id);
        Task<CourseCategoryDto> RestoreCategoryAsync(string id);
        Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id);

        // Added missing methods to match your project's interface
        Task<IEnumerable<CourseCategoryDto>> GetLearnerAccessibleCategoriesAsync(string learnerId);
        Task<bool> HasUserEnrollmentsInCategoryAsync(string categoryId, string learnerId);
    }
}