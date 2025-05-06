using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Admin;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class ProjectTechnology
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        public string TechnologyId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [ForeignKey("TechnologyId")]
        public Technology? Technology { get; set; }
    }
}