namespace ExcellyGenLMS.Application.DTOs.ProjectManager
{
    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public int Progress { get; set; }
        public List<string> TechnologyIds { get; set; } = new();
        public List<ProjectRoleDto> Roles { get; set; } = new();
    }

    public class ProjectRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
        public int RequiredCount { get; set; }
    }

    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public int Progress { get; set; }
        public List<string>? TechnologyIds { get; set; }
        public List<ProjectRoleDto>? Roles { get; set; }
    }

    public class UpdateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public DateTime? StartDate { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public int Progress { get; set; }
        public List<string>? TechnologyIds { get; set; }
        public List<ProjectRoleDto>? Roles { get; set; }
    }
}