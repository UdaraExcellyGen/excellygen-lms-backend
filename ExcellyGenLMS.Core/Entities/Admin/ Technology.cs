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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}