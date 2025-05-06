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

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.ProjectTechnologies)
                    .ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectRoles)
                .Include(p => p.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(string id)
        {
            return await _context.Projects
                .Include(p => p.ProjectTechnologies)
                    .ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectRoles)
                .Include(p => p.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task DeleteProjectAsync(string id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsByStatusAsync(string status)
        {
            return await _context.Projects
                .Where(p => p.Status == status)
                .Include(p => p.ProjectTechnologies)
                    .ThenInclude(pt => pt.Technology)
                .Include(p => p.ProjectRoles)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}