// File: ExcellyGenLMS.Core/Entities/ProjectManager/PMEmployeeAssignment.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class PMEmployeeAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProjectId { get; set; } = string.Empty;

        [ForeignKey("ProjectId")]
        public virtual PMProject Project { get; set; } = null!;

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [ForeignKey("EmployeeId")]
        public virtual User Employee { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int WorkloadPercentage { get; set; } = 100;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    }
}