// File: ExcellyGenLMS.Core/Entities/ProjectManager/PMProjectRequiredRole.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class PMProjectRequiredRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProjectId { get; set; } = string.Empty;

        [ForeignKey("ProjectId")]
        public virtual PMProject Project { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int Count { get; set; } = 1;
    }
}