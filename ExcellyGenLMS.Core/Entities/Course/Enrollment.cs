// ExcellyGenLMS.Core/Entities/Course/Enrollment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        [Column("enrollment_id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public required string UserId { get; set; }

        [Required]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("enrollment_date")]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [Column("status")]
        public string Status { get; set; } = "active"; // active, completed, withdrawn

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
    }
}