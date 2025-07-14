using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class Badge
    {
        [Key]
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string HowToEarn { get; set; }
        public required string IconPath { get; set; }
        public int TargetProgress { get; set; }
        public required string Category { get; set; }
        public required string Color { get; set; }
        public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    }
}