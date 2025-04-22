using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Auth;

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

        [Required]
        public int CoursePoints { get; set; }

        [Required]
        public int EstimatedTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        [MaxLength]
        public string? ThumbnailImage { get; set; }

        // Foreign key for the category
        [Required]
        public string CategoryId { get; set; } = string.Empty;

        [ForeignKey("CategoryId")]
        public virtual CourseCategory Category { get; set; } = null!;

        // Foreign key for the creator
        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<Lesson>? Lessons { get; set; }
    }

    public enum CourseStatus
    {
        Draft,
        Published
    }
}