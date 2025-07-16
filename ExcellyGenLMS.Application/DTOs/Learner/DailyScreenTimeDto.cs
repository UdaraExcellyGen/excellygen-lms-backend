namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class DailyScreenTimeDto
    {
        public string Day { get; set; } = string.Empty;
        public string FullDate { get; set; } = string.Empty;
        public int? TotalMinutes { get; set; } // Changed to nullable
        public bool IsToday { get; set; }      // Added for convenience
    }
}