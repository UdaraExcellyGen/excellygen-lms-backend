// Path: ExcellyGenLMS.Core/Interfaces/Repositories/ProjectManager/IPMEmployeeAssignmentRepository.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.ProjectManager;

namespace ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager
{
    public interface IPMEmployeeAssignmentRepository
    {
        // EXISTING METHODS (unchanged)
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

        // NEW OPTIMIZED METHODS: Bulk operations to reduce database calls and fix N+1 query problems

        /// <summary>
        /// Get current workload for multiple employees in a single query
        /// This replaces multiple individual calls to GetEmployeeCurrentWorkloadAsync
        /// </summary>
        /// <param name="employeeIds">List of employee IDs to get workloads for</param>
        /// <returns>Dictionary mapping employee ID to their current total workload percentage</returns>
        Task<Dictionary<string, int>> GetEmployeesCurrentWorkloadAsync(List<string> employeeIds);

        /// <summary>
        /// Get assignments with projects for multiple employees in a single query
        /// This replaces multiple individual calls to GetEmployeeAssignmentsWithProjectsAsync
        /// </summary>
        /// <param name="employeeIds">List of employee IDs to get assignments for</param>
        /// <returns>Dictionary mapping employee ID to their list of assignments with project data included</returns>
        Task<Dictionary<string, List<PMEmployeeAssignment>>> GetEmployeesAssignmentsWithProjectsAsync(List<string> employeeIds);

        /// <summary>
        /// Get active project names for multiple employees in a single query
        /// Useful for displaying project lists on employee cards without loading full assignment data
        /// </summary>
        /// <param name="employeeIds">List of employee IDs to get active project names for</param>
        /// <returns>Dictionary mapping employee ID to list of their active project names</returns>
        Task<Dictionary<string, List<string>>> GetEmployeesActiveProjectNamesAsync(List<string> employeeIds);

        /// <summary>
        /// Check if multiple employees are assigned to a specific project in a single query
        /// Useful for bulk validation operations
        /// </summary>
        /// <param name="projectId">The project ID to check assignments for</param>
        /// <param name="employeeIds">List of employee IDs to check</param>
        /// <returns>Dictionary mapping employee ID to boolean indicating if they're assigned to the project</returns>
        Task<Dictionary<string, bool>> AreEmployeesAssignedToProjectAsync(string projectId, List<string> employeeIds);

        /// <summary>
        /// Get assignment counts by project for analytics and reporting
        /// Useful for displaying team sizes on project cards
        /// </summary>
        /// <param name="projectIds">List of project IDs to get assignment counts for</param>
        /// <returns>Dictionary mapping project ID to the number of employees assigned to it</returns>
        Task<Dictionary<string, int>> GetProjectAssignmentCountsAsync(List<string> projectIds);
    }
}