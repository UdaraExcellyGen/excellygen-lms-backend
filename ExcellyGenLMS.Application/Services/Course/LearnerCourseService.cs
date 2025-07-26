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
        private readonly IDocumentProgressRepository _documentProgressRepository;
        private readonly ICourseDocumentRepository _courseDocumentRepository;
        private readonly ILessonRepository _lessonRepository;

        public LearnerCourseService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ILessonProgressRepository lessonProgressRepository,
            IQuizService quizService,
            IFileStorageService fileStorageService,
            ILogger<LearnerCourseService> logger,
            IQuizAttemptRepository quizAttemptRepository,
            IDocumentProgressRepository documentProgressRepository,
            ICourseDocumentRepository courseDocumentRepository,
            ILessonRepository lessonRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonProgressRepository = lessonProgressRepository;
            _quizService = quizService;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _quizAttemptRepository = quizAttemptRepository;
            _documentProgressRepository = documentProgressRepository;
            _courseDocumentRepository = courseDocumentRepository;
            _lessonRepository = lessonRepository;
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

                var allLessonsLookup = (await _courseRepository.GetLessonsByCourseIdsAsync(enrolledCourseIds)).ToLookup(l => l.CourseId);
                var allQuizzesLookup = (await _quizService.GetQuizzesByCourseIdsAsync(enrolledCourseIds)).ToLookup(q => q.LessonId);

                var allQuizIds = allQuizzesLookup.SelectMany(q => q).Select(q => q.QuizId).ToList();
                var lastAttemptByQuizId = (await _quizAttemptRepository.GetCompletedAttemptsByUserAndQuizzesAsync(userId, allQuizIds))
                                          .GroupBy(a => a.QuizId)
                                          .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.CompletionTime).First());

                var completedDocumentIds = (await _documentProgressRepository.GetProgressByUserIdAndCourseIdsAsync(userId, enrolledCourseIds))
                                            .Where(p => p.IsCompleted)
                                            .Select(p => p.DocumentId)
                                            .ToHashSet();

                var enrolledCourses = new List<LearnerCourseDto>();
                foreach (var enrollment in enrollments)
                {
                    if (enrollment.Course == null)
                    {
                        _logger.LogWarning("Skipping enrollment ID {EnrollmentId} because its associated course is null.", enrollment.Id);
                        continue;
                    }

                    var course = enrollment.Course;
                    var courseLessonsFromLookup = allLessonsLookup[course.Id].ToList();

                    // ==========================================================
                    // === START: CORRECTED PROGRESS CALCULATION LOGIC        ===
                    // ==========================================================
                    int totalCourseItems = 0;
                    int completedCourseItems = 0;
                    int completedLessonsCount = 0;

                    var learnerLessons = new List<LearnerLessonDto>();

                    foreach (var lesson in courseLessonsFromLookup)
                    {
                        var quizForLesson = allQuizzesLookup[lesson.Id].FirstOrDefault();
                        lastAttemptByQuizId.TryGetValue(quizForLesson?.QuizId ?? 0, out var lastAttempt);
                        bool isQuizCompletedForLesson = lastAttempt != null;

                        var learnerDocuments = lesson.Documents?.Select(d => new CourseDocumentDto
                        {
                            Id = d.Id,
                            Name = d.Name,
                            DocumentType = d.DocumentType,
                            FileSize = d.FileSize,
                            FileUrl = d.FilePath != null ? _fileStorageService.GetFileUrl(d.FilePath) : string.Empty,
                            LastUpdatedDate = d.LastUpdatedDate,
                            LessonId = d.LessonId,
                            IsCompleted = completedDocumentIds.Contains(d.Id)
                        }).ToList() ?? new List<CourseDocumentDto>();

                        // Step 1: Count items for THIS lesson
                        int docsInLesson = learnerDocuments.Count;
                        int completedDocsInLesson = learnerDocuments.Count(d => d.IsCompleted);
                        int quizInLesson = quizForLesson != null ? 1 : 0;
                        int completedQuizInLesson = isQuizCompletedForLesson ? 1 : 0;

                        // Step 2: Add this lesson's counts to the course-wide totals
                        totalCourseItems += docsInLesson + quizInLesson;
                        completedCourseItems += completedDocsInLesson + completedQuizInLesson;

                        // Step 3: Determine if the lesson itself is fully complete
                        bool isLessonFullyCompleted = (docsInLesson + quizInLesson > 0) && (completedDocsInLesson + completedQuizInLesson) == (docsInLesson + quizInLesson);
                        if (isLessonFullyCompleted)
                        {
                            completedLessonsCount++;
                        }

                        learnerLessons.Add(new LearnerLessonDto
                        {
                            Id = lesson.Id,
                            LessonName = lesson.LessonName,
                            LessonPoints = lesson.LessonPoints,
                            LastUpdatedDate = lesson.LastUpdatedDate,
                            Documents = learnerDocuments,
                            IsCompleted = isLessonFullyCompleted,
                            HasQuiz = quizForLesson != null,
                            QuizId = quizForLesson?.QuizId,
                            IsQuizCompleted = isQuizCompletedForLesson,
                            LastAttemptId = lastAttempt?.QuizAttemptId
                        });
                    }
                    // ========================================================
                    // === END: CORRECTED PROGRESS CALCULATION LOGIC        ===
                    // ========================================================

                    int progressPercentage = totalCourseItems > 0 ? (int)Math.Round((double)completedCourseItems / totalCourseItems * 100) : 0;

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
                        Technologies = course.CourseTechnologies?.Where(ct => ct.Technology != null)
                            .Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name }).ToList() ?? new List<TechnologyDto>(),
                        Status = course.Status,
                        IsEnrolled = true,
                        EnrollmentDate = enrollment.EnrollmentDate,
                        EnrollmentStatus = enrollment.Status,
                        EnrollmentId = enrollment.Id,
                        ProgressPercentage = progressPercentage,
                        TotalLessons = courseLessonsFromLookup.Count,
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

        public async Task<DocumentProgressDto> MarkDocumentCompletedAsync(string userId, int documentId)
        {
            _logger.LogInformation("Attempting to mark document {DocumentId} as complete for user {UserId}", documentId, userId);

            var document = await _courseDocumentRepository.GetByIdAsync(documentId) ?? throw new KeyNotFoundException($"Document with ID {documentId} not found.");

            var lesson = await _lessonRepository.GetByIdAsync(document.LessonId) ?? throw new KeyNotFoundException($"Lesson for document {documentId} not found.");

            _ = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, lesson.CourseId) ?? throw new InvalidOperationException($"User not enrolled in course {lesson.CourseId}.");

            var progress = await _documentProgressRepository.GetProgressByUserIdAndDocumentIdAsync(userId, documentId);
            if (progress == null)
            {
                progress = new DocumentProgress { UserId = userId, DocumentId = documentId, IsCompleted = true, CompletionDate = DateTime.UtcNow };
                await _documentProgressRepository.AddAsync(progress);
            }
            else if (!progress.IsCompleted)
            {
                progress.IsCompleted = true;
                progress.CompletionDate = DateTime.UtcNow;
                await _documentProgressRepository.UpdateAsync(progress);
            }

            return new DocumentProgressDto
            {
                DocumentId = progress.DocumentId,
                IsCompleted = progress.IsCompleted
            };
        }

        public async Task<bool> HasLearnerCompletedAllCourseContentAsync(string userId, int courseId)
        {
            var details = await GetLearnerCourseDetailsAsync(userId, courseId);
            // Added more detailed logging
            _logger.LogInformation("Checking course completion for user {UserId}, course {CourseId}. Calculated Percentage: {Percentage}%", userId, courseId, details?.ProgressPercentage ?? -1);
            return details?.ProgressPercentage == 100;
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
                Category = course.Category != null ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title } : new CategoryDto { Id = "uncategorized", Title = "Uncategorized" },
                Technologies = course.CourseTechnologies?.Where(ct => ct.Technology != null).Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name }).ToList() ?? new List<TechnologyDto>(),
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