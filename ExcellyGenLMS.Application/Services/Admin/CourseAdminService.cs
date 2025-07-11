using ExcellyGenLMS.Application.DTOs.Admin;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class CourseAdminService : ICourseAdminService
    {
        private readonly ICourseAdminRepository _courseAdminRepository;
        private readonly ILogger<CourseAdminService> _logger;

        public CourseAdminService(ICourseAdminRepository courseAdminRepository, ILogger<CourseAdminService> logger)
        {
            _courseAdminRepository = courseAdminRepository ?? throw new ArgumentNullException(nameof(courseAdminRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // OPTIMIZATION: Use repository method for optimized course retrieval
        public async Task<List<CourseDto>> GetCoursesByCategoryIdAsync(string categoryId)
        {
            _logger.LogInformation("AdminService: Getting courses for category {CategoryId}", categoryId);

            try
            {
                // Repository will handle the optimization internally
                var courses = await _courseAdminRepository.GetCoursesByCategoryIdAsync(categoryId);
                return courses.Select(course => MapToDto(course)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses for category {CategoryId}", categoryId);
                throw;
            }
        }

        // OPTIMIZATION: Add method for category statistics
        public async Task<AdminCategoryStatsDto> GetCategoryStatsAsync(string categoryId)
        {
            _logger.LogInformation("AdminService: Getting stats for category {CategoryId}", categoryId);

            try
            {
                // Get courses first
                var courses = await _courseAdminRepository.GetCoursesByCategoryIdAsync(categoryId);

                // Calculate stats (this will be optimized in the repository layer)
                var totalCourses = courses.Count;
                var averageTime = courses.Any() ? courses.Average(c => c.EstimatedTime) : 0;

                // For now, return basic stats - repository optimization will improve this
                return new AdminCategoryStatsDto
                {
                    TotalCourses = totalCourses,
                    TotalLearners = 0, // Will be optimized in repository
                    AverageTime = averageTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category stats for {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<CourseDto> UpdateCourseAdminAsync(int id, UpdateCourseAdminDto updateCourseDto)
        {
            _logger.LogInformation("AdminService: Updating course {CourseId}", id);

            try
            {
                var course = await _courseAdminRepository.GetCourseByIdAsync(id);

                if (course == null)
                {
                    _logger.LogWarning("AdminService: Course {CourseId} not found for update.", id);
                    throw new KeyNotFoundException($"Course with ID {id} not found");
                }

                course.Title = updateCourseDto.Title;
                course.Description = updateCourseDto.Description;
                course.LastUpdatedDate = DateTime.UtcNow;

                var updatedCourse = await _courseAdminRepository.UpdateCourseAsync(course);
                _logger.LogInformation("AdminService: Course {CourseId} updated.", id);

                var resultCourse = await _courseAdminRepository.GetCourseByIdAsync(updatedCourse.Id);
                if (resultCourse == null)
                {
                    _logger.LogError("AdminService: Failed to re-fetch course {CourseId} after update.", id);
                    throw new InvalidOperationException("Failed to retrieve course details after update.");
                }

                return MapToDto(resultCourse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course {CourseId}", id);
                throw;
            }
        }

        public async Task DeleteCourseAsync(int id)
        {
            _logger.LogInformation("AdminService: Deleting course {CourseId}", id);

            try
            {
                var course = await _courseAdminRepository.GetCourseByIdAsync(id);
                if (course == null)
                {
                    _logger.LogWarning("AdminService: Course {CourseId} not found for deletion.", id);
                    throw new KeyNotFoundException($"Course with ID {id} not found for deletion.");
                }

                await _courseAdminRepository.DeleteCourseAsync(id);
                _logger.LogInformation("AdminService: Course {CourseId} deleted.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course {CourseId}", id);
                throw;
            }
        }

        // Optimized mapping method with better error handling
        private static CourseDto MapToDto(ExcellyGenLMS.Core.Entities.Course.Course course)
        {
            if (course == null) return null!;

            try
            {
                // Map Creator to UserBasicDto (as expected by CourseDto)
                UserBasicDto creatorDto = course.Creator != null
                    ? new UserBasicDto { Id = course.Creator.Id, Name = course.Creator.Name }
                    : new UserBasicDto { Id = string.Empty, Name = "N/A" };

                // Map Category to CategoryDto
                CategoryDto categoryDto = course.Category != null
                    ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
                    : new CategoryDto { Id = string.Empty, Title = "N/A" };

                // Map Technologies
                List<ExcellyGenLMS.Application.DTOs.Course.TechnologyDto> techDtos = course.CourseTechnologies?
                    .Where(ct => ct.Technology != null)
                    .Select(ct => new ExcellyGenLMS.Application.DTOs.Course.TechnologyDto { Id = ct.Technology!.Id, Name = ct.Technology!.Name })
                    .ToList() ?? new List<ExcellyGenLMS.Application.DTOs.Course.TechnologyDto>();

                // Map Lessons (and potentially nested documents if needed and included)
                List<LessonDto> lessonDtos = course.Lessons?
                   .Select(l => new LessonDto
                   {
                       Id = l.Id,
                       LessonName = l.LessonName,
                       LessonPoints = l.LessonPoints,
                       LastUpdatedDate = l.LastUpdatedDate,
                       CourseId = l.CourseId,
                       Documents = l.Documents?.Select(d => new CourseDocumentDto
                       {
                           Id = d.Id,
                           Name = d.Name,
                           DocumentType = d.DocumentType,
                           FileSize = d.FileSize,
                           FileUrl = "[URL Generation Required]",
                           LastUpdatedDate = d.LastUpdatedDate,
                           LessonId = d.LessonId
                       }).ToList() ?? new List<CourseDocumentDto>()
                   }).ToList() ?? new List<LessonDto>();

                return new CourseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description ?? string.Empty,
                    CalculatedCoursePoints = course.CoursePoints,
                    EstimatedTime = course.EstimatedTime,
                    CreatedAt = course.CreatedAt,
                    LastUpdatedDate = course.LastUpdatedDate,
                    Status = course.Status,
                    ThumbnailUrl = "[URL Generation Required]",
                    Category = categoryDto,
                    Creator = creatorDto,
                    Technologies = techDtos,
                    Lessons = lessonDtos,
                };
            }
            catch (Exception ex)
            {
                // Log the error but don't throw to prevent breaking the entire operation
                Console.WriteLine($"Error mapping course {course.Id}: {ex.Message}");

                // Return a basic DTO with essential information
                return new CourseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description ?? string.Empty,
                    CalculatedCoursePoints = course.CoursePoints,
                    EstimatedTime = course.EstimatedTime,
                    CreatedAt = course.CreatedAt,
                    LastUpdatedDate = course.LastUpdatedDate,
                    Status = course.Status,
                    ThumbnailUrl = "[URL Generation Required]",
                    Category = new CategoryDto { Id = string.Empty, Title = "N/A" },
                    Creator = new UserBasicDto { Id = string.Empty, Name = "N/A" },
                    Technologies = new List<ExcellyGenLMS.Application.DTOs.Course.TechnologyDto>(),
                    Lessons = new List<LessonDto>(),
                };
            }
        }
    }

    // OPTIMIZATION: Move CategoryStatsDto to Admin DTOs namespace to avoid conflicts
    public class AdminCategoryStatsDto
    {
        public int TotalCourses { get; set; }
        public int TotalLearners { get; set; }
        public double AverageTime { get; set; }
    }
}