// ExcellyGenLMS.Application/Services/Course/CourseCoordinatorAnalyticsService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Admin;
using ExcellyGenLMS.Core.Entities.Course;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class CourseCoordinatorAnalyticsService : ICourseCoordinatorAnalyticsService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseCategoryRepository _courseCategoryRepository;

        public CourseCoordinatorAnalyticsService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IQuizRepository quizRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ILessonRepository lessonRepository,
            ICourseCategoryRepository courseCategoryRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _quizRepository = quizRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _lessonRepository = lessonRepository;
            _courseCategoryRepository = courseCategoryRepository;
        }

        #region Enhanced Methods (Main Implementation)

        public async Task<EnrollmentAnalyticsResponse> GetEnrollmentAnalyticsAsync(string coordinatorId, string? categoryId = null, string status = "all")
        {
            // Get all courses for the coordinator
            var allCourses = await _courseRepository.GetAllAsync();
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId);

            // Apply category filter if specified
            if (!string.IsNullOrEmpty(categoryId))
            {
                coordinatorCourses = coordinatorCourses.Where(c => c.CategoryId == categoryId);
            }

            var coursesList = coordinatorCourses.ToList();
            var enrollmentsList = new List<EnrollmentAnalyticsDto>();

            // Process each course
            foreach (var course in coursesList)
            {
                var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.Id);
                var enrollmentList = enrollments.ToList();

                // Calculate enrollment counts
                var totalEnrollments = enrollmentList.Count;
                var ongoingEnrollments = enrollmentList.Count(e =>
                    e.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
                    e.Status.Equals("ongoing", StringComparison.OrdinalIgnoreCase) ||
                    e.Status.Equals("inprogress", StringComparison.OrdinalIgnoreCase));
                var completedEnrollments = enrollmentList.Count(e =>
                    e.Status.Equals("completed", StringComparison.OrdinalIgnoreCase));

                // Create enrollment analytics DTO
                var enrollmentDto = new EnrollmentAnalyticsDto
                {
                    CourseId = course.Id,
                    Course = course.Title,
                    ShortCourseName = TruncateCourseName(course.Title),
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Title ?? "Unknown",
                    TotalEnrollments = totalEnrollments,
                    OngoingEnrollments = ongoingEnrollments,
                    CompletedEnrollments = completedEnrollments,
                    CoordinatorId = course.CreatorId,
                    CoordinatorName = course.Creator?.Name ?? "Unknown"
                };

                enrollmentsList.Add(enrollmentDto);
            }

            // Apply status filter for display
            var filteredEnrollments = FilterEnrollmentsByStatus(enrollmentsList, status);

            // Get categories
            var categories = await GetCourseCategoriesAsync(coordinatorId);

            // Calculate total stats
            var totalStats = new EnrollmentStatsDto
            {
                TotalCourses = enrollmentsList.Count,
                TotalEnrollments = enrollmentsList.Sum(e => e.TotalEnrollments),
                TotalOngoing = enrollmentsList.Sum(e => e.OngoingEnrollments),
                TotalCompleted = enrollmentsList.Sum(e => e.CompletedEnrollments)
            };

            return new EnrollmentAnalyticsResponse
            {
                Enrollments = filteredEnrollments,
                Categories = categories,
                TotalStats = totalStats
            };
        }

        public async Task<List<CourseCategoryAnalyticsDto>> GetCourseCategoriesAsync(string coordinatorId)
        {
            var allCategories = await _courseCategoryRepository.GetAllCategoriesAsync();
            var allCourses = await _courseRepository.GetAllAsync();
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId).ToList();

            var categoryAnalytics = new List<CourseCategoryAnalyticsDto>();

            foreach (var category in allCategories.Where(c => c.Status == "active"))
            {
                var coursesInCategory = coordinatorCourses.Where(c => c.CategoryId == category.Id).ToList();

                if (coursesInCategory.Any()) // Only include categories with coordinator's courses
                {
                    var totalEnrollments = 0;
                    foreach (var course in coursesInCategory)
                    {
                        var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.Id);
                        totalEnrollments += enrollments.Count();
                    }

                    categoryAnalytics.Add(new CourseCategoryAnalyticsDto
                    {
                        Id = category.Id,
                        Name = category.Title,
                        Description = category.Description,
                        TotalCourses = coursesInCategory.Count,
                        TotalEnrollments = totalEnrollments
                    });
                }
            }

            return categoryAnalytics;
        }

        public async Task<List<CoordinatorCourseAnalyticsDto>> GetCoordinatorCoursesAsync(string coordinatorId, string? categoryId = null)
        {
            var allCourses = await _courseRepository.GetAllAsync();
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId);

            // Apply category filter if specified
            if (!string.IsNullOrEmpty(categoryId))
            {
                coordinatorCourses = coordinatorCourses.Where(c => c.CategoryId == categoryId);
            }

            var courseAnalytics = new List<CoordinatorCourseAnalyticsDto>();

            foreach (var course in coordinatorCourses)
            {
                var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.Id);
                var enrollmentList = enrollments.ToList();

                var totalEnrollments = enrollmentList.Count;
                var ongoingEnrollments = enrollmentList.Count(e =>
                    e.Status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
                    e.Status.Equals("ongoing", StringComparison.OrdinalIgnoreCase) ||
                    e.Status.Equals("inprogress", StringComparison.OrdinalIgnoreCase));
                var completedEnrollments = enrollmentList.Count(e =>
                    e.Status.Equals("completed", StringComparison.OrdinalIgnoreCase));

                courseAnalytics.Add(new CoordinatorCourseAnalyticsDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    ShortTitle = TruncateCourseName(course.Title),
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Title ?? "Unknown",
                    TotalEnrollments = totalEnrollments,
                    OngoingEnrollments = ongoingEnrollments,
                    CompletedEnrollments = completedEnrollments,
                    IsCreatedByCurrentCoordinator = true
                });
            }

            return courseAnalytics;
        }

        public async Task<List<CourseQuizAnalyticsDto>> GetQuizzesForCourseAsync(int courseId, string coordinatorId)
        {
            // Verify the course belongs to the coordinator
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null || course.CreatorId != coordinatorId)
            {
                return new List<CourseQuizAnalyticsDto>();
            }

            // Get lessons for the course
            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            var lessonIds = lessons.Select(l => l.Id).ToList();

            if (!lessonIds.Any())
            {
                return new List<CourseQuizAnalyticsDto>();
            }

            // Get quizzes for all lessons in the course
            var quizzes = await _quizRepository.GetQuizzesByLessonIdsAsync(lessonIds);
            var quizAnalytics = new List<CourseQuizAnalyticsDto>();

            foreach (var quiz in quizzes)
            {
                var attempts = await _quizAttemptRepository.GetAttemptsByQuizIdAsync(quiz.QuizId);
                var attemptsList = attempts.ToList();

                var totalAttempts = attemptsList.Count;
                var averageScore = attemptsList.Any() && attemptsList.Any(a => a.Score.HasValue)
                    ? attemptsList.Where(a => a.Score.HasValue).Average(a => a.Score!.Value)
                    : 0;

                quizAnalytics.Add(new CourseQuizAnalyticsDto
                {
                    QuizId = quiz.QuizId,
                    QuizTitle = quiz.QuizTitle,
                    CourseId = courseId,
                    CourseTitle = course.Title,
                    TotalAttempts = totalAttempts,
                    AverageScore = (decimal)averageScore,
                    IsCreatedByCurrentCoordinator = true
                });
            }

            return quizAnalytics;
        }

        public async Task<QuizPerformanceAnalyticsResponse> GetQuizPerformanceAsync(int quizId, string coordinatorId)
        {
            // Get quiz and verify ownership
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
            {
                throw new UnauthorizedAccessException("Quiz not found.");
            }

            // Get lesson and course to verify coordinator ownership
            var lesson = await _lessonRepository.GetByIdAsync(quiz.LessonId);
            if (lesson == null)
            {
                throw new UnauthorizedAccessException("Lesson not found.");
            }

            var course = await _courseRepository.GetByIdAsync(lesson.CourseId);
            if (course == null || course.CreatorId != coordinatorId)
            {
                throw new UnauthorizedAccessException("You don't have permission to view this quiz performance.");
            }

            // Get quiz attempts
            var attempts = await _quizAttemptRepository.GetAttemptsByQuizIdAsync(quizId);
            var completedAttempts = attempts.Where(a => a.IsCompleted && a.Score.HasValue).ToList();

            var totalAttempts = completedAttempts.Count;

            // Create mark ranges with proper intervals
            var markRanges = GetMarkRangeAnalytics(completedAttempts, quiz.TotalMarks);

            // Calculate quiz statistics
            var averageScore = completedAttempts.Any()
                ? completedAttempts.Average(a => a.Score!.Value)
                : 0;

            var normalizedAverage = quiz.TotalMarks > 0
                ? (averageScore / quiz.TotalMarks) * 100
                : 0;

            var passCount = completedAttempts.Count(a => {
                var normalizedScore = quiz.TotalMarks > 0 ? (a.Score!.Value / quiz.TotalMarks) * 100 : 0;
                return normalizedScore >= 60;
            });

            var passRate = totalAttempts > 0 ? (decimal)passCount / totalAttempts * 100 : 0;

            var quizStats = new QuizStatsDto
            {
                TotalAttempts = totalAttempts,
                AverageScore = (decimal)normalizedAverage,
                PassRate = passRate
            };

            return new QuizPerformanceAnalyticsResponse
            {
                PerformanceData = markRanges,
                QuizStats = quizStats
            };
        }

        #endregion

        #region Original Methods (Backward Compatibility)

        public async Task<IEnumerable<CourseEnrollmentAnalyticsDto>> GetEnrollmentAnalyticsSimpleAsync(string coordinatorId)
        {
            var allCourses = await _courseRepository.GetAllAsync();
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId).ToList();
            var analyticsData = new List<CourseEnrollmentAnalyticsDto>();

            foreach (var course in coordinatorCourses)
            {
                var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.Id);
                var enrollmentCount = enrollments.Count(e => e.Status.Equals("active", StringComparison.OrdinalIgnoreCase));

                analyticsData.Add(new CourseEnrollmentAnalyticsDto
                {
                    course = course.Title,
                    count = enrollmentCount
                });
            }
            return analyticsData;
        }

        public async Task<IEnumerable<CoordinatorCourseDto>> GetCoordinatorCoursesSimpleAsync(string coordinatorId)
        {
            var allCourses = await _courseRepository.GetAllAsync();
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId);

            return coordinatorCourses.Select(c => new CoordinatorCourseDto
            {
                CourseId = c.Id,
                CourseTitle = c.Title
            }).ToList();
        }

        public async Task<IEnumerable<CourseQuizDto>> GetQuizzesForCourseSimpleAsync(int courseId, string coordinatorId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null || course.CreatorId != coordinatorId)
            {
                return Enumerable.Empty<CourseQuizDto>();
            }

            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            var lessonIds = lessons.Select(l => l.Id).ToList();

            if (!lessonIds.Any())
            {
                return Enumerable.Empty<CourseQuizDto>();
            }

            var quizzes = await _quizRepository.GetQuizzesByLessonIdsAsync(lessonIds);

            return quizzes.Select(q => new CourseQuizDto
            {
                QuizId = q.QuizId,
                QuizTitle = q.QuizTitle
            }).ToList();
        }

        public async Task<IEnumerable<MarkRangeDataDto>> GetQuizPerformanceSimpleAsync(int quizId, string coordinatorId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null) return Enumerable.Empty<MarkRangeDataDto>();

            var lesson = await _lessonRepository.GetByIdAsync(quiz.LessonId);
            if (lesson == null) return Enumerable.Empty<MarkRangeDataDto>();

            var course = await _courseRepository.GetByIdAsync(lesson.CourseId);
            if (course == null || course.CreatorId != coordinatorId)
            {
                return Enumerable.Empty<MarkRangeDataDto>();
            }

            var attempts = await _quizAttemptRepository.GetAttemptsByQuizIdAsync(quizId);
            var completedAttempts = attempts.Where(a => a.IsCompleted && a.Score.HasValue).ToList();

            if (!completedAttempts.Any()) return Enumerable.Empty<MarkRangeDataDto>();

            var ranges = new List<MarkRangeDataDto>
            {
                new MarkRangeDataDto { range = "0-20", count = 0 },
                new MarkRangeDataDto { range = "21-40", count = 0 },
                new MarkRangeDataDto { range = "41-60", count = 0 },
                new MarkRangeDataDto { range = "61-80", count = 0 },
                new MarkRangeDataDto { range = "81-100", count = 0 }
            };

            var totalMarks = quiz.TotalMarks > 0 ? quiz.TotalMarks : 100;

            foreach (var attempt in completedAttempts)
            {
                var score = attempt.Score!.Value;
                double normalizedScore = ((double)score / totalMarks) * 100;

                if (normalizedScore <= 20) ranges[0].count++;
                else if (normalizedScore <= 40) ranges[1].count++;
                else if (normalizedScore <= 60) ranges[2].count++;
                else if (normalizedScore <= 80) ranges[3].count++;
                else if (normalizedScore <= 100) ranges[4].count++;
            }

            return ranges;
        }

        #endregion

        #region Helper Methods

        private static string TruncateCourseName(string courseName, int maxLength = 25)
        {
            if (courseName.Length <= maxLength)
                return courseName;

            return courseName.Substring(0, maxLength - 3) + "...";
        }

        private List<EnrollmentAnalyticsDto> FilterEnrollmentsByStatus(List<EnrollmentAnalyticsDto> enrollments, string status)
        {
            return status.ToLower() switch
            {
                "ongoing" => enrollments.Where(e => e.OngoingEnrollments > 0).ToList(),
                "completed" => enrollments.Where(e => e.CompletedEnrollments > 0).ToList(),
                _ => enrollments // "all" or any other value
            };
        }

        private List<MarkRangeAnalyticsDto> GetMarkRangeAnalytics(List<QuizAttempt> attempts, int totalMarks)
        {
            var ranges = new List<MarkRangeAnalyticsDto>
            {
                new MarkRangeAnalyticsDto { Range = "0-20", MinMark = 0, MaxMark = 20, Count = 0, Percentage = 0 },
                new MarkRangeAnalyticsDto { Range = "21-40", MinMark = 21, MaxMark = 40, Count = 0, Percentage = 0 },
                new MarkRangeAnalyticsDto { Range = "41-60", MinMark = 41, MaxMark = 60, Count = 0, Percentage = 0 },
                new MarkRangeAnalyticsDto { Range = "61-80", MinMark = 61, MaxMark = 80, Count = 0, Percentage = 0 },
                new MarkRangeAnalyticsDto { Range = "81-100", MinMark = 81, MaxMark = 100, Count = 0, Percentage = 0 }
            };

            if (!attempts.Any()) return ranges;

            var validTotalMarks = totalMarks > 0 ? totalMarks : 100;

            foreach (var attempt in attempts)
            {
                var score = attempt.Score!.Value;
                var normalizedScore = (score / (double)validTotalMarks) * 100;

                if (normalizedScore <= 20) ranges[0].Count++;
                else if (normalizedScore <= 40) ranges[1].Count++;
                else if (normalizedScore <= 60) ranges[2].Count++;
                else if (normalizedScore <= 80) ranges[3].Count++;
                else ranges[4].Count++;
            }

            // Calculate percentages
            var totalAttempts = attempts.Count;
            foreach (var range in ranges)
            {
                range.Percentage = totalAttempts > 0 ? (decimal)range.Count / totalAttempts * 100 : 0;
            }

            return ranges;
        }

        #endregion
    }
}