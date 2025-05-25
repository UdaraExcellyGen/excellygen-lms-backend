// ExcellyGenLMS.Application/Services/Admin/CourseCategoryService.cs
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

        public async Task<List<CourseCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var categoryDtos = new List<CourseCategoryDto>();

            foreach (var category in categories)
            {
                var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
                var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(category.Id);
                var avgDurationTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(category.Id);

                string avgDurationString = avgDurationTimeSpan.HasValue
                    ? $"{Math.Round(avgDurationTimeSpan.Value.TotalHours)} hours"
                    : "N/A";

                categoryDtos.Add(MapToDto(category, coursesCount, activeLearnersCount, avgDurationString));
            }

            return categoryDtos;
        }

        public async Task<CourseCategoryDto> GetCategoryByIdAsync(string id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found");

            var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
            var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(category.Id);
            var avgDurationTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(category.Id);

            string avgDurationString = avgDurationTimeSpan.HasValue
                ? $"{Math.Round(avgDurationTimeSpan.Value.TotalHours)} hours"
                : "N/A";

            return MapToDto(category, coursesCount, activeLearnersCount, avgDurationString);
        }

        public async Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto createCategoryDto)
        {
            var category = new CourseCategory
            {
                Id = Guid.NewGuid().ToString(),
                Title = createCategoryDto.Title,
                Description = createCategoryDto.Description,
                Icon = createCategoryDto.Icon,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
            return MapToDto(createdCategory, 0, 0, "N/A");
        }

        public async Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto updateCategoryDto)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found");

            category.Title = updateCategoryDto.Title;
            category.Description = updateCategoryDto.Description;
            category.Icon = updateCategoryDto.Icon;
            category.Status = updateCategoryDto.Status;

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(category);
            var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
            var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(category.Id);
            var avgDurationTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(category.Id);

            string avgDurationString = avgDurationTimeSpan.HasValue
                ? $"{Math.Round(avgDurationTimeSpan.Value.TotalHours)} hours"
                : "N/A";

            return MapToDto(updatedCategory, coursesCount, activeLearnersCount, avgDurationString);
        }

        public async Task DeleteCategoryAsync(string id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public async Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id)
        {
            var category = await _categoryRepository.ToggleCategoryStatusAsync(id);
            var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(category.Id);
            var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(category.Id);
            var avgDurationTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(category.Id);

            string avgDurationString = avgDurationTimeSpan.HasValue
                ? $"{Math.Round(avgDurationTimeSpan.Value.TotalHours)} hours"
                : "N/A";

            return MapToDto(category, coursesCount, activeLearnersCount, avgDurationString);
        }

        private static CourseCategoryDto MapToDto(CourseCategory category, int coursesCount, int activeLearnersCount, string avgDuration)
        {
            return new CourseCategoryDto
            {
                Id = category.Id,
                Title = category.Title,
                Description = category.Description,
                Icon = category.Icon,
                Status = category.Status,
                TotalCourses = coursesCount,
                ActiveLearnersCount = activeLearnersCount,
                AvgDuration = avgDuration
            };
        }
    }
}