using ExcellyGenLMS.Core.Enums; // For CourseStatus
using System;
using System.Collections.Generic;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    // DTO for a single lesson in the learner's course view, with progress
    public class LearnerLessonDto
    {
        public int Id { get; set; }
        public string LessonName { get; set; } = string.Empty;
        public int LessonPoints { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public List<CourseDocumentDto> Documents { get; set; } = new();
        public bool IsCompleted { get; set; } // Learner's progress on this lesson
        public bool HasQuiz { get; set; } // Indicates if this lesson has an associated quiz
        public int? QuizId { get; set; } // The ID of the quiz for this lesson, if any
        public bool IsQuizCompleted { get; set; } // Indicates if the quiz for this lesson is completed by the learner
        public int? LastAttemptId { get; set; }
    }

    // DTO for a course in the learner's view (e.g., on their dashboard or course content page)
    public class LearnerCourseDto
    {
        public int Id { get; set; } // This is the Course.Id
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EstimatedTime { get; set; } // In Hours
        public string ThumbnailUrl { get; set; } = string.Empty;
        public CategoryDto Category { get; set; } = null!;
        public List<TechnologyDto> Technologies { get; set; } = new();
        public CourseStatus Status { get; set; } // Course status (e.g., Draft, Published)
        public bool IsInactive { get; set; }
        public UserBasicDto Creator { get; set; } = null!; // Using Course namespace UserBasicDto

        // Learner-specific fields
        public bool IsEnrolled { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string EnrollmentStatus { get; set; } = string.Empty; // e.g., "active", "completed", "withdrawn"
        public int ProgressPercentage { get; set; } // Calculated based on completed lessons/quizzes
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public List<LearnerLessonDto> Lessons { get; set; } = new();

        // ADDED: EnrollmentId for handling unenrollment (from Enrollment entity)
        public int? EnrollmentId { get; set; }

        // ADDED: Active learners count for available course cards (shows enrolled student count)
        public int ActiveLearnersCount { get; set; }
    }
}