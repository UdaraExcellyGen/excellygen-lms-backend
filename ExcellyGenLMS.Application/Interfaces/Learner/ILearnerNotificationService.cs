// Path: ExcellyGenLMS.Application/Interfaces/Learner/ILearnerNotificationService.cs

using ExcellyGenLMS.Application.DTOs.Learner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface ILearnerNotificationService
    {
        Task<LearnerNotificationDto> CreateNotificationAsync(CreateLearnerNotificationDto createDto);
        Task<LearnerNotificationSummaryDto> GetUserNotificationSummaryAsync(string userId);
        Task<IEnumerable<LearnerNotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllNotificationsAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(int notificationId, string userId);

        // Specific notification creators
        Task CreateProjectAssignmentNotificationAsync(string employeeId, string projectId, string projectName, string role, int workloadPercentage, string assignerName);
        Task CreateProjectUpdateNotificationAsync(string employeeId, string projectId, string projectName, string updateType,string? role, int? workloadPercentage,  string assignerName);
        Task CreateProjectRemovalNotificationAsync(string employeeId, string projectId, string projectName, string assignerName);

        //  For badge unlock notifications
        Task CreateBadgeUnlockedNotificationAsync(string userId, string badgeTitle);
    }
}