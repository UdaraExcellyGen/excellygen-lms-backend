// File: ExcellyGenLMS.Core/Entities/ProjectManager/PMProject.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class PMProject
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? Deadline { get; set; }

        public int Progress { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<PMProjectTechnology> Technologies { get; set; } = new List<PMProjectTechnology>();
        public virtual ICollection<PMEmployeeAssignment> EmployeeAssignments { get; set; } = new List<PMEmployeeAssignment>();
        public virtual ICollection<PMProjectRequiredRole> RequiredRoles { get; set; } = new List<PMProjectRequiredRole>();
    }
}

