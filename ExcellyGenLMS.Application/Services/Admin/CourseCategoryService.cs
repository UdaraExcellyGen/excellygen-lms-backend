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

        private CourseCategoryDto MapToDto(CourseCategory category, int? coursesCount = null, int? learnersCount = null, string? avgDuration = null)
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
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.Creator?.Name ?? "System",
                TotalCourses = coursesCount ?? category.Courses?.Count(c => !c.IsInactive) ?? 0,
                ActiveLearnersCount = learnersCount ?? 0,
                AvgDuration = avgDuration ?? "N/A",
                RestoreAt = category.DeletedAt.HasValue ? category.DeletedAt.Value.AddDays(30) : null
            };
        }

        public async Task<List<CourseCategoryDto>> GetAllCategoriesAsync(bool includeDeleted = false)
        {
            var categories = await _categoryRepository.GetAllCategoryDetailsAsync(includeDeleted);

            var categoryDtos = categories.Select(category =>
            {
                var coursesCount = category.Courses?.Count(c => !c.IsInactive) ?? 0;

                var activeLearnersCount = category.Courses?
                    .SelectMany(c => c.Enrollments)
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count() ?? 0;

                var validCourses = category.Courses?.Where(c => !c.IsInactive && c.EstimatedTime > 0).ToList() ?? new List<ExcellyGenLMS.Core.Entities.Course.Course>();
                var avgDuration = "N/A";
                if (validCourses.Any())
                {
                    var avgHours = validCourses.Average(c => c.EstimatedTime);
                    avgDuration = $"{Math.Round(avgHours)} hours";
                }

                return MapToDto(category, coursesCount, activeLearnersCount, avgDuration);
            }).ToList();

            return categoryDtos;
        }

        public async Task<CourseCategoryDto> GetCategoryByIdAsync(string id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found");

            var coursesCount = await _categoryRepository.GetCoursesCountByCategoryIdAsync(id);
            var activeLearnersCount = await _categoryRepository.GetActiveLearnersCountByCategoryIdAsync(id);
            var avgTimeSpan = await _categoryRepository.GetAverageCourseDurationByCategoryIdAsync(id);
            var avgDuration = avgTimeSpan.HasValue ? $"{Math.Round(avgTimeSpan.Value.TotalHours)} hours" : "N/A";

            return MapToDto(category, coursesCount, activeLearnersCount, avgDuration);
        }

        public async Task<CourseCategoryDto> CreateCategoryAsync(CreateCourseCategoryDto dto, string creatorId)
        {
            var category = new CourseCategory
            {
                Title = dto.Title,
                Description = dto.Description,
                Icon = dto.Icon,
                CreatedById = creatorId
            };

            var createdCategory = await _categoryRepository.CreateCategoryAsync(category);
            return await GetCategoryByIdAsync(createdCategory.Id);
        }

        public async Task<CourseCategoryDto> UpdateCategoryAsync(string id, UpdateCourseCategoryDto dto)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found");

            category.Title = dto.Title;
            category.Description = dto.Description;
            category.Icon = dto.Icon;
            category.Status = dto.Status;

            await _categoryRepository.UpdateCategoryAsync(category);
            return await GetCategoryByIdAsync(id);
        }

        public async Task DeleteCategoryAsync(string id)
        {
            if (await _categoryRepository.HasActiveCoursesAsync(id))
            {
                throw new InvalidOperationException("Cannot delete category with active courses.");
            }

            await _categoryRepository.DeleteCategoryAsync(id);
        }

        public async Task<CourseCategoryDto> RestoreCategoryAsync(string id)
        {
            var category = await _categoryRepository.RestoreCategoryAsync(id)
                ?? throw new KeyNotFoundException($"Category with ID {id} not found or cannot be restored");

            return await GetCategoryByIdAsync(category.Id);
        }

        public async Task<CourseCategoryDto> ToggleCategoryStatusAsync(string id)
        {
            await _categoryRepository.ToggleCategoryStatusAsync(id);
            return await GetCategoryByIdAsync(id);
        }

        public async Task<IEnumerable<CourseCategoryDto>> GetLearnerAccessibleCategoriesAsync(string learnerId)
        {
            var allCategories = await GetAllCategoriesAsync();
            return allCategories.Where(c => c.Status == "active" && !c.IsDeleted);
        }

        public async Task<bool> HasUserEnrollmentsInCategoryAsync(string categoryId, string learnerId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category?.Courses == null)
                return false;

            return category.Courses.Any(c => c.Enrollments.Any(e => e.UserId == learnerId));
        }
    }
}