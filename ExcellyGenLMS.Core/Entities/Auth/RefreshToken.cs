using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Auth
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public bool IsUsed { get; set; } = false;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}