using System;

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    public class TechnologyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTechnologyDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTechnologyDto
    {
        public string Name { get; set; } = string.Empty;
    }
}