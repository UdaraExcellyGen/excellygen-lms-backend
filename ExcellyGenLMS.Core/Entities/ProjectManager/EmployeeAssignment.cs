using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class EmployeeAssignment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProjectId { get; set; } = string.Empty;

        [Required]
        public string EmployeeId { get; set; } = string.Empty; // References User.Id

        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [Range(1, 100)]
        public int WorkloadPercentage { get; set; } = 100;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [ForeignKey("EmployeeId")]
        public User? Employee { get; set; }
    }
}