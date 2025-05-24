//using ExcellyGenLMS.Application.DTOs.Auth;
//using ExcellyGenLMS.Application.DTOs.Course;
//using ExcellyGenLMS.Application.Interfaces.Admin;
//using ExcellyGenLMS.Core.Entities.Course;
//using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;


//namespace ExcellyGenLMS.Application.Services.Admin
//{
//    public class CourseAdminService : ICourseAdminService
//    {
//        private readonly ICourseAdminRepository _courseRepository;

//        public CourseAdminService(ICourseAdminRepository courseRepository)
//        {
//            _courseRepository = courseRepository;
//        }

//        public async Task<List<CourseDto>> GetCoursesByCategoryIdAsync(string categoryId)
//        {
//            var courses = await _courseRepository.GetCoursesByCategoryIdAsync(categoryId);
//            return courses.Select(MapToDto).ToList();
//        }

//        public async Task<CourseDto> UpdateCourseAdminAsync(int id, UpdateCourseAdminDto updateCourseDto)
//        {
//            var course = await _courseRepository.GetCourseByIdAsync(id)
//                ?? throw new KeyNotFoundException($"Course with ID {id} not found");

//            // Update only title and description as admin
//            course.Title = updateCourseDto.Title;
//            course.Description = updateCourseDto.Description;
//            course.LastUpdatedDate = DateTime.UtcNow;

//            var updatedCourse = await _courseRepository.UpdateCourseAsync(course);
//            return MapToDto(updatedCourse);
//        }

//        public async Task DeleteCourseAsync(int id)
//        {
//            await _courseRepository.DeleteCourseAsync(id);
//        }

//        // Helper method to map entity to DTO
//        private static CourseDto MapToDto(Course course)
//        {
//            return new CourseDto
//            {
//                Id = course.Id.ToString(),
//                Title = course.Title,
//                Description = course.Description ?? string.Empty,
//                CreatedAt = course.CreatedAt,
//                CreatedAtFormatted = course.CreatedAt.ToString("yyyy-MM-dd"),
//                Lessons = course.Lessons?.Count ?? 0,
//                Creator = course.Creator != null ? new UserDto
//                {
//                    Id = course.Creator.Id,
//                    Name = course.Creator.Name,
//                    Email = course.Creator.Email,
//                    Phone = course.Creator.Phone,
//                    Roles = course.Creator.Roles,
//                    Department = course.Creator.Department,
//                    Status = course.Creator.Status,
//                    JoinedDate = course.Creator.JoinedDate,
//                    JobRole = course.Creator.JobRole,
//                    About = course.Creator.About,
//                    FirebaseUid = course.Creator.FirebaseUid,
//                    Avatar = course.Creator.Avatar
//                } : null
//            };
//        }
//    }
//}

using ExcellyGenLMS.Application.DTOs.Admin; // <-- Add this line for UpdateCourseAdminDto
// Remove unused DTO using: using ExcellyGenLMS.Application.DTOs.Auth; - No longer mapping to full UserDto here
using ExcellyGenLMS.Application.DTOs.Course; // Needed for CourseDto, UserBasicDto, CategoryDto, etc.
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Auth; // Still needed to access User properties if repo includes it
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin; // Use the specific Admin Repo Interface
// Required standard namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace ExcellyGenLMS.Application.Services.Admin
{
    public class CourseAdminService : ICourseAdminService
    {
        private readonly ICourseAdminRepository _courseAdminRepository; // Renamed variable for clarity
        private readonly ILogger<CourseAdminService> _logger; // Added logger

        // Consider injecting other repos IF needed for more complex admin actions or richer mapping
        public CourseAdminService(ICourseAdminRepository courseAdminRepository, ILogger<CourseAdminService> logger)
        {
            _courseAdminRepository = courseAdminRepository ?? throw new ArgumentNullException(nameof(courseAdminRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<CourseDto>> GetCoursesByCategoryIdAsync(string categoryId)
        {
            _logger.LogInformation("AdminService: Getting courses for category {CategoryId}", categoryId);
            // ** IMPORTANT **: Ensure GetCoursesByCategoryIdAsync in ICourseAdminRepository uses .Include()
            //                 for Category, Creator, Lessons, CourseTechnologies.Technology if MapToDto needs them.
            var courses = await _courseAdminRepository.GetCoursesByCategoryIdAsync(categoryId);
            return courses.Select(course => MapToDto(course)).ToList(); // Use updated MapToDto
        }

        public async Task<CourseDto> UpdateCourseAdminAsync(int id, UpdateCourseAdminDto updateCourseDto)
        {
            _logger.LogInformation("AdminService: Updating course {CourseId}", id);
            // ** IMPORTANT **: Ensure GetCourseByIdAsync in ICourseAdminRepository uses .Include()
            //                 for Category, Creator, Lessons, CourseTechnologies.Technology if MapToDto needs them.
            var course = await _courseAdminRepository.GetCourseByIdAsync(id);

            if (course == null)
            {
                _logger.LogWarning("AdminService: Course {CourseId} not found for update.", id);
                throw new KeyNotFoundException($"Course with ID {id} not found");
            }

            // Update fields allowed by this Admin DTO
            course.Title = updateCourseDto.Title;
            course.Description = updateCourseDto.Description;
            course.LastUpdatedDate = DateTime.UtcNow; // Ensure LastUpdatedDate is updated

            var updatedCourse = await _courseAdminRepository.UpdateCourseAsync(course);
            _logger.LogInformation("AdminService: Course {CourseId} updated.", id);

            // It's often safer to re-fetch the entity after an update to ensure all data
            // (including potential DB triggers or computed values) is current,
            // especially before mapping to a DTO that expects related entities.
            var resultCourse = await _courseAdminRepository.GetCourseByIdAsync(updatedCourse.Id);
            if (resultCourse == null)
            {
                _logger.LogError("AdminService: Failed to re-fetch course {CourseId} after update.", id);
                throw new InvalidOperationException("Failed to retrieve course details after update.");
            }


            return MapToDto(resultCourse); // Use updated MapToDto
        }

        public async Task DeleteCourseAsync(int id)
        {
            _logger.LogInformation("AdminService: Deleting course {CourseId}", id);
            // Optional: Add checks here (e.g., if admin can delete only certain courses)
            var course = await _courseAdminRepository.GetCourseByIdAsync(id); // Check existence before calling repo delete
            if (course == null)
            {
                _logger.LogWarning("AdminService: Course {CourseId} not found for deletion.", id);
                throw new KeyNotFoundException($"Course with ID {id} not found for deletion.");
            }
            // NOTE: This admin delete likely won't handle deleting associated files (thumbnail/documents)
            // That logic currently resides in the main CourseService. Decide if Admins should trigger that too.
            await _courseAdminRepository.DeleteCourseAsync(id);
            _logger.LogInformation("AdminService: Course {CourseId} deleted.", id);
        }

<<<<<<< HEAD
        // --- Mapping Helper - Refactored ---
        // Maps the Core Course Entity to the Application Course DTO.
        // Assumes the input 'course' has related entities (Creator, Category, etc.) loaded via .Include() in the repository.
        private static CourseDto MapToDto(Course course)
=======
        // Helper method to map entity to DTO
        private static CourseDto MapToDto(ExcellyGenLMS.Core.Entities.Course.Course course)
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
        {
            if (course == null) return null!; // Or handle error/return default

            // Map Creator to UserBasicDto (as expected by CourseDto)
            UserBasicDto creatorDto = course.Creator != null
                ? new UserBasicDto { Id = course.Creator.Id, Name = course.Creator.Name }
                : new UserBasicDto { Id = string.Empty, Name = "N/A" }; // Default if Creator not loaded/null

            // Map Category to CategoryDto
            CategoryDto categoryDto = course.Category != null
                ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
                : new CategoryDto { Id = string.Empty, Title = "N/A" }; // Default if Category not loaded/null

            // Map Technologies
            List<ExcellyGenLMS.Application.DTOs.Course.TechnologyDto> techDtos = course.CourseTechnologies?
                .Where(ct => ct.Technology != null) // Filter out potential nulls if include failed partially
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
                       // FileUrl needs generation logic (typically needs IFileStorageService -> not available here)
                       FileUrl = "[URL Generation Required]",
                       LastUpdatedDate = d.LastUpdatedDate,
                       LessonId = d.LessonId
                   }).ToList() ?? new List<CourseDocumentDto>()
               }).ToList() ?? new List<LessonDto>();


            return new CourseDto
            {
                Id = course.Id, // CourseDto.Id is int
                Title = course.Title,
                Description = course.Description ?? string.Empty,
                CalculatedCoursePoints = course.CoursePoints, // Or could recalculate here: lessonDtos.Sum(l => l.LessonPoints),
                EstimatedTime = course.EstimatedTime,
                CreatedAt = course.CreatedAt,
                LastUpdatedDate = course.LastUpdatedDate,
                Status = course.Status,
                // ThumbnailUrl generation needs IFileStorageService, not typically available in this service.
                ThumbnailUrl = "[URL Generation Required]", // Placeholder
                Category = categoryDto,
                Creator = creatorDto,
                Technologies = techDtos,
                Lessons = lessonDtos,
            };
        }
    }
}