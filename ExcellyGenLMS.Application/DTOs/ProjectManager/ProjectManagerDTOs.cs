// Path: ExcellyGenLMS.Application/DTOs/ProjectManager/ProjectManagerDTOs.cs

using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.ProjectManager
{
    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int Progress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public List<TechnologyDto> RequiredSkills { get; set; } = new List<TechnologyDto>();
        public List<RequiredRoleDto> RequiredRoles { get; set; } = new List<RequiredRoleDto>();
    }

    public class CreateProjectDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ShortDescription { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int Progress { get; set; } = 0;
        public List<string> RequiredTechnologyIds { get; set; } = new List<string>();
        public List<RequiredRoleCreateDto> RequiredRoles { get; set; } = new List<RequiredRoleCreateDto>();
    }

    public class UpdateProjectDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ShortDescription { get; set; }
        public required string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int Progress { get; set; }
        public List<string> RequiredTechnologyIds { get; set; } = new List<string>();
        public List<RequiredRoleCreateDto> RequiredRoles { get; set; } = new List<RequiredRoleCreateDto>();
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateRoleDto
    {
        public required string Name { get; set; }
    }

    public class UpdateRoleDto
    {
        public required string Name { get; set; }
    }

    public class RequiredRoleDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RequiredRoleCreateDto
    {
        public required string RoleId { get; set; }
        public int Count { get; set; } = 1;
    }




    public class TechnologyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatorId { get; set; } = string.Empty;   // Add this field
        public string CreatorType { get; set; } = string.Empty; // Add this field
    }

}