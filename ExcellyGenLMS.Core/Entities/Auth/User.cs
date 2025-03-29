using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Auth
{
    public class User
    {
        [Key]
        public required string Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Phone { get; set; }

        [Required]
        public required List<string> Roles { get; set; } = new();

        [Required]
        public required string Department { get; set; }

        [Required]
        public required string Status { get; set; } // "active" or "inactive"

        [Required]
        public required DateTime JoinedDate { get; set; }

        public string JobRole { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string FirebaseUid { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }
}