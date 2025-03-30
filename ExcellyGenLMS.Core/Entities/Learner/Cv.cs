using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Core.Entities.Learner
{
    public class CV
    {
        [Key]
        public int CvId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}