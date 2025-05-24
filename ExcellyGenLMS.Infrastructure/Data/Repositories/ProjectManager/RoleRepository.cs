// Path: ExcellyGenLMS.Infrastructure/Data/Repositories/ProjectManager/RoleRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.ProjectManager
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PMRoleDefinition>> GetAllAsync()
        {
            return await _context.PMRoleDefinitions.ToListAsync();
        }

        public async Task<PMRoleDefinition?> GetByIdAsync(string id)
        {
            return await _context.PMRoleDefinitions.FindAsync(id);
        }

        public async Task<PMRoleDefinition?> GetByNameAsync(string name)
        {
            return await _context.PMRoleDefinitions
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
        }

        public async Task<PMRoleDefinition> AddAsync(PMRoleDefinition role)
        {
            // Set GUID if not provided
            if (string.IsNullOrEmpty(role.Id))
            {
                role.Id = Guid.NewGuid().ToString();
            }

            _context.PMRoleDefinitions.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<PMRoleDefinition> UpdateAsync(PMRoleDefinition role)
        {
            var existingRole = await _context.PMRoleDefinitions.FindAsync(role.Id)
                ?? throw new KeyNotFoundException($"Role with ID {role.Id} not found");

            existingRole.Name = role.Name;
            existingRole.UpdatedAt = DateTime.UtcNow;

            _context.PMRoleDefinitions.Update(existingRole);
            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var role = await _context.PMRoleDefinitions.FindAsync(id);
            if (role == null)
            {
                return false;
            }

            _context.PMRoleDefinitions.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}