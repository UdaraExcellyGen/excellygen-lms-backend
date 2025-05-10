// Path: ExcellyGenLMS.Core/Interfaces/Repositories/ProjectManager/IProjectRepository.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.ProjectManager;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager
{
    public interface IProjectRepository
    {
        Task<IEnumerable<PMProject>> GetAllAsync(string? status = null);
        Task<PMProject?> GetByIdAsync(string id);
        Task<PMProject> AddAsync(PMProject project);
        Task<PMProject> UpdateAsync(PMProject project);
        Task<bool> DeleteAsync(string id);
        Task UpdateTechnologiesAsync(string projectId, IEnumerable<string> technologyIds);
        Task UpdateRequiredRolesAsync(string projectId, IEnumerable<PMProjectRequiredRole> requiredRoles);
    }
}