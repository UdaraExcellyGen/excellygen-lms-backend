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
        private readonly IQuizService _quizService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<LearnerCourseService> _logger;
        private readonly IQuizAttemptRepository _quizAttemptRepository;

        public LearnerCourseService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ILessonProgressRepository lessonProgressRepository,
            IQuizService quizService,
            IFileStorageService fileStorageService,
            ILogger<LearnerCourseService> logger,
            IQuizAttemptRepository quizAttemptRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonProgressRepository = lessonProgressRepository;
            _quizService = quizService;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _quizAttemptRepository = quizAttemptRepository;
        }

        public async Task<IEnumerable<LearnerCourseDto>> GetAvailableCoursesAsync(string userId, string? categoryId = null)
        {
            _logger.LogInformation("Getting available courses for user {UserId}, category: {CategoryId}", userId, categoryId ?? "All");
            var allCourses = (await _courseRepository.GetAllPublishedCoursesWithDetailsAsync()).ToList();
            var userEnrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);
            var enrolledCourseIds = userEnrollments.Select(e => e.CourseId).ToHashSet();
            var availableCourses = allCourses
                .Where(course => !enrolledCourseIds.Contains(course.Id) &&
                                !course.IsInactive &&
                               (categoryId == null || course.CategoryId == categoryId))
                .Select(course => MapCourseToLightweightDto(course, false))
                .ToList();
            _logger.LogInformation("Found {Count} available courses for user {UserId}.", availableCourses.Count, userId);
            return availableCourses;
        }

        public async Task<IEnumerable<LearnerCourseDto>> GetEnrolledCoursesAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting enrolled courses for user {UserId}", userId);
                var enrollments = (await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId)).ToList();
                if (!enrollments.Any()) return new List<LearnerCourseDto>();

                var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();

                var allLessons = (await _courseRepository.GetLessonsByCourseIdsAsync(enrolledCourseIds)).ToLookup(l => l.CourseId);
                var allProgresses = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdsAsync(userId, enrolledCourseIds)).ToDictionary(p => p.LessonId);
                var allQuizzes = (await _quizService.GetQuizzesByCourseIdsAsync(enrolledCourseIds)).ToLookup(q => q.LessonId);

                var allQuizIds = allQuizzes.SelectMany(q => q).Select(q => q.QuizId).ToList();
                var lastAttemptByQuizId = (await _quizAttemptRepository.GetCompletedAttemptsByUserAndQuizzesAsync(userId, allQuizIds))
                                          .GroupBy(a => a.QuizId)
                                          .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.CompletionTime).First());

                var enrolledCourses = new List<LearnerCourseDto>();
                foreach (var enrollment in enrollments)
                {
                    if (enrollment.Course == null)
                    {
                        _logger.LogWarning("Skipping enrollment ID {EnrollmentId} because its associated course is null.", enrollment.Id);
                        continue;
                    }

                    var course = enrollment.Course;
                    var courseLessons = allLessons[course.Id].ToList();
                    var learnerLessons = courseLessons.Select(lesson =>
                    {
                        allProgresses.TryGetValue(lesson.Id, out var progress);
                        var quizForLesson = allQuizzes[lesson.Id].FirstOrDefault();
                        lastAttemptByQuizId.TryGetValue(quizForLesson?.QuizId ?? 0, out var lastAttempt);

                        return new LearnerLessonDto
                        {
                            Id = lesson.Id,
                            LessonName = lesson.LessonName,
                            LessonPoints = lesson.LessonPoints,
                            IsCompleted = progress?.IsCompleted ?? false,
                            HasQuiz = quizForLesson != null,
                            QuizId = quizForLesson?.QuizId,
                            IsQuizCompleted = lastAttempt != null,
                            LastAttemptId = lastAttempt?.QuizAttemptId,
                            Documents = lesson.Documents?.Select(d => new CourseDocumentDto
                            {
                                Id = d.Id,
                                Name = d.Name,
                                DocumentType = d.DocumentType,
                                FileSize = d.FileSize,
                                FileUrl = d.FilePath != null ? _fileStorageService.GetFileUrl(d.FilePath) : string.Empty,
                                LastUpdatedDate = d.LastUpdatedDate,
                                LessonId = d.LessonId
                            }).ToList() ?? new List<CourseDocumentDto>()
                        };
                    }).ToList();

                    int completedLessonsCount = learnerLessons.Count(l => l.IsCompleted);
                    int totalLessonsCount = learnerLessons.Count;
                    int progressPercentage = totalLessonsCount > 0 ? (int)Math.Round((double)completedLessonsCount / totalLessonsCount * 100) : 0;

                    enrolledCourses.Add(new LearnerCourseDto
                    {
                        Id = course.Id,
                        Title = course.Title,
                        Description = course.Description,
                        EstimatedTime = course.EstimatedTime,
                        IsInactive = course.IsInactive,
                        ThumbnailUrl = course.ThumbnailImagePath != null ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : string.Empty,
                        Creator = new UserBasicDto { Id = course.Creator.Id, Name = course.Creator.Name },
                        Category = course.Category != null
                            ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
                            : new CategoryDto { Id = "uncategorized", Title = "Uncategorized" },
                        Technologies = course.CourseTechnologies?
                            .Where(ct => ct.Technology != null)
                            .Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name })
                            .ToList() ?? new List<TechnologyDto>(),
                        Status = course.Status,
                        IsEnrolled = true,
                        EnrollmentDate = enrollment.EnrollmentDate,
                        EnrollmentStatus = enrollment.Status,
                        EnrollmentId = enrollment.Id,
                        ProgressPercentage = progressPercentage,
                        TotalLessons = totalLessonsCount,
                        CompletedLessons = completedLessonsCount,
                        Lessons = learnerLessons
                    });
                }
                return enrolledCourses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving enrolled courses for user {UserId}", userId);
                throw;
            }
        }

        public async Task<LearnerCourseDto?> GetLearnerCourseDetailsAsync(string userId, int courseId)
        {
            var enrolledCourses = await GetEnrolledCoursesAsync(userId);
            return enrolledCourses.FirstOrDefault(c => c.Id == courseId);
        }

        public async Task<LessonProgressDto> MarkLessonCompletedAsync(string userId, int lessonId)
        {
            var lesson = await _courseRepository.GetLessonWithDocumentsAsync(lessonId) ?? throw new KeyNotFoundException($"Lesson with ID {lessonId} not found.");
            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, lesson.CourseId) ?? throw new InvalidOperationException($"User not enrolled in course {lesson.CourseId}.");

            var progress = await _lessonProgressRepository.GetProgressByUserIdAndLessonIdAsync(userId, lessonId);
            if (progress == null)
            {
                progress = new LessonProgress { UserId = userId, LessonId = lessonId, IsCompleted = true, CompletionDate = DateTime.UtcNow };
                await _lessonProgressRepository.AddAsync(progress);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletionDate = DateTime.UtcNow;
                await _lessonProgressRepository.UpdateAsync(progress);
            }

            // --- START: MODIFIED LOGIC ---
            // Get all lesson IDs for the course
            var lessonsInCourse = (await _courseRepository.GetLessonsByCourseIdAsync(lesson.CourseId)).Select(l => l.Id).ToHashSet();

            // Get all of the user's completed lessons for this course
            var completedLessonsForCourse = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdAsync(userId, lesson.CourseId))
                                             .Where(p => p.IsCompleted)
                                             .Select(p => p.LessonId)
                                             .ToHashSet();

            // Check if the set of completed lessons contains all the lessons for the course
            if (lessonsInCourse.Any() && lessonsInCourse.IsSubsetOf(completedLessonsForCourse))
            {
                // Only update if it's not already marked as completed
                if (enrollment.CompletionDate == null)
                {
                    _logger.LogInformation("All lessons completed for course {CourseId} by user {UserId}. Marking enrollment as complete.", lesson.CourseId, userId);
                    enrollment.Status = "completed";
                    enrollment.CompletionDate = DateTime.UtcNow; // Set the completion date
                    await _enrollmentRepository.UpdateEnrollmentAsync(enrollment);
                }
            }
            // --- END: MODIFIED LOGIC ---

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
            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, courseId);
            // The reliable check is now the CompletionDate, not the status string
            return enrollment?.CompletionDate != null;
        }

        private LearnerCourseDto MapCourseToLightweightDto(Core.Entities.Course.Course course, bool isEnrolled, Enrollment? enrollment = null)
        {
            return new LearnerCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                EstimatedTime = course.EstimatedTime,
                IsInactive = course.IsInactive,
                ThumbnailUrl = course.ThumbnailImagePath != null ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : string.Empty,
                Category = course.Category != null
                    ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
                    : new CategoryDto { Id = "uncategorized", Title = "Uncategorized" },
                Technologies = course.CourseTechnologies?
                    .Where(ct => ct.Technology != null)
                    .Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name })
                    .ToList() ?? new List<TechnologyDto>(),
                Status = course.Status,
                IsEnrolled = isEnrolled,
                EnrollmentDate = enrollment?.EnrollmentDate,
                EnrollmentStatus = enrollment?.Status ?? "not_enrolled",
                EnrollmentId = enrollment?.Id,
                Lessons = new List<LearnerLessonDto>()
            };
        }
    }
}