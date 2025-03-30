using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Topic { get; set; } // Added 'required' keyword

        public string? Description { get; set; } // Made nullable with ?

        [Required]
        public int CoursePoints { get; set; }

        [Required]
        public int EstimatedTime { get; set; }

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        
        [MaxLength]
        public string? ThumbnailImage { get; set; } // Made nullable with ?

        // Add this to your Course.cs
        public ICollection<Lesson>? Lessons { get; set; }
    }

    public enum CourseStatus
    {
        Draft,
        Published
    }
}