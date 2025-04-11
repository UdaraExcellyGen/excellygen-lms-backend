using ExcellyGenLMS.Core.Entities.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.Auth
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(string id);
        Task<List<User>> SearchUsersAsync(string? searchTerm, List<string>? roles, string? status);
    }
}