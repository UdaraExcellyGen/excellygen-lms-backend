using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class Project
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Completed

        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Progress { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<ProjectTechnology> ProjectTechnologies { get; set; } = new List<ProjectTechnology>();
        public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
        public ICollection<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>();
    }
}