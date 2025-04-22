using ExcellyGenLMS.Application.DTOs.Auth;
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Admin;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;

namespace ExcellyGenLMS.Application.Services.Admin
{
    public class CourseAdminService : ICourseAdminService
    {
        private readonly ICourseAdminRepository _courseRepository;

        public CourseAdminService(ICourseAdminRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<List<CourseDto>> GetCoursesByCategoryIdAsync(string categoryId)
        {
            var courses = await _courseRepository.GetCoursesByCategoryIdAsync(categoryId);
            return courses.Select(MapToDto).ToList();
        }

        public async Task<CourseDto> UpdateCourseAdminAsync(int id, UpdateCourseAdminDto updateCourseDto)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id)
                ?? throw new KeyNotFoundException($"Course with ID {id} not found");

            // Update only title and description as admin
            course.Title = updateCourseDto.Title;
            course.Description = updateCourseDto.Description;
            course.LastUpdatedDate = DateTime.UtcNow;

            var updatedCourse = await _courseRepository.UpdateCourseAsync(course);
            return MapToDto(updatedCourse);
        }

        public async Task DeleteCourseAsync(int id)
        {
            await _courseRepository.DeleteCourseAsync(id);
        }

        // Helper method to map entity to DTO
        private static CourseDto MapToDto(Course course)
        {
            return new CourseDto
            {
                Id = course.Id.ToString(),
                Title = course.Title,
                Description = course.Description ?? string.Empty,
                CreatedAt = course.CreatedAt,
                CreatedAtFormatted = course.CreatedAt.ToString("yyyy-MM-dd"),
                Lessons = course.Lessons?.Count ?? 0,
                Creator = course.Creator != null ? new UserDto
                {
                    Id = course.Creator.Id,
                    Name = course.Creator.Name,
                    Email = course.Creator.Email,
                    Phone = course.Creator.Phone,
                    Roles = course.Creator.Roles,
                    Department = course.Creator.Department,
                    Status = course.Creator.Status,
                    JoinedDate = course.Creator.JoinedDate,
                    JobRole = course.Creator.JobRole,
                    About = course.Creator.About,
                    FirebaseUid = course.Creator.FirebaseUid,
                    Avatar = course.Creator.Avatar
                } : null
            };
        }
    }
}