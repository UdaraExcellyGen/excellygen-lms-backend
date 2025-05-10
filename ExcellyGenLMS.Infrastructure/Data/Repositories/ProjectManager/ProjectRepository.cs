// Path: ExcellyGenLMS.Infrastructure/Data/Repositories/ProjectManager/ProjectRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Interfaces.Repositories.ProjectManager;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.ProjectManager
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PMProject>> GetAllAsync(string? status = null)
        {
            var query = _context.PMProjects
                .Include(p => p.Technologies)
                    .ThenInclude(t => t.Technology)
                .Include(p => p.RequiredRoles)
                .Include(p => p.Creator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query.ToListAsync();
        }

        public async Task<PMProject?> GetByIdAsync(string id)
        {
            return await _context.PMProjects
                .Include(p => p.Technologies)
                    .ThenInclude(t => t.Technology)
                .Include(p => p.RequiredRoles)
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PMProject> AddAsync(PMProject project)
        {
            _context.PMProjects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<PMProject> UpdateAsync(PMProject project)
        {
            var existingProject = await _context.PMProjects.FindAsync(project.Id)
                ?? throw new KeyNotFoundException($"Project with ID {project.Id} not found");

            // Update basic properties
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.ShortDescription = project.ShortDescription;
            existingProject.Status = project.Status;
            existingProject.StartDate = project.StartDate;
            existingProject.Deadline = project.Deadline;
            existingProject.Progress = project.Progress;
            existingProject.UpdatedAt = DateTime.UtcNow;

            _context.PMProjects.Update(existingProject);
            await _context.SaveChangesAsync();

            return existingProject;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var project = await _context.PMProjects.FindAsync(id);
            if (project == null)
            {
                return false;
            }

            _context.PMProjects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task UpdateTechnologiesAsync(string projectId, IEnumerable<string> technologyIds)
        {
            // First, remove existing technologies
            var existingTechnologies = await _context.PMProjectTechnologies
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            _context.PMProjectTechnologies.RemoveRange(existingTechnologies);
            await _context.SaveChangesAsync();

            // Then, add new technologies
            if (technologyIds != null && technologyIds.Any())
            {
                foreach (var techId in technologyIds)
                {
                    _context.PMProjectTechnologies.Add(new PMProjectTechnology
                    {
                        ProjectId = projectId,
                        TechnologyId = techId
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateRequiredRolesAsync(string projectId, IEnumerable<PMProjectRequiredRole> requiredRoles)
        {
            // First, remove existing roles
            var existingRoles = await _context.PMProjectRequiredRoles
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();

            _context.PMProjectRequiredRoles.RemoveRange(existingRoles);
            await _context.SaveChangesAsync();

            // Then, add new roles
            if (requiredRoles != null && requiredRoles.Any())
            {
                foreach (var role in requiredRoles)
                {
                    role.ProjectId = projectId; // Ensure project ID is set correctly
                    _context.PMProjectRequiredRoles.Add(role);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}