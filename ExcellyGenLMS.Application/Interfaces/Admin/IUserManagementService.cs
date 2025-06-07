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

        // Add these new permission-based methods
        Task<AdminUserDto> CreateUserAsync(string currentUserId, AdminCreateUserDto createUserDto);
        Task<AdminUserDto?> UpdateUserAsync(string currentUserId, string id, AdminUpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(string currentUserId, string id);
        Task<AdminUserDto?> ToggleUserStatusAsync(string currentUserId, string id);

        // Add SuperAdmin promotion method
        Task<AdminUserDto?> PromoteToSuperAdminAsync(string currentUserId, string targetUserId);
    }
}