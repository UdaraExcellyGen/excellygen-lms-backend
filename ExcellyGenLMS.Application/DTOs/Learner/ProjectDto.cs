using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class ProjectDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public string Role { get; set; } = string.Empty;
        public List<string> Technologies { get; set; } = new List<string>();
        public List<string> Team { get; set; } = new List<string>();
    }
}