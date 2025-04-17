namespace ExcellyGenLMS.Application.DTOs.Auth
{
    public class TokenDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public required string CurrentRole { get; set; }
    }
}