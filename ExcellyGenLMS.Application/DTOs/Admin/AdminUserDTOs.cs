using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Admin
{
    // Admin-specific DTOs for user management
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; }
        public string JobRole { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string? Avatar { get; set; }

        // New properties for password management
        public bool RequirePasswordChange { get; set; } = false;
        public string? TemporaryPassword { get; set; } // Only populated when a temp password is generated
    }

    public class AdminCreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Department { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Will be optional now
    }

    public class AdminUpdateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Password { get; set; }

        // New property to request temporary password generation
        public bool? GenerateTemporaryPassword { get; set; }
    }

    public class AdminUserSearchParams
    {
        public string? SearchTerm { get; set; }
        public List<string>? Roles { get; set; }
        public string Status { get; set; } = "all";
    }
}