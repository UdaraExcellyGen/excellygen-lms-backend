// Path: ExcellyGenLMS.Core/Interfaces/Repositories/Learner/ILearnerNotificationRepository.cs

using ExcellyGenLMS.Core.Entities.Learner;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface ILearnerNotificationRepository
    {
        Task<LearnerNotification> CreateAsync(LearnerNotification notification);
        Task<LearnerNotification?> GetByIdAsync(int id);
        Task<IEnumerable<LearnerNotification>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadCountByUserIdAsync(string userId);
        Task<int> GetTotalCountByUserIdAsync(string userId);
        Task<IEnumerable<LearnerNotification>> GetRecentByUserIdAsync(string userId, int count = 5);
        Task<LearnerNotification> UpdateAsync(LearnerNotification notification);
        Task<bool> DeleteAsync(int id);
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteAllByUserIdAsync(string userId);
    }
}