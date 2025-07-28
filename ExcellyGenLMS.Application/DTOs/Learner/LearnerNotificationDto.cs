// Path: ExcellyGenLMS.Application/DTOs/Learner/LearnerNotificationDto.cs

using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class LearnerNotificationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; 
        public bool IsRead { get; set; }
        // MODIFIED: This change is ESSENTIAL. It must be a string.
        public string CreatedAt { get; set; } = string.Empty; 
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? AssignerName { get; set; }
        public string? Role { get; set; }
        public int? WorkloadPercentage { get; set; }
    }

    public class CreateLearnerNotificationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? AssignerName { get; set; }
        public string? Role { get; set; }
        public int? WorkloadPercentage { get; set; }
    }

    public class LearnerNotificationSummaryDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public List<LearnerNotificationDto> RecentNotifications { get; set; } = new List<LearnerNotificationDto>();
    }

    public class MarkNotificationReadDto
    {
        public int NotificationId { get; set; }
    }

    public class MarkAllNotificationsReadDto
    {
        public string UserId { get; set; } = string.Empty;
    }
}