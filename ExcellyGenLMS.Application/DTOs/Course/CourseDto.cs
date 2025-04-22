using System.ComponentModel.DataAnnotations;
using ExcellyGenLMS.Application.DTOs.Auth;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    public class CourseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedAtFormatted { get; set; } = string.Empty;
        public int Lessons { get; set; }
        public UserDto? Creator { get; set; }
    }

    public class UpdateCourseAdminDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}