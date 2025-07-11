// Path: ExcellyGenLMS.Application/Services/Learner/LearnerNotificationService.cs

using Microsoft.Extensions.Logging;
using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class LearnerNotificationService : ILearnerNotificationService
    {
        private readonly ILearnerNotificationRepository _notificationRepository;
        private readonly ILogger<LearnerNotificationService> _logger;

        public LearnerNotificationService(
            ILearnerNotificationRepository notificationRepository,
            ILogger<LearnerNotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<LearnerNotificationDto> CreateNotificationAsync(CreateLearnerNotificationDto createDto)
        {
            _logger.LogInformation($"Creating notification for user {createDto.UserId}");

            var notification = new LearnerNotification
            {
                UserId = createDto.UserId,
                Title = createDto.Title,
                Message = createDto.Message,
                Type = createDto.Type,
                ProjectId = createDto.ProjectId,
                ProjectName = createDto.ProjectName,
                AssignerName = createDto.AssignerName,
                Role = createDto.Role,
                WorkloadPercentage = createDto.WorkloadPercentage,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            return MapToDto(createdNotification);
        }

        public async Task<LearnerNotificationSummaryDto> GetUserNotificationSummaryAsync(string userId)
        {
            var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
            var totalCount = await _notificationRepository.GetTotalCountByUserIdAsync(userId);
            var recentNotifications = await _notificationRepository.GetRecentByUserIdAsync(userId, 5);

            return new LearnerNotificationSummaryDto
            {
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                RecentNotifications = recentNotifications.Select(MapToDto).ToList()
            };
        }

        public async Task<IEnumerable<LearnerNotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId, page, pageSize);
            return notifications.Select(MapToDto);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, string userId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification?.UserId != userId) return false;

            return await _notificationRepository.DeleteAsync(notificationId);
        }

        public async Task CreateProjectAssignmentNotificationAsync(string employeeId, string projectId, string projectName, string role, int workloadPercentage, string assignerName)
        {
            var createDto = new CreateLearnerNotificationDto
            {
                UserId = employeeId,
                Title = "New Project Assignment",
                Message = $"You have been assigned to project '{projectName}' as {role} with {workloadPercentage}% workload by {assignerName}.",
                Type = "project_assignment",
                ProjectId = projectId,
                ProjectName = projectName,
                AssignerName = assignerName,
                Role = role,
                WorkloadPercentage = workloadPercentage
            };

            await CreateNotificationAsync(createDto);
            _logger.LogInformation($"Created project assignment notification for employee {employeeId} on project {projectId}");
        }

        public async Task CreateProjectUpdateNotificationAsync(string employeeId, string projectId, string projectName, string updateType, string assignerName)
        {
            var createDto = new CreateLearnerNotificationDto
            {
                UserId = employeeId,
                Title = "Project Assignment Updated",
                Message = $"Your assignment to project '{projectName}' has been updated by {assignerName}. Update type: {updateType}",
                Type = "project_update",
                ProjectId = projectId,
                ProjectName = projectName,
                AssignerName = assignerName
            };

            await CreateNotificationAsync(createDto);
            _logger.LogInformation($"Created project update notification for employee {employeeId} on project {projectId}");
        }

        public async Task CreateProjectRemovalNotificationAsync(string employeeId, string projectId, string projectName, string assignerName)
        {
            var createDto = new CreateLearnerNotificationDto
            {
                UserId = employeeId,
                Title = "Project Assignment Removed",
                Message = $"You have been removed from project '{projectName}' by {assignerName}.",
                Type = "project_removal",
                ProjectId = projectId,
                ProjectName = projectName,
                AssignerName = assignerName
            };

            await CreateNotificationAsync(createDto);
            _logger.LogInformation($"Created project removal notification for employee {employeeId} on project {projectId}");
        }

        private static LearnerNotificationDto MapToDto(LearnerNotification notification)
        {
            return new LearnerNotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ProjectId = notification.ProjectId,
                ProjectName = notification.ProjectName,
                AssignerName = notification.AssignerName,
                Role = notification.Role,
                WorkloadPercentage = notification.WorkloadPercentage
            };
        }
    }
}