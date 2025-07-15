using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Enums; // Ensure this using is present

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int Id { get; set; } // Keep int as it's likely auto-incrementing PK

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        // CoursePoints will be calculated from Lessons, nullable allows calculation before publish
        public int? CoursePoints { get; set; }

        [Required] // Estimated time is required in step 1
        public int EstimatedTime { get; set; } // In Hours

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [Required]
        public bool IsInactive { get; set; } = false;

        [MaxLength(1024)] // Store URL/path, Increased length
        public string? ThumbnailImagePath { get; set; } // Renamed

        // Foreign key for the category
        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [ForeignKey("CategoryId")]
        public virtual CourseCategory Category { get; set; } = null!;

        // Foreign key for the creator (assuming User Id is string)
        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; } = null!;

        // Navigation property for Lessons
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        // Navigation property for Technologies (Many-to-Many via join table)
        public virtual ICollection<CourseTechnology> CourseTechnologies { get; set; } = new List<CourseTechnology>();
    }
}