using System;

namespace ExcellyGenLMS.Application.DTOs.Auth
{
    public class HeartbeatDto
    {
        public string? AccessToken { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}