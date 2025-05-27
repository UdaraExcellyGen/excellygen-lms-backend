using ExcellyGenLMS.Application.DTOs.Admin;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface ICourseCategoryService
    {
        Task<List<CourseCategoryDto>> GetAllCategoriesAsync();
        Task<CourseCategoryDto> GetCategoryByIdAsync(string id);
        Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto createCategoryDto);
        Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto updateCategoryDto);
        Task DeleteCategoryAsync(string id);
        Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id);
    }
}