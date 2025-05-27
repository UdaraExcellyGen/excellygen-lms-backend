using System.Threading.Tasks;
using System.Collections.Generic;
using ExcellyGenLMS.Application.DTOs.Auth;

namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string id);
        Task<UserDto> VerifyCredentialsAsync(string email, string password);
        Task<UserDto> GetUserFromTokenAsync(string token);

        // Add this missing method
        string GetUserIdFromToken(string token);
    }
}