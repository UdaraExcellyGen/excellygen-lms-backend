namespace ExcellyGenLMS.Application.DTOs.Auth
{
    public class LoginDto
    {
        public required string Email { get; set; }
        public string? Password { get; set; }
        public string? FirebaseToken { get; set; }
    }

    public class RefreshTokenDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }

    public class ResetPasswordDto
    {
        public required string Email { get; set; }
    }

    public class SelectRoleDto
    {
        public required string UserId { get; set; }
        public required string Role { get; set; }
        public required string AccessToken { get; set; }
    }

    public class AuthResultDto
    {
        public required string UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required List<string> Roles { get; set; }
        public required TokenDto Token { get; set; }
        public bool RequirePasswordChange { get; set; } = false;
        public string? Avatar { get; set; } 
    }
}