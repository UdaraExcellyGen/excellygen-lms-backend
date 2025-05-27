// Path: ExcellyGenLMS.Application/Interfaces/ProjectManager/IEmployeeAssignmentService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;

namespace ExcellyGenLMS.Application.Interfaces.ProjectManager
{
    public interface IEmployeeAssignmentService
    {
        Task<IEnumerable<EmployeeDto>> GetAvailableEmployeesAsync(EmployeeFilterDto? filter = null);
        Task<IEnumerable<EmployeeDto>> GetEmployeesWithMatchingSkillsAsync(List<string> requiredSkills);
        Task<EmployeeDto> GetEmployeeByIdAsync(string employeeId);
        Task<EmployeeWorkloadDto> GetEmployeeWorkloadAsync(string employeeId);
        
        Task<EmployeeAssignmentDto> AssignEmployeeToProjectAsync(CreateEmployeeAssignmentDto request);
        Task<IEnumerable<EmployeeAssignmentDto>> AssignMultipleEmployeesToProjectAsync(BulkAssignEmployeesDto request);
        Task<EmployeeAssignmentDto?> UpdateEmployeeAssignmentAsync(int assignmentId, UpdateEmployeeAssignmentDto request);
        Task<bool> RemoveEmployeeFromProjectAsync(int assignmentId);
        Task<bool> RemoveEmployeeFromProjectByIdsAsync(string projectId, string employeeId);
        
        Task<IEnumerable<EmployeeAssignmentDto>> GetProjectAssignmentsAsync(string projectId);
        Task<IEnumerable<EmployeeAssignmentDto>> GetEmployeeAssignmentsAsync(string employeeId);
        
        Task<bool> ValidateAssignmentAsync(string employeeId, int workloadPercentage);
        Task<IEnumerable<string>> GetEmployeeSkillsAsync(string employeeId);
    }
}