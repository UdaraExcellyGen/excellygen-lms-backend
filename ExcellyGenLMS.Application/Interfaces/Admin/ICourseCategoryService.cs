using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ICourseCategoryService
    {
        Task<List<CourseCategoryDto>> GetAllCategoriesAsync(bool includeDeleted = false);
        Task<CourseCategoryDto> GetCategoryByIdAsync(string id);
        // Add creatorId parameter to the method signature
        Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto createCategoryDto, string creatorId);
        Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto updateCategoryDto);
        Task DeleteCategoryAsync(string id);
        Task<CourseCategoryDto> RestoreCategoryAsync(string id);
        Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id);
        Task<List<CourseCategoryDto>> GetLearnerAccessibleCategoriesAsync(string userId);
        Task<bool> HasUserEnrollmentsInCategoryAsync(string userId, string categoryId);
    }
}