using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class Project
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Assigned"; // Assigned, In Progress, Completed

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [Required]
        [StringLength(100)]
        public required string Role { get; set; }

        public virtual ICollection<ProjectTechnology> Technologies { get; set; } = new List<ProjectTechnology>();
        public virtual ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    }
}