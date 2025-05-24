// Path: ExcellyGenLMS.Core/Interfaces/Repositories/ProjectManager/IPMEmployeeAssignmentRepository.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.ProjectManager;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager
{
    public interface IPMEmployeeAssignmentRepository
    {
        Task<IEnumerable<PMEmployeeAssignment>> GetAllAsync();
        Task<PMEmployeeAssignment?> GetByIdAsync(int id);
        Task<IEnumerable<PMEmployeeAssignment>> GetByProjectIdAsync(string projectId);
        Task<IEnumerable<PMEmployeeAssignment>> GetByEmployeeIdAsync(string employeeId);
        Task<PMEmployeeAssignment> AddAsync(PMEmployeeAssignment assignment);
        Task<IEnumerable<PMEmployeeAssignment>> AddRangeAsync(IEnumerable<PMEmployeeAssignment> assignments);
        Task<PMEmployeeAssignment> UpdateAsync(PMEmployeeAssignment assignment);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByProjectAndEmployeeAsync(string projectId, string employeeId);
        Task<int> GetEmployeeCurrentWorkloadAsync(string employeeId);
        Task<bool> HasDuplicateAssignmentAsync(string projectId, string employeeId, string role);
        Task<IEnumerable<PMEmployeeAssignment>> GetEmployeeAssignmentsWithProjectsAsync(string employeeId);
        Task<bool> IsEmployeeAssignedToProjectAsync(string projectId, string employeeId);
    }
}