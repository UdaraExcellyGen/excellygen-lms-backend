using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class ProjectRole
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RequiredCount { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
    }
}