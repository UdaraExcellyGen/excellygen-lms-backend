using ExcellyGenLMS.Application.DTOs.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Admin
{
    public interface IUserManagementService
    {
        Task<List<AdminUserDto>> GetAllUsersAsync();
        Task<AdminUserDto?> GetUserByIdAsync(string id);
        Task<AdminUserDto> CreateUserAsync(AdminCreateUserDto createUserDto);
        Task<AdminUserDto?> UpdateUserAsync(string id, AdminUpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string id);
        Task<AdminUserDto?> ToggleUserStatusAsync(string id);
        Task<List<AdminUserDto>> SearchUsersAsync(AdminUserSearchParams searchParams);
    }
}