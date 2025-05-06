using System;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class UserTechnologyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
    }
}