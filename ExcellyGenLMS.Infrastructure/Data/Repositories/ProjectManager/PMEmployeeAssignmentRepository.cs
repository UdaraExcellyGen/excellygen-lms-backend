// Path: ExcellyGenLMS.Infrastructure/Data/Repositories/ProjectManager/PMEmployeeAssignmentRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;
using ExcellyGenLMS.Infrastructure.Data;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.ProjectManager
{
    public class PMEmployeeAssignmentRepository : IPMEmployeeAssignmentRepository
    {
        private readonly ApplicationDbContext _context;

        public PMEmployeeAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PMEmployeeAssignment>> GetAllAsync()
        {
            return await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Include(a => a.Employee)
                .ToListAsync();
        }

        public async Task<PMEmployeeAssignment?> GetByIdAsync(int id)
        {
            return await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<PMEmployeeAssignment>> GetByProjectIdAsync(string projectId)
        {
            return await _context.PMEmployeeAssignments
                .Include(a => a.Employee)
                .Include(a => a.Project)
                .Where(a => a.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PMEmployeeAssignment>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<PMEmployeeAssignment> AddAsync(PMEmployeeAssignment assignment)
        {
            _context.PMEmployeeAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            
            // Return the assignment with included relationships
            return await GetByIdAsync(assignment.Id) ?? assignment;
        }

        public async Task<IEnumerable<PMEmployeeAssignment>> AddRangeAsync(IEnumerable<PMEmployeeAssignment> assignments)
        {
            _context.PMEmployeeAssignments.AddRange(assignments);
            await _context.SaveChangesAsync();
            
            // Return assignments with included relationships
            var assignmentIds = assignments.Select(a => a.Id).ToList();
            return await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Include(a => a.Employee)
                .Where(a => assignmentIds.Contains(a.Id))
                .ToListAsync();
        }

        public async Task<PMEmployeeAssignment> UpdateAsync(PMEmployeeAssignment assignment)
        {
            var existingAssignment = await _context.PMEmployeeAssignments.FindAsync(assignment.Id)
                ?? throw new KeyNotFoundException($"Assignment with ID {assignment.Id} not found");

            existingAssignment.Role = assignment.Role;
            existingAssignment.WorkloadPercentage = assignment.WorkloadPercentage;

            _context.PMEmployeeAssignments.Update(existingAssignment);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(existingAssignment.Id) ?? existingAssignment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var assignment = await _context.PMEmployeeAssignments.FindAsync(id);
            if (assignment == null)
            {
                return false;
            }

            _context.PMEmployeeAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByProjectAndEmployeeAsync(string projectId, string employeeId)
        {
            var assignments = await _context.PMEmployeeAssignments
                .Where(a => a.ProjectId == projectId && a.EmployeeId == employeeId)
                .ToListAsync();

            if (!assignments.Any())
            {
                return false;
            }

            _context.PMEmployeeAssignments.RemoveRange(assignments);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetEmployeeCurrentWorkloadAsync(string employeeId)
        {
            return await _context.PMEmployeeAssignments
                .Where(a => a.EmployeeId == employeeId)
                .SumAsync(a => a.WorkloadPercentage);
        }

        public async Task<bool> HasDuplicateAssignmentAsync(string projectId, string employeeId, string role)
        {
            return await _context.PMEmployeeAssignments
                .AnyAsync(a => a.ProjectId == projectId && 
                              a.EmployeeId == employeeId && 
                              a.Role == role);
        }

        public async Task<IEnumerable<PMEmployeeAssignment>> GetEmployeeAssignmentsWithProjectsAsync(string employeeId)
        {
            return await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId && a.Project.Status == "Active")
                .ToListAsync();
        }

        public async Task<bool> IsEmployeeAssignedToProjectAsync(string projectId, string employeeId)
        {
            return await _context.PMEmployeeAssignments
                .AnyAsync(a => a.ProjectId == projectId && a.EmployeeId == employeeId);
        }

        // NEW OPTIMIZED METHODS: Bulk operations to reduce database calls

        /// <summary>
        /// Get current workload for multiple employees in a single query
        /// </summary>
        public async Task<Dictionary<string, int>> GetEmployeesCurrentWorkloadAsync(List<string> employeeIds)
        {
            if (!employeeIds?.Any() == true)
            {
                return new Dictionary<string, int>();
            }

            var workloads = await _context.PMEmployeeAssignments
                .Where(a => employeeIds.Contains(a.EmployeeId))
                .GroupBy(a => a.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, TotalWorkload = g.Sum(a => a.WorkloadPercentage) })
                .ToListAsync();

            // Ensure all requested employees are in the result, even if they have 0 workload
            return employeeIds.ToDictionary(
                empId => empId,
                empId => workloads.FirstOrDefault(w => w.EmployeeId == empId)?.TotalWorkload ?? 0
            );
        }

        /// <summary>
        /// Get assignments with projects for multiple employees in a single query
        /// </summary>
        public async Task<Dictionary<string, List<PMEmployeeAssignment>>> GetEmployeesAssignmentsWithProjectsAsync(List<string> employeeIds)
        {
            if (!employeeIds?.Any() == true)
            {
                return new Dictionary<string, List<PMEmployeeAssignment>>();
            }

            var assignments = await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Include(a => a.Employee)
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Project.Status == "Active")
                .ToListAsync();

            return employeeIds.ToDictionary(
                empId => empId,
                empId => assignments.Where(a => a.EmployeeId == empId).ToList()
            );
        }

        /// <summary>
        /// Get active project names for multiple employees
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetEmployeesActiveProjectNamesAsync(List<string> employeeIds)
        {
            if (!employeeIds?.Any() == true)
            {
                return new Dictionary<string, List<string>>();
            }

            var projectAssignments = await _context.PMEmployeeAssignments
                .Include(a => a.Project)
                .Where(a => employeeIds.Contains(a.EmployeeId) && a.Project.Status == "Active")
                .Select(a => new { a.EmployeeId, ProjectName = a.Project.Name })
                .ToListAsync();

            return employeeIds.ToDictionary(
                empId => empId,
                empId => projectAssignments
                    .Where(pa => pa.EmployeeId == empId)
                    .Select(pa => pa.ProjectName)
                    .Distinct()
                    .ToList()
            );
        }

        /// <summary>
        /// Check if multiple employees are assigned to specific projects
        /// </summary>
        public async Task<Dictionary<string, bool>> AreEmployeesAssignedToProjectAsync(string projectId, List<string> employeeIds)
        {
            if (!employeeIds?.Any() == true)
            {
                return new Dictionary<string, bool>();
            }

            var assignedEmployees = await _context.PMEmployeeAssignments
                .Where(a => a.ProjectId == projectId && employeeIds.Contains(a.EmployeeId))
                .Select(a => a.EmployeeId)
                .Distinct()
                .ToListAsync();

            return employeeIds.ToDictionary(
                empId => empId,
                empId => assignedEmployees.Contains(empId)
            );
        }

        /// <summary>
        /// Get assignment counts by project for analytics
        /// </summary>
        public async Task<Dictionary<string, int>> GetProjectAssignmentCountsAsync(List<string> projectIds)
        {
            if (!projectIds?.Any() == true)
            {
                return new Dictionary<string, int>();
            }

            var counts = await _context.PMEmployeeAssignments
                .Where(a => projectIds.Contains(a.ProjectId))
                .GroupBy(a => a.ProjectId)
                .Select(g => new { ProjectId = g.Key, Count = g.Count() })
                .ToListAsync();

            return projectIds.ToDictionary(
                projId => projId,
                projId => counts.FirstOrDefault(c => c.ProjectId == projId)?.Count ?? 0
            );
        }
    }
}