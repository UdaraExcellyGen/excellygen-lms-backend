// ExcellyGenLMS.Application/DTOs/Course/LessonProgressDto.cs
using System;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    public class LessonProgressDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int LessonId { get; set; }
        public string LessonName { get; set; } = string.Empty; // For convenience
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public class MarkLessonCompletedDto
    {
        public int LessonId { get; set; }
    }
}