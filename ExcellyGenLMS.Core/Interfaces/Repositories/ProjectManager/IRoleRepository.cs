// Path: ExcellyGenLMS.Core/Interfaces/Repositories/ProjectManager/IRoleRepository.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.ProjectManager;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager
{
    public interface IRoleRepository
    {
        Task<IEnumerable<PMRoleDefinition>> GetAllAsync();
        Task<PMRoleDefinition?> GetByIdAsync(string id);
        Task<PMRoleDefinition?> GetByNameAsync(string name);
        Task<PMRoleDefinition> AddAsync(PMRoleDefinition role);
        Task<PMRoleDefinition> UpdateAsync(PMRoleDefinition role);
        Task<bool> DeleteAsync(string id);
    }
}