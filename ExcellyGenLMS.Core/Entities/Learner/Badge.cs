using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class Badge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; } // Made nullable since it might be optional

        [StringLength(255)]
        public string? Image { get; set; } // Made nullable since it might be optional

        public DateTime? EarnedDate { get; set; }
    }
}