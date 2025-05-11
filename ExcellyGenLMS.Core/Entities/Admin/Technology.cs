// Path: ExcellyGenLMS.Core/Entities/Admin/Technology.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Admin
{
    public class Technology
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active"; // active or inactive

        // New fields for creator information
        public string? CreatorId { get; set; }
        public string? CreatorType { get; set; } = "admin"; // admin or project_manager

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}