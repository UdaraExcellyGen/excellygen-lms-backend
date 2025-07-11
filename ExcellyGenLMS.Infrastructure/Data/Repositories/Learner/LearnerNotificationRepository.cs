// Path: ExcellyGenLMS.Infrastructure/Data/Repositories/Learner/LearnerNotificationRepository.cs

using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Infrastructure.Data;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Learner
{
    public class LearnerNotificationRepository : ILearnerNotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public LearnerNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LearnerNotification> CreateAsync(LearnerNotification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            _context.LearnerNotifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<LearnerNotification?> GetByIdAsync(int id)
        {
            return await _context.LearnerNotifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<LearnerNotification>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.LearnerNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByUserIdAsync(string userId)
        {
            return await _context.LearnerNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<int> GetTotalCountByUserIdAsync(string userId)
        {
            return await _context.LearnerNotifications
                .Where(n => n.UserId == userId)
                .CountAsync();
        }

        public async Task<IEnumerable<LearnerNotification>> GetRecentByUserIdAsync(string userId, int count = 5)
        {
            return await _context.LearnerNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<LearnerNotification> UpdateAsync(LearnerNotification notification)
        {
            _context.LearnerNotifications.Update(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _context.LearnerNotifications.FindAsync(id);
            if (notification == null) return false;

            _context.LearnerNotifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            var notification = await _context.LearnerNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.LearnerNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByUserIdAsync(string userId)
        {
            var notifications = await _context.LearnerNotifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.LearnerNotifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}