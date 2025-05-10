// Path: ExcellyGenLMS.Application/Interfaces/ProjectManager/IRoleService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;

namespace ExcellyGenLMS.Application.Interfaces.ProjectManager
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(string id);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto);
        Task<RoleDto?> UpdateRoleAsync(string id, UpdateRoleDto updateRoleDto);
        Task<bool> DeleteRoleAsync(string id);
    }
}