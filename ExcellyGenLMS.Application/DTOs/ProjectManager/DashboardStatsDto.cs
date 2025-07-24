// Path: ExcellyGenLMS.Application/DTOs/ProjectManager/DashboardStatsDto.cs

namespace ExcellyGenLMS.Application.DTOs.ProjectManager
{
    public class ProjectManagerDashboardStatsDto
    {
        public ProjectStatsDto Projects { get; set; } = new();
        public EmployeeStatsDto Employees { get; set; } = new();
        public TechnologyStatsDto Technologies { get; set; } = new();
    }

    public class ProjectStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int WithEmployees { get; set; }
    }

    public class EmployeeStatsDto
    {
        public int Total { get; set; }
        public int OnProjects { get; set; }
        public int Available { get; set; }
        public int FullyUtilized { get; set; }
    }

    public class TechnologyStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
    }
}