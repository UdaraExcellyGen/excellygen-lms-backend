using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Enums;
using System.Collections.Generic;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public int? CoursePoints { get; set; }

        [Required]
        public int EstimatedTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [Required]
        public bool IsInactive { get; set; } = false;

        [MaxLength(1024)]
        public string? ThumbnailImagePath { get; set; }

        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [ForeignKey("CategoryId")]
        public virtual CourseCategory Category { get; set; } = null!;

        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public virtual ICollection<CourseTechnology> CourseTechnologies { get; set; } = new List<CourseTechnology>();

        // THIS LINE IS THE MAIN FIX FOR THE BUILD ERRORS
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}