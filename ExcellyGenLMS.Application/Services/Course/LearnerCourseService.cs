//// ExcellyGenLMS.Application/Services/Course/LearnerCourseService.cs

//using ExcellyGenLMS.Application.DTOs.Course;
//using ExcellyGenLMS.Application.Interfaces.Course;
//using ExcellyGenLMS.Core.Entities.Course;
//using ExcellyGenLMS.Core.Interfaces.Infrastructure;
//using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ExcellyGenLMS.Application.Services.Course
//{
//    public class LearnerCourseService : ILearnerCourseService
//    {
//        private readonly ICourseRepository _courseRepository;
//        private readonly IEnrollmentRepository _enrollmentRepository;
//        private readonly ILessonProgressRepository _lessonProgressRepository;
//        private readonly IQuizService _quizService;
//        private readonly IQuizAttemptService _quizAttemptService;
//        private readonly IFileStorageService _fileStorageService;
//        private readonly ILogger<LearnerCourseService> _logger;

//        public LearnerCourseService(
//            ICourseRepository courseRepository,
//            IEnrollmentRepository enrollmentRepository,
//            ILessonProgressRepository lessonProgressRepository,
//            IQuizService quizService,
//            IQuizAttemptService quizAttemptService,
//            IFileStorageService fileStorageService,
//            ILogger<LearnerCourseService> logger)
//        {
//            _courseRepository = courseRepository;
//            _enrollmentRepository = enrollmentRepository;
//            _lessonProgressRepository = lessonProgressRepository;
//            _quizService = quizService;
//            _quizAttemptService = quizAttemptService;
//            _fileStorageService = fileStorageService;
//            _logger = logger;
//        }

//        private LearnerCourseDto MapCourseToLightweightDto(Core.Entities.Course.Course course, bool isEnrolled, Enrollment? enrollment = null)
//        {
//            return new LearnerCourseDto
//            {
//                Id = course.Id,
//                Title = course.Title,
//                Description = course.Description,
//                EstimatedTime = course.EstimatedTime,
//                ThumbnailUrl = course.ThumbnailImagePath != null ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : string.Empty,
//                Category = course.Category != null
//                    ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
//                    : new CategoryDto { Id = "uncategorized", Title = "Uncategorized" },
//                Technologies = course.CourseTechnologies?
//                    .Where(ct => ct.Technology != null)
//                    .Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name })
//                    .ToList() ?? new List<TechnologyDto>(),
//                Status = course.Status,
//                IsEnrolled = isEnrolled,
//                EnrollmentDate = enrollment?.EnrollmentDate,
//                EnrollmentStatus = enrollment?.Status ?? "not_enrolled",
//                EnrollmentId = enrollment?.Id,
//                ProgressPercentage = 0,
//                TotalLessons = 0,
//                CompletedLessons = 0,
//                Lessons = new List<LearnerLessonDto>()
//            };
//        }

//        private async Task<LearnerCourseDto> MapCourseToDetailedDto(Core.Entities.Course.Course course, string userId, Enrollment? enrollment = null)
//        {
//            var courseId = course.Id;

//            // Await tasks sequentially to prevent DbContext concurrency issues.
//            var lessons = (await _courseRepository.GetLessonsByCourseIdAsync(courseId)).ToList();
//            var progresses = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdAsync(userId, courseId)).ToList();
//            var quizDtos = (await _quizService.GetQuizzesByCourseIdAsync(courseId)).ToList();

//            var quizByLessonId = quizDtos.ToDictionary(q => q.LessonId, q => q);

//            var quizIds = quizDtos.Select(q => q.QuizId).ToList();

//            var quizCompletionTasks = quizIds.Select(async quizId =>
//                new { QuizId = quizId, IsCompleted = await _quizAttemptService.HasUserCompletedQuizAsync(userId, quizId) }
//            );
//            var quizCompletions = await Task.WhenAll(quizCompletionTasks);
//            var quizCompletionLookup = quizCompletions.ToDictionary(qc => qc.QuizId, qc => qc.IsCompleted);

//            var learnerLessons = new List<LearnerLessonDto>();
//            foreach (var lesson in lessons.OrderBy(l => l.Id))
//            {
//                var progress = progresses.FirstOrDefault(lp => lp.LessonId == lesson.Id);
//                var hasQuiz = quizByLessonId.TryGetValue(lesson.Id, out var quizDto);
//                bool isQuizCompleted = hasQuiz && quizDto != null &&
//                    quizCompletionLookup.TryGetValue(quizDto.QuizId, out var completed) && completed;

//                learnerLessons.Add(new LearnerLessonDto
//                {
//                    Id = lesson.Id,
//                    LessonName = lesson.LessonName,
//                    LessonPoints = lesson.LessonPoints,
//                    LastUpdatedDate = lesson.LastUpdatedDate,
//                    Documents = lesson.Documents?.Select(d => new CourseDocumentDto
//                    {
//                        Id = d.Id,
//                        Name = d.Name,
//                        DocumentType = d.DocumentType,
//                        FileSize = d.FileSize,
//                        FileUrl = d.FilePath != null ? _fileStorageService.GetFileUrl(d.FilePath) : string.Empty,
//                        LastUpdatedDate = d.LastUpdatedDate,
//                        LessonId = d.LessonId
//                    }).ToList() ?? new List<CourseDocumentDto>(),
//                    IsCompleted = progress?.IsCompleted ?? false,
//                    HasQuiz = hasQuiz,
//                    QuizId = quizDto?.QuizId,
//                    IsQuizCompleted = isQuizCompleted
//                });
//            }

//            int completedLessonsCount = learnerLessons.Count(l => l.IsCompleted && (!l.HasQuiz || l.IsQuizCompleted));
//            int totalContentItems = learnerLessons.Count;
//            int progressPercentage = totalContentItems > 0 ? (int)Math.Round((double)completedLessonsCount / totalContentItems * 100) : 0;

//            return new LearnerCourseDto
//            {
//                Id = course.Id,
//                Title = course.Title,
//                Description = course.Description,
//                EstimatedTime = course.EstimatedTime,
//                ThumbnailUrl = course.ThumbnailImagePath != null ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : string.Empty,
//                Category = course.Category != null
//                    ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
//                    : new CategoryDto { Id = "uncategorized", Title = "Uncategorized" },
//                Technologies = course.CourseTechnologies?
//                    .Where(ct => ct.Technology != null)
//                    .Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name })
//                    .ToList() ?? new List<TechnologyDto>(),
//                Status = course.Status,
//                IsEnrolled = enrollment != null,
//                EnrollmentDate = enrollment?.EnrollmentDate,
//                EnrollmentStatus = enrollment?.Status ?? "not_enrolled",
//                ProgressPercentage = progressPercentage,
//                TotalLessons = totalContentItems,
//                CompletedLessons = completedLessonsCount,
//                Lessons = learnerLessons,
//                EnrollmentId = enrollment?.Id
//            };
//        }

//        // ======================================================================================
//        // THE MAIN FIX: Changed Task.WhenAll to sequential awaits to solve DbContext concurrency.
//        // ======================================================================================
//        public async Task<IEnumerable<LearnerCourseDto>> GetAvailableCoursesAsync(string userId, string? categoryId = null)
//        {
//            _logger.LogInformation("Getting available courses for user {UserId}, category: {CategoryId}", userId, categoryId ?? "All");

//            // Await each database call separately to prevent concurrency issues.
//            var allCourses = (await _courseRepository.GetAllPublishedCoursesWithDetailsAsync()).ToList();
//            var userEnrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);

//            var enrolledCourseIds = userEnrollments.Select(e => e.CourseId).ToHashSet();

//            var availableCourses = allCourses
//                .Where(course => !enrolledCourseIds.Contains(course.Id) &&
//                               (categoryId == null || course.CategoryId == categoryId))
//                .Select(course => MapCourseToLightweightDto(course, false))
//                .ToList();

//            _logger.LogInformation("Found {Count} available courses for user {UserId}.", availableCourses.Count, userId);
//            return availableCourses;
//        }

//        public async Task<IEnumerable<LearnerCourseDto>> GetEnrolledCoursesAsync(string userId)
//        {
//            _logger.LogInformation("Getting enrolled courses for user {UserId}", userId);

//            var enrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);

//            if (!enrollments.Any())
//            {
//                _logger.LogInformation("No enrolled courses found for user {UserId}.", userId);
//                return new List<LearnerCourseDto>();
//            }

//            var enrolledCourses = new List<LearnerCourseDto>();
//            foreach (var enrollment in enrollments)
//            {
//                if (enrollment.Course != null)
//                {
//                    var courseDto = MapCourseToLightweightDto(enrollment.Course, true, enrollment);
//                    enrolledCourses.Add(courseDto);
//                }
//                else
//                {
//                    _logger.LogWarning("Enrollment record with ID {EnrollmentId} exists for user {UserId} but has a null Course.", enrollment.Id, userId);
//                }
//            }

//            _logger.LogInformation("Found {Count} enrolled courses for user {UserId}.", enrolledCourses.Count, userId);
//            return enrolledCourses;
//        }

//        public async Task<LearnerCourseDto?> GetLearnerCourseDetailsAsync(string userId, int courseId)
//        {
//            _logger.LogInformation("Getting learner course details for user {UserId} and course {CourseId}", userId, courseId);

//            // Await each database call separately.
//            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
//            if (course == null)
//            {
//                _logger.LogWarning("Course {CourseId} not found.", courseId);
//                return null;
//            }

//            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, courseId);

//            return await MapCourseToDetailedDto(course, userId, enrollment);
//        }

//        public async Task<LessonProgressDto> MarkLessonCompletedAsync(string userId, int lessonId)
//        {
//            _logger.LogInformation("Attempting to mark lesson {LessonId} as completed for user {UserId}", lessonId, userId);

//            // Await each database call separately.
//            var lesson = await _courseRepository.GetLessonWithDocumentsAsync(lessonId);
//            if (lesson == null)
//            {
//                throw new KeyNotFoundException($"Lesson with ID {lessonId} not found.");
//            }

//            var progress = await _lessonProgressRepository.GetProgressByUserIdAndLessonIdAsync(userId, lessonId);

//            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, lesson.CourseId);
//            if (enrollment == null)
//            {
//                throw new InvalidOperationException($"User {userId} is not enrolled in the course ({lesson.CourseId}) for lesson {lessonId}.");
//            }

//            if (progress == null)
//            {
//                progress = new LessonProgress
//                {
//                    UserId = userId,
//                    LessonId = lessonId,
//                    IsCompleted = true,
//                    CompletionDate = DateTime.UtcNow
//                };
//                await _lessonProgressRepository.AddAsync(progress);
//                _logger.LogInformation("Created new progress record for lesson {LessonId} for user {UserId}.", lessonId, userId);
//            }
//            else if (!progress.IsCompleted)
//            {
//                progress.IsCompleted = true;
//                progress.CompletionDate = DateTime.UtcNow;
//                await _lessonProgressRepository.UpdateAsync(progress);
//                _logger.LogInformation("Updated progress record for lesson {LessonId} for user {UserId}.", lessonId, userId);
//            }
//            else
//            {
//                _logger.LogInformation("Lesson {LessonId} already marked as completed for user {UserId}.", lessonId, userId);
//            }

//            return new LessonProgressDto
//            {
//                Id = progress.Id,
//                UserId = progress.UserId,
//                LessonId = progress.LessonId,
//                LessonName = lesson.LessonName,
//                IsCompleted = progress.IsCompleted,
//                CompletionDate = progress.CompletionDate
//            };
//        }

//        public async Task<bool> HasLearnerCompletedAllCourseContentAsync(string userId, int courseId)
//        {
//            _logger.LogInformation("Checking if user {UserId} has completed all content for course {CourseId}", userId, courseId);

//            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
//            if (course == null)
//            {
//                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
//            }

//            var lessons = course.Lessons.ToList();
//            if (!lessons.Any())
//            {
//                _logger.LogInformation("Course {CourseId} has no lessons.", courseId);
//                return true;
//            }

//            var progresses = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdAsync(userId, courseId)).ToList();
//            var quizDtos = (await _quizService.GetQuizzesByCourseIdAsync(courseId)).ToList();

//            var quizByLessonId = quizDtos.ToDictionary(q => q.LessonId, q => q);

//            var quizIds = quizDtos.Select(q => q.QuizId).ToList();
//            var quizCompletionTasks = quizIds.Select(async quizId =>
//                new { QuizId = quizId, IsCompleted = await _quizAttemptService.HasUserCompletedQuizAsync(userId, quizId) }
//            );
//            var quizCompletions = await Task.WhenAll(quizCompletionTasks);
//            var quizCompletionLookup = quizCompletions.ToDictionary(qc => qc.QuizId, qc => qc.IsCompleted);

//            foreach (var lesson in lessons)
//            {
//                var progress = progresses.FirstOrDefault(lp => lp.LessonId == lesson.Id);
//                bool lessonMarkedCompleted = progress?.IsCompleted ?? false;

//                bool quizIsCompleted = true;
//                if (quizByLessonId.TryGetValue(lesson.Id, out var quizDto))
//                {
//                    quizIsCompleted = quizCompletionLookup.TryGetValue(quizDto.QuizId, out var completed) && completed;
//                }

//                if (!lessonMarkedCompleted || !quizIsCompleted)
//                {
//                    _logger.LogDebug("User {UserId} has not completed lesson {LessonId} (lesson: {LessonCompleted}, quiz: {QuizCompleted}).",
//                        userId, lesson.Id, lessonMarkedCompleted, quizIsCompleted);
//                    return false;
//                }
//            }

//            _logger.LogInformation("User {UserId} has completed all content for course {CourseId}.", userId, courseId);
//            return true;
//        }
//    }
//}

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
        private readonly IQuizAttemptService _quizAttemptService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<LearnerCourseService> _logger;
        // ADDED: Repository to get full attempt details
        private readonly IQuizAttemptRepository _quizAttemptRepository;

        public LearnerCourseService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ILessonProgressRepository lessonProgressRepository,
            IQuizService quizService,
            IQuizAttemptService quizAttemptService,
            IFileStorageService fileStorageService,
            ILogger<LearnerCourseService> logger,
            IQuizAttemptRepository quizAttemptRepository) // MODIFIED: Inject repository
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _lessonProgressRepository = lessonProgressRepository;
            _quizService = quizService;
            _quizAttemptService = quizAttemptService;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _quizAttemptRepository = quizAttemptRepository; // MODIFIED: Assign repository
        }

        private LearnerCourseDto MapCourseToLightweightDto(Core.Entities.Course.Course course, bool isEnrolled, Enrollment? enrollment = null)
        {
            // This method remains unchanged.
            return new LearnerCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                EstimatedTime = course.EstimatedTime,
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
                ProgressPercentage = 0,
                TotalLessons = 0,
                CompletedLessons = 0,
                Lessons = new List<LearnerLessonDto>()
            };
        }

        private async Task<LearnerCourseDto> MapCourseToDetailedDto(Core.Entities.Course.Course course, string userId, Enrollment? enrollment = null)
        {
            var courseId = course.Id;

            var lessons = (await _courseRepository.GetLessonsByCourseIdAsync(courseId)).ToList();
            var progresses = (await _lessonProgressRepository.GetProgressByUserIdAndCourseIdAsync(userId, courseId)).ToList();
            var quizDtos = (await _quizService.GetQuizzesByCourseIdAsync(courseId)).ToList();
            var quizByLessonId = quizDtos.ToDictionary(q => q.LessonId, q => q);

            // --- START OF CRITICAL FIX ---
            // Instead of just getting a boolean, we now get the full attempt object to find its ID.
            var lastAttemptLookup = new Dictionary<int, QuizAttempt>();
            var quizIds = quizDtos.Select(q => q.QuizId).ToList();

            // Loop sequentially to avoid DbContext concurrency issues
            foreach (var quizId in quizIds)
            {
                var completedAttempts = await _quizAttemptRepository.GetCompletedAttemptsByUserAndQuizAsync(userId, quizId);
                // Find the most recent completed attempt
                var lastCompletedAttempt = completedAttempts.OrderByDescending(a => a.CompletionTime).FirstOrDefault();
                if (lastCompletedAttempt != null)
                {
                    lastAttemptLookup[quizId] = lastCompletedAttempt;
                }
            }
            // --- END OF CRITICAL FIX ---

            var learnerLessons = new List<LearnerLessonDto>();
            foreach (var lesson in lessons.OrderBy(l => l.Id))
            {
                var progress = progresses.FirstOrDefault(lp => lp.LessonId == lesson.Id);
                var hasQuiz = quizByLessonId.TryGetValue(lesson.Id, out var quizDto);

                // --- MODIFIED LOGIC TO POPULATE DTO ---
                bool isQuizCompleted = false;
                int? lastAttemptId = null;

                if (hasQuiz && quizDto != null)
                {
                    if (lastAttemptLookup.TryGetValue(quizDto.QuizId, out var lastAttempt))
                    {
                        isQuizCompleted = true;
                        lastAttemptId = lastAttempt.QuizAttemptId; // Assign the ID
                    }
                }

                learnerLessons.Add(new LearnerLessonDto
                {
                    Id = lesson.Id,
                    LessonName = lesson.LessonName,
                    LessonPoints = lesson.LessonPoints,
                    LastUpdatedDate = lesson.LastUpdatedDate,
                    Documents = lesson.Documents?.Select(d => new CourseDocumentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        DocumentType = d.DocumentType,
                        FileSize = d.FileSize,
                        FileUrl = d.FilePath != null ? _fileStorageService.GetFileUrl(d.FilePath) : string.Empty,
                        LastUpdatedDate = d.LastUpdatedDate,
                        LessonId = d.LessonId
                    }).ToList() ?? new List<CourseDocumentDto>(),
                    IsCompleted = progress?.IsCompleted ?? false,
                    HasQuiz = hasQuiz,
                    QuizId = quizDto?.QuizId,
                    IsQuizCompleted = isQuizCompleted, // Set the flag
                    LastAttemptId = lastAttemptId      // SET THE ID
                });
            }

            int completedLessonsCount = learnerLessons.Count(l => l.IsCompleted && (!l.HasQuiz || l.IsQuizCompleted));
            int totalContentItems = learnerLessons.Count;
            int progressPercentage = totalContentItems > 0 ? (int)Math.Round((double)completedLessonsCount / totalContentItems * 100) : 0;

            return new LearnerCourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                EstimatedTime = course.EstimatedTime,
                ThumbnailUrl = course.ThumbnailImagePath != null ? _fileStorageService.GetFileUrl(course.ThumbnailImagePath) : string.Empty,
                Category = course.Category != null
                    ? new CategoryDto { Id = course.Category.Id, Title = course.Category.Title }
                    : new CategoryDto { Id = "uncategorized", Title = "Uncategorized" },
                Technologies = course.CourseTechnologies?
                    .Where(ct => ct.Technology != null)
                    .Select(ct => new TechnologyDto { Id = ct.Technology.Id, Name = ct.Technology.Name })
                    .ToList() ?? new List<TechnologyDto>(),
                Status = course.Status,
                IsEnrolled = enrollment != null,
                EnrollmentDate = enrollment?.EnrollmentDate,
                EnrollmentStatus = enrollment?.Status ?? "not_enrolled",
                ProgressPercentage = progressPercentage,
                TotalLessons = totalContentItems,
                CompletedLessons = completedLessonsCount,
                Lessons = learnerLessons,
                EnrollmentId = enrollment?.Id
            };
        }

        // --- Other methods in the service remain unchanged ---

        public async Task<IEnumerable<LearnerCourseDto>> GetAvailableCoursesAsync(string userId, string? categoryId = null)
        {
            _logger.LogInformation("Getting available courses for user {UserId}, category: {CategoryId}", userId, categoryId ?? "All");
            var allCourses = (await _courseRepository.GetAllPublishedCoursesWithDetailsAsync()).ToList();
            var userEnrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);
            var enrolledCourseIds = userEnrollments.Select(e => e.CourseId).ToHashSet();
            var availableCourses = allCourses
                .Where(course => !enrolledCourseIds.Contains(course.Id) &&
                               (categoryId == null || course.CategoryId == categoryId))
                .Select(course => MapCourseToLightweightDto(course, false))
                .ToList();
            _logger.LogInformation("Found {Count} available courses for user {UserId}.", availableCourses.Count, userId);
            return availableCourses;
        }

        public async Task<IEnumerable<LearnerCourseDto>> GetEnrolledCoursesAsync(string userId)
        {
            _logger.LogInformation("Getting enrolled courses for user {UserId}", userId);
            var enrollments = await _enrollmentRepository.GetEnrollmentsByUserIdAsync(userId);
            if (!enrollments.Any())
            {
                _logger.LogInformation("No enrolled courses found for user {UserId}.", userId);
                return new List<LearnerCourseDto>();
            }
            var enrolledCourses = new List<LearnerCourseDto>();
            foreach (var enrollment in enrollments)
            {
                if (enrollment.Course != null)
                {
                    var courseDto = MapCourseToLightweightDto(enrollment.Course, true, enrollment);
                    enrolledCourses.Add(courseDto);
                }
                else
                {
                    _logger.LogWarning("Enrollment record with ID {EnrollmentId} exists for user {UserId} but has a null Course.", enrollment.Id, userId);
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
            return await MapCourseToDetailedDto(course, userId, enrollment);
        }

        public async Task<LessonProgressDto> MarkLessonCompletedAsync(string userId, int lessonId)
        {
            _logger.LogInformation("Attempting to mark lesson {LessonId} as completed for user {UserId}", lessonId, userId);
            var lesson = await _courseRepository.GetLessonWithDocumentsAsync(lessonId);
            if (lesson == null)
            {
                throw new KeyNotFoundException($"Lesson with ID {lessonId} not found.");
            }
            var progress = await _lessonProgressRepository.GetProgressByUserIdAndLessonIdAsync(userId, lessonId);
            var enrollment = await _enrollmentRepository.GetEnrollmentByUserIdAndCourseIdAsync(userId, lesson.CourseId);
            if (enrollment == null)
            {
                throw new InvalidOperationException($"User {userId} is not enrolled in the course ({lesson.CourseId}) for lesson {lessonId}.");
            }
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
            // This method could also be updated to use the new logic, but the current fix is in MapCourseToDetailedDto
            _logger.LogInformation("Checking if user {UserId} has completed all content for course {CourseId}", userId, courseId);
            var course = await _courseRepository.GetByIdWithDetailsAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException($"Course with ID {courseId} not found.");
            }
            var detailedDto = await MapCourseToDetailedDto(course, userId);
            return detailedDto.ProgressPercentage >= 100;
        }
    }
}