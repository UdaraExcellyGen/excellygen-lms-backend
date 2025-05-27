// File: ExcellyGenLMS.Core/Entities/ProjectManager/PMProjectTechnology.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExcellyGenLMS.Core.Entities.Admin;

namespace ExcellyGenLMS.Core.Entities.ProjectManager
{
    public class PMProjectTechnology
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProjectId { get; set; } = string.Empty;

        [ForeignKey("ProjectId")]
        public virtual PMProject Project { get; set; } = null!;

        [Required]
        public string TechnologyId { get; set; } = string.Empty;

        [ForeignKey("TechnologyId")]
        public virtual Technology Technology { get; set; } = null!;
    }
}