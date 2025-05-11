using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Auth
{
    public class SendTempPasswordDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string TempPassword { get; set; } = string.Empty;
    }
}