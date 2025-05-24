using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Admin; // For Technology entity

namespace ExcellyGenLMS.Core.Entities.Course
{
    [Table("CourseTechnologies")] // Explicit table name
    public class CourseTechnology
    {
        // Composite Key (configured in DbContext using Fluent API)
        public int CourseId { get; set; }
        public string TechnologyId { get; set; } = string.Empty; // Assuming Technology Id is string

        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("TechnologyId")]
        public virtual Technology Technology { get; set; } = null!;
    }
}