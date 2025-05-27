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
    }
}