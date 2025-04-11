using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Auth
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required List<string> Roles { get; set; }
        public required string Department { get; set; }
        public required string Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public string JobRole { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
        public string FirebaseUid { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    public class CreateUserDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required List<string> Roles { get; set; }
        public required string Department { get; set; }
        public required string Password { get; set; }
    }

    public class UpdateUserDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required List<string> Roles { get; set; }
        public required string Department { get; set; }
        public string? Password { get; set; }
        public required string Status { get; set; }
    }

    public class UserSearchParams
    {
        public string? SearchTerm { get; set; }
        public List<string>? Roles { get; set; }
        public string Status { get; set; } = "all";
    }
}