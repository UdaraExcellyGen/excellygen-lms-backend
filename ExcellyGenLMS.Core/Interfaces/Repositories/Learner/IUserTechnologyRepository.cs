using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Learner
{
    public interface IUserTechnologyRepository
    {
        Task<List<UserTechnology>> GetUserTechnologiesAsync(string userId);
        Task<List<Technology>> GetAvailableTechnologiesAsync(string userId);
        Task<UserTechnology> AddUserTechnologyAsync(string userId, string technologyId);
        Task RemoveUserTechnologyAsync(string userId, string technologyId);

        // NEW OPTIMIZED METHODS: Bulk operations to reduce N+1 queries
        
        /// <summary>
        /// Get skills (technology names) for multiple users in a single query
        /// This replaces individual calls to GetUserTechnologiesAsync for each employee
        /// </summary>
        /// <param name="userIds">List of user IDs to get skills for</param>
        /// <returns>Dictionary mapping user ID to list of their skill names</returns>
        Task<Dictionary<string, List<string>>> GetSkillsForMultipleUsersAsync(List<string> userIds);
        
        /// <summary>
        /// Get technology count for multiple users in a single query
        /// Useful for displaying skill statistics on employee cards
        /// </summary>
        /// <param name="userIds">List of user IDs to get technology counts for</param>
        /// <returns>Dictionary mapping user ID to their technology count</returns>
        Task<Dictionary<string, int>> GetTechnologyCountForMultipleUsersAsync(List<string> userIds);
    }
}