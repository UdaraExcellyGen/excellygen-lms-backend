using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobRole { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserProfileDto
    {
        public string? JobRole { get; set; }
        public string? About { get; set; }
    }
}