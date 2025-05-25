// ExcellyGenLMS.Application/Services/Course/LearnerCourseService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Infrastructure;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class LearnerCourseService : ILearnerCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ILessonProgressRepository _lessonProgressRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptService _quizAttemptService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<LearnerCourseService> _logger;

        public LearnerCourseService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ILessonProgressRepository lessonProgressRepository,
            IQuizRepository quizRepository,
            IQuizAttemptService quizAttemptService,
            IFileStorageService fileStorageService,
            ILogger<LearnerCourseService> logger)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonProgressRepository = lessonProgressRepository;
            _quizRepository = quizRepository;
            _quizAttemptService = quizAttemptService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        private async Task<LearnerCourseDto> MapCourseToLearnerDto(Core.Entities.Course.Course course, string userId, Enrollment? enrollment = null)
        {
            var lessons = (await _courseRepository.GetLessonsByCourseIdAsync(course.Id)).ToList();
            // Get quizzes for all lessons in one go if possible, or iterate
            var quizzesForCourse = new List<Quiz>();
            foreach (var lesson in lessons)
            {
                var quiz = await _quizRepository.GetQuizByLessonIdAsync(lesson.Id);
                if (quiz != null) quizzesForCourse.Add(quiz);
            }

            var lessonProgresses = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdAsync(userId, course.Id)).ToList();

            var learnerLessons = new List<LearnerLessonDto>();
            foreach (var lesson in lessons.OrderBy(l => l.Id))
            {
                var progress = lessonProgresses.FirstOrDefault(lp => lp.LessonId == lesson.Id);
                var quiz = quizzesForCourse.FirstOrDefault(q => q.LessonId == lesson.Id);
                bool isQuizCompleted = false;

                if (quiz != null)
                {
                    isQuizCompleted = await _quizAttemptService.HasUserCompletedQuizAsync(userId, quiz.QuizId);
                }

                learnerLessons.Add(new LearnerLessonDto
                {
                    Id = lesson.Id,
                    LessonName = lesson.LessonName,
                    LessonPoints = lesson.LessonPoints,
                    LastUpdatedDate = lesson.LastUpdatedDate,
                    Documents = lesson.Documents.Select(d => new CourseDocumentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        DocumentType = d.DocumentType,
                        FileSize = d.FileSize,
                        FileUrl = d.FilePath != null ? _fileStorageService.GetFileUrl(d.FilePath) : string.Empty,
                        LastUpdatedDate = d.LastUpdatedDate,
                        LessonId = d.LessonId
                    }).ToList(),
                    IsCompleted = progress?.IsCompleted ?? false,
                    HasQuiz = quiz != null,
                    QuizId = quiz?.QuizId,
                    IsQuizCompleted = isQuizCompleted
                });
            }

            int completedLessonsCount = learnerLessons.Count(l => l.IsCompleted && (!l.HasQuiz || (l.HasQuiz && l.IsQuizCompleted)));
            int totalContentItems = learnerLessons.Count;
            int progressPercentage = (totalContentItems > 0) ? (int)Math.Round((double)completedLessonsCount / totalContentItems * 100) : 0;

            return new LearnerCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                EstimatedTime = course.EstimatedTime,
                ThumbnailUrl = course.ThumbnailImagePath != null ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : string.Empty,
                Category = new CategoryDto { Id = course.Category.Id, Title = course.Category.Title },
                Technologies = course.CourseTechnologies.Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name }).ToList(),
                Status = course.Status,
                IsEnrolled = enrollment != null,
                EnrollmentDate = enrollment?.EnrollmentDate,
                EnrollmentStatus = enrollment?.Status ?? "not_enrolled",
                ProgressPercentage = progressPercentage,
                TotalLessons = totalContentItems,
                CompletedLessons = completedLessonsCount,
                Lessons = learnerLessons,
                EnrollmentId = enrollment?.Id // ADDED: Populate EnrollmentId
            };
        }

        public async Task<IEnumerable<LearnerCourseDto>> GetAvailableCoursesAsync(string userId, string? categoryId = null)
        {
            _logger.LogInformation("Getting available courses for user {UserId}, category: {CategoryId}", userId, categoryId ?? "All");

            var allPublishedCourses = await _courseRepository.GetAllPublishedCoursesWithDetailsAsync();
            var userEnrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);
            var enrolledCourseIds = userEnrollments.Select(e => e.CourseId).ToHashSet();

            var availableCourses = new List<LearnerCourseDto>();
            foreach (var course in allPublishedCourses)
            {
                if (!enrolledCourseIds.Contains(course.Id) && (categoryId == null || course.CategoryId == categoryId))
                {
                    availableCourses.Add(await MapCourseToLearnerDto(course, userId));
                }
            }
            _logger.LogInformation("Found {Count} available courses for user {UserId}.", availableCourses.Count, userId);
            return availableCourses;
        }

        public async Task<IEnumerable<LearnerCourseDto>> GetEnrolledCoursesAsync(string userId)
        {
            _logger.LogInformation("Getting enrolled courses for user {UserId}", userId);

            var userEnrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);

            var enrolledCourses = new List<LearnerCourseDto>();
            foreach (var enrollment in userEnrollments)
            {
                var course = await _courseRepository.GetByIdWithDetailsAsync(enrollment.CourseId);
                if (course != null)
                {
                    enrolledCourses.Add(await MapCourseToLearnerDto(course, userId, enrollment));
                }
            }
            _logger.LogInformation("Found {Count} enrolled courses for user {UserId}.", enrolledCourses.Count, userId);
            return enrolledCourses;
        }

        public async Task<LearnerCourseDto?> GetLearnerCourseDetailsAsync(string userId, int courseId)
        {
            _logger.LogInformation("Getting learner course details for user {UserId} and course {CourseId}", userId, courseId);

            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found.", courseId);
                return null;
            }

            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, courseId);
            return await MapCourseToLearnerDto(course, userId, enrollment);
        }

        public async Task<LessonProgressDto> MarkLessonCompletedAsync(string userId, int lessonId)
        {
            _logger.LogInformation("Attempting to mark lesson {LessonId} as completed for user {UserId}", lessonId, userId);

            var lesson = await _courseRepository.GetLessonWithDocumentsAsync(lessonId);
            if (lesson == null)
            {
                throw new KeyNotFoundException($"Lesson with ID {lessonId} not found.");
            }

            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, lesson.CourseId);
            if (enrollment == null)
            {
                throw new InvalidOperationException($"User {userId} is not enrolled in the course ({lesson.CourseId}) for lesson {lessonId}.");
            }

            var progress = await _lessonProgressRepository.GetProgressByUserIdAndLessonIdAsync(userId, lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletionDate = DateTime.UtcNow
                };
                await _lessonProgressRepository.AddAsync(progress);
                _logger.LogInformation("Created new progress record for lesson {LessonId} for user {UserId}.", lessonId, userId);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletionDate = DateTime.UtcNow;
                await _lessonProgressRepository.UpdateAsync(progress);
                _logger.LogInformation("Updated progress record for lesson {LessonId} for user {UserId}.", lessonId, userId);
            }
            else
            {
                _logger.LogInformation("Lesson {LessonId} already marked as completed for user {UserId}.", lessonId, userId);
            }

            return new LessonProgressDto
            {
                Id = progress.Id,
                UserId = progress.UserId,
                LessonId = progress.LessonId,
                LessonName = lesson.LessonName,
                IsCompleted = progress.IsCompleted,
                CompletionDate = progress.CompletionDate
            };
        }

        public async Task<bool> HasLearnerCompletedAllCourseContentAsync(string userId, int courseId)
        {
            _logger.LogInformation("Checking if user {UserId} has completed all content for course {CourseId}", userId, courseId);

            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }

            var lessonsInCourse = course.Lessons.ToList();
            if (!lessonsInCourse.Any())
            {
                _logger.LogInformation("Course {CourseId} has no lessons.", courseId);
                return true;
            }

            var lessonProgresses = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdAsync(userId, course.Id)).ToList();

            foreach (var lesson in lessonsInCourse)
            {
                var progress = lessonProgresses.FirstOrDefault(lp => lp.LessonId == lesson.Id);
                bool lessonMarkedCompleted = progress?.IsCompleted ?? false;

                var quiz = await _quizRepository.GetQuizByLessonIdAsync(lesson.Id);
                bool quizIsCompleted = true;
                if (quiz != null)
                {
                    quizIsCompleted = await _quizAttemptService.HasUserCompletedQuizAsync(userId, quiz.QuizId);
                }

                if (!lessonMarkedCompleted || !quizIsCompleted)
                {
                    _logger.LogDebug("User {UserId} has not completed lesson {LessonId} (marked completed: {LessonCompleted}, quiz completed: {QuizCompleted}).", userId, lesson.Id, lessonMarkedCompleted, quizIsCompleted);
                    return false;
                }
            }
            _logger.LogInformation("User {UserId} has completed all content for course {CourseId}.", userId, courseId);
            return true;
        }
    }
}