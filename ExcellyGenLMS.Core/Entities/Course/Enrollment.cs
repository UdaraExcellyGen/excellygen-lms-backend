// Enrollment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Course // Corrected Namespace
{
    [Table("Enrollments")] // Corrected Table name to "Enrollments" (plural convention)
    public class Enrollment
    {
        [Key] // Primary Key attribute
        [Column("enrollment_id")] // Explicitly name the column
        public int Id { get; set; } // Changed property name to Id for convention

        [Column("enrollment_time")] // Explicitly name the column
        public TimeSpan Time { get; set; } // Use TimeSpan for time

        [Column("enrollment_date")] // Explicitly name the column
        public DateTime Date { get; set; } // Use DateTime for date
    }
}