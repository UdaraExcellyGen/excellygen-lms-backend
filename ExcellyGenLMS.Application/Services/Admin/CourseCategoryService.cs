using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class CourseCategoryService : ICourseCategoryService
    {
        private readonly ICourseCategoryRepository _categoryRepository;

        public CourseCategoryService(ICourseCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        private CourseCategoryDto MapToDto(CourseCategory category, int coursesCount, int learnersCount, string avgDuration)
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
                RestoreAt = category.DeletedAt.HasValue ? category.DeletedAt.Value.AddDays(30) : null
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

        public async Task<CourseCategoryDto> GetCategoryByIdAsync(string id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id) ?? throw new KeyNotFoundException($"Category with ID {id} not found");
            var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(id);
            var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(id);
            var avgTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(id);
            var avgDuration = avgTimeSpan.HasValue ? $"{Math.Round(avgTimeSpan.Value.TotalHours)} hours" : "N/A";
            return MapToDto(category, coursesCount, activeLearnersCount, avgDuration);
        }

        public async Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto dto, string creatorId)
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
            if (await _categoryRepository.HasActiveCoursesAsync(id)) throw new InvalidOperationException("Cannot delete category with active courses.");
            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public async Task<CourseCategoryDto> RestoreCategoryAsync(string id)
        {
            var cat = await _categoryRepository.RestoreCategoryAsync(id) ?? throw new KeyNotFoundException();
            return await GetCategoryByIdAsync(cat.Id);
        }

        public async Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id)
        {
            await _categoryRepository.ToggleCategoryStatusAsync(id);
            return await GetCategoryByIdAsync(id);
        }

        // ADDED: Implemented missing method
        public async Task<IEnumerable<CourseCategoryDto>> GetLearnerAccessibleCategoriesAsync(string learnerId)
        {
            var allCategories = await GetAllCategoriesAsync();
            return allCategories.Where(c => c.Status == "active" && !c.IsDeleted);
        }

        // ADDED: Implemented missing method
        public async Task<bool> HasUserEnrollmentsInCategoryAsync(string categoryId, string learnerId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null || category.Courses == null) return false;

            return category.Courses.Any(c => c.Enrollments.Any(e => e.UserId == learnerId));
        }
    }
}