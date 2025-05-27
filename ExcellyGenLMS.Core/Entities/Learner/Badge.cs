using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class Badge
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Icon { get; set; } // Icon name (Trophy, Book, Award, etc.)

        [StringLength(50)]
        public string? Color { get; set; } = "#BF4BF6"; // Color hex code for badges

        [StringLength(255)]
        public string? ImagePath { get; set; } // Path to badge image (PNG)


        public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    }
}