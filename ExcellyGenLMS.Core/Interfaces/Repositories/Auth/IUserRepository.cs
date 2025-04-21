using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Auth;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Auth
{
    public interface IUserRepository
    {
        /// <summary>
        /// Gets all users
        /// </summary>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <returns>User if found, null if not found</returns>
        Task<User?> GetUserByIdAsync(string id);

        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <returns>User if found, null if not found</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Adds a new user
        /// </summary>
        Task<bool> AddUserAsync(User user);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Deletes a user by ID
        /// </summary>
        Task<bool> DeleteUserAsync(string id);

        /// <summary>
        /// Searches users by various criteria
        /// </summary>
        Task<List<User>> SearchUsersAsync(string? searchTerm, List<string>? roles, string status);
    }
}