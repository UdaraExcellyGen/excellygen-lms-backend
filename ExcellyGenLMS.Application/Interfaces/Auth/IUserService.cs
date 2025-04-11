using ExcellyGenLMS.Application.DTOs.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Auth
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task DeleteUserAsync(string id);
        Task<UserDto> ToggleUserStatusAsync(string id);
        Task<List<UserDto>> SearchUsersAsync(UserSearchParams searchParams);
    }
}