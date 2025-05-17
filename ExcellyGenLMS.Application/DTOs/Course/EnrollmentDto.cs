// ExcellyGenLMS.Application/DTOs/Course/EnrollmentDto.cs
using System;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
        public string CourseTitle { get; set; }
    }

    public class CreateEnrollmentDto
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }
        public string Status { get; set; } = "active";
    }

    public class UpdateEnrollmentDto
    {
        public string Status { get; set; }
    }
}