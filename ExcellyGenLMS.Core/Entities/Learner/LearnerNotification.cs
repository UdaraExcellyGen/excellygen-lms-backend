// Path: ExcellyGenLMS.Core/Entities/Learner/LearnerNotification.cs

using System;
using System.ComponentModel.DataAnnotations;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class LearnerNotification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "project_assignment", "project_update", etc.

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional project-related fields
        public string? ProjectId { get; set; }

        [MaxLength(200)]
        public string? ProjectName { get; set; }

        [MaxLength(100)]
        public string? AssignerName { get; set; }

        [MaxLength(100)]
        public string? Role { get; set; }

        public int? WorkloadPercentage { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}