// Path: ExcellyGenLMS.Application/Services/ProjectManager/RoleService.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;
using ExcellyGenLMS.Application.Interfaces.ProjectManager;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;

namespace ExcellyGenLMS.Application.Services.ProjectManager
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(MapRoleToDto);
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return null;
            }
            
            return MapRoleToDto(role);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto createRoleDto)
        {
            var role = new PMRoleDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = createRoleDto.Name,
                CreatedAt = DateTime.UtcNow
            };

            var createdRole = await _roleRepository.AddAsync(role);
            return MapRoleToDto(createdRole);
        }

        public async Task<RoleDto?> UpdateRoleAsync(string id, UpdateRoleDto updateRoleDto)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return null;
            }

            role.Name = updateRoleDto.Name;
            role.UpdatedAt = DateTime.UtcNow;

            var updatedRole = await _roleRepository.UpdateAsync(role);
            return MapRoleToDto(updatedRole);
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            return await _roleRepository.DeleteAsync(id);
        }

        private static RoleDto MapRoleToDto(PMRoleDefinition role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };
        }
    }
}