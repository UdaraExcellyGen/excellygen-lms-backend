namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class BadgeDto
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string HowToEarn { get; set; }
        public required string IconPath { get; set; }
        public int CurrentProgress { get; set; }
        public int TargetProgress { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsClaimed { get; set; }
        public string? DateEarned { get; set; }
        public required string Category { get; set; }
        public required string Color { get; set; }
    }
}