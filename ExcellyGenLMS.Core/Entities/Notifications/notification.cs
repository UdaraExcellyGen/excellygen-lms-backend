using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.Notifications
{
    public class Notification
    {
        [Key]
        public Guid NotificationID { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string ActorType { get; set; }
        
        [Required]
        public Guid ActorID { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string NotificationType { get; set; }
        
        [StringLength(100)]
        public string? NotificationSubType { get; set; }
        
        [StringLength(255)]
        public string? Subject { get; set; }
        
        [Required]
        public required string Message { get; set; }
        
        [StringLength(50)]
        public string? RelatedEntity { get; set; }
        
        public Guid? RelatedEntityID { get; set; }
        
        [Required]
        public bool IsRead { get; set; } = false;
        
        [Required]
        public bool IsDeleted { get; set; } = false;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ModifiedAt { get; set; }
        
        [StringLength(255)]
        public string? ActionURL { get; set; }
    }
}