using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using ExcellyGenLMS.Core.Entities.Course;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCourseCategoryDto = ExcellyGenLMS.Application.DTOs.Course.CourseCategoryAnalyticsDto;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class CourseCoordinatorAnalyticsService : ICourseCoordinatorAnalyticsService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly ILessonRepository _lessonRepository;

        public CourseCoordinatorAnalyticsService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IQuizRepository quizRepository,
            IQuizAttemptRepository quizAttemptRepository,
            ILessonRepository lessonRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _quizRepository = quizRepository;
            _quizAttemptRepository = quizAttemptRepository;
            _lessonRepository = lessonRepository;
        }

        public async Task<List<ApplicationCourseCategoryDto>> GetCourseCategoriesAsync(string coordinatorId)
        {
            var coreDtos = await _courseRepository.GetCourseCategoryAnalyticsAsync(coordinatorId);

            return coreDtos.Select(dto => new ApplicationCourseCategoryDto
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                TotalCourses = dto.TotalCourses,
                TotalEnrollments = dto.TotalEnrollments
            }).ToList();
        }

        public async Task<DTOs.Course.EnrollmentAnalyticsResponse> GetEnrollmentAnalyticsAsync(string coordinatorId, string? categoryId, string status, string ownership)
        {
            var creatorIdFilter = ownership.Equals("mine", StringComparison.OrdinalIgnoreCase) ? coordinatorId : null;

            var courses = (await _courseRepository.GetCoursesByCreatorIdAsync(creatorIdFilter, categoryId)).ToList();
            var courseIds = courses.Select(c => c.Id).ToList();

            var categories = await GetCourseCategoriesAsync(coordinatorId);

            if (!courseIds.Any())
            {
                return new DTOs.Course.EnrollmentAnalyticsResponse { Categories = categories, Enrollments = new List<DTOs.Course.EnrollmentAnalyticsDto>() };
            }

            var allEnrollmentsForCourses = (await _enrollmentRepository.GetEnrollmentsByCourseIdsAsync(courseIds)).ToList();
            var enrollmentsByCourseId = allEnrollmentsForCourses.GroupBy(e => e.CourseId).ToDictionary(g => g.Key, g => g.ToList());

            var enrollmentsDtoList = new List<DTOs.Course.EnrollmentAnalyticsDto>();
            foreach (var course in courses)
            {
                var enrollmentsForCourse = enrollmentsByCourseId.GetValueOrDefault(course.Id, new List<Enrollment>());

                var ongoingCount = enrollmentsForCourse.Count(e => e.CompletionDate == null);
                var completedCount = enrollmentsForCourse.Count(e => e.CompletionDate != null);

                var enrollmentDto = new DTOs.Course.EnrollmentAnalyticsDto
                {
                    CourseId = course.Id,
                    Course = course.Title,
                    ShortCourseName = TruncateName(course.Title),
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Title ?? "Unknown",
                    TotalEnrollments = enrollmentsForCourse.Count,
                    OngoingEnrollments = ongoingCount,
                    CompletedEnrollments = completedCount,
                    CoordinatorId = course.CreatorId,
                    CoordinatorName = course.Creator?.Name ?? "Unknown"
                };

                if (status.Equals("ongoing", StringComparison.OrdinalIgnoreCase) && enrollmentDto.OngoingEnrollments > 0)
                {
                    enrollmentsDtoList.Add(enrollmentDto);
                }
                else if (status.Equals("completed", StringComparison.OrdinalIgnoreCase) && enrollmentDto.CompletedEnrollments > 0)
                {
                    enrollmentsDtoList.Add(enrollmentDto);
                }
                else if (status.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    enrollmentsDtoList.Add(enrollmentDto);
                }
            }

            var totalStats = new DTOs.Course.EnrollmentStatsDto
            {
                TotalCourses = enrollmentsDtoList.Count,
                TotalEnrollments = enrollmentsDtoList.Sum(e => status.Equals("ongoing", StringComparison.OrdinalIgnoreCase) ? e.OngoingEnrollments : status.Equals("completed", StringComparison.OrdinalIgnoreCase) ? e.CompletedEnrollments : e.TotalEnrollments),
                TotalOngoing = enrollmentsDtoList.Sum(e => e.OngoingEnrollments),
                TotalCompleted = enrollmentsDtoList.Sum(e => e.CompletedEnrollments)
            };

            return new DTOs.Course.EnrollmentAnalyticsResponse
            {
                Enrollments = enrollmentsDtoList,
                Categories = categories,
                TotalStats = totalStats
            };
        }

        public async Task<List<DTOs.Course.CoordinatorCourseAnalyticsDto>> GetCoordinatorCoursesAsync(string coordinatorId, string? categoryId, string ownership)
        {
            var creatorIdFilter = ownership.Equals("mine", StringComparison.OrdinalIgnoreCase) ? coordinatorId : null;
            var courses = (await _courseRepository.GetCoursesByCreatorIdAsync(creatorIdFilter, categoryId)).ToList();
            var courseIds = courses.Select(c => c.Id).ToList();

            if (!courseIds.Any()) return new List<DTOs.Course.CoordinatorCourseAnalyticsDto>();

            var enrollments = (await _enrollmentRepository.GetEnrollmentsByCourseIdsAsync(courseIds)).ToList();
            var enrollmentsByCourseId = enrollments.GroupBy(e => e.CourseId).ToDictionary(g => g.Key, g => g.ToList());

            return courses.Select(course =>
            {
                var enrollmentsForCourse = enrollmentsByCourseId.GetValueOrDefault(course.Id, new List<Enrollment>());
                return new DTOs.Course.CoordinatorCourseAnalyticsDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    ShortTitle = TruncateName(course.Title),
                    CategoryId = course.CategoryId,
                    CategoryName = course.Category?.Title ?? "Unknown",
                    TotalEnrollments = enrollmentsForCourse.Count,
                    OngoingEnrollments = enrollmentsForCourse.Count(e => e.CompletionDate == null),
                    CompletedEnrollments = enrollmentsForCourse.Count(e => e.CompletionDate != null),
                    IsCreatedByCurrentCoordinator = course.CreatorId == coordinatorId
                };
            }).ToList();
        }

        public async Task<List<DTOs.Course.CourseQuizAnalyticsDto>> GetQuizzesForCourseAsync(int courseId, string coordinatorId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) // A course coordinator can view quizzes for any course. Ownership is checked on performance.
            {
                return new List<DTOs.Course.CourseQuizAnalyticsDto>();
            }

            var quizzes = (await _quizRepository.GetQuizzesByCourseIdAsync(courseId)).ToList();
            if (!quizzes.Any()) return new List<DTOs.Course.CourseQuizAnalyticsDto>();

            var quizIds = quizzes.Select(q => q.QuizId).ToList();
            var allAttempts = (await _quizAttemptRepository.GetCompletedAttemptsByQuizIdsAsync(quizIds)).ToList();
            var attemptsByQuizId = allAttempts.GroupBy(a => a.QuizId).ToDictionary(g => g.Key, g => g.ToList());

            return quizzes.Select(quiz =>
            {
                var attempts = attemptsByQuizId.GetValueOrDefault(quiz.QuizId, new List<QuizAttempt>());
                var averageScore = attempts.Any() && quiz.TotalMarks > 0
                    ? (attempts.Average(a => a.Score ?? 0) / (double)quiz.TotalMarks) * 100
                    : 0;

                return new DTOs.Course.CourseQuizAnalyticsDto
                {
                    QuizId = quiz.QuizId,
                    QuizTitle = quiz.QuizTitle,
                    CourseId = courseId,
                    CourseTitle = course.Title,
                    TotalAttempts = attempts.Count,
                    AverageScore = (decimal)averageScore,
                    IsCreatedByCurrentCoordinator = course.CreatorId == coordinatorId
                };
            }).ToList();
        }

        public async Task<DTOs.Course.QuizPerformanceAnalyticsResponse> GetQuizPerformanceAsync(int quizId, string coordinatorId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            // Any coordinator can see any quiz performance, so no ownership check here.
            if (quiz?.Lesson?.Course == null)
            {
                throw new KeyNotFoundException("Quiz or its associated course not found.");
            }

            var attempts = (await _quizAttemptRepository.GetCompletedAttemptsByQuizIdAsync(quizId))
                           .Where(a => a.Score.HasValue).ToList();

            var totalValidAttempts = attempts.Count;
            if (totalValidAttempts == 0) return new DTOs.Course.QuizPerformanceAnalyticsResponse();

            var performanceData = GetMarkRangeAnalytics(attempts, quiz.TotalMarks);

            double averageScore = quiz.TotalMarks > 0
                ? (attempts.Average(a => a.Score!.Value) / (double)quiz.TotalMarks) * 100
                : 0;

            var passMark = 60;
            var passCount = attempts.Count(a => (a.Score!.Value / (double)quiz.TotalMarks) * 100 >= passMark);
            var passRate = totalValidAttempts > 0 ? (decimal)passCount / totalValidAttempts * 100 : 0;

            return new DTOs.Course.QuizPerformanceAnalyticsResponse
            {
                PerformanceData = performanceData,
                QuizStats = new DTOs.Course.QuizStatsDto
                {
                    TotalAttempts = totalValidAttempts,
                    AverageScore = (decimal)averageScore,
                    PassRate = passRate
                }
            };
        }

        private static string TruncateName(string? name, int maxLength = 30)
        {
            if (string.IsNullOrEmpty(name)) return "N/A";
            return name.Length <= maxLength ? name : name.Substring(0, maxLength - 3) + "...";
        }

        private List<DTOs.Course.MarkRangeAnalyticsDto> GetMarkRangeAnalytics(List<QuizAttempt> attempts, int totalMarks)
        {
            var ranges = new List<DTOs.Course.MarkRangeAnalyticsDto>
            {
                new() { Range = "0-20", MinMark = 0, MaxMark = 20 },
                new() { Range = "21-40", MinMark = 21, MaxMark = 40 },
                new() { Range = "41-60", MinMark = 41, MaxMark = 60 },
                new() { Range = "61-80", MinMark = 61, MaxMark = 80 },
                new() { Range = "81-100", MinMark = 81, MaxMark = 100 }
            };

            if (!attempts.Any()) return ranges;

            var validTotalMarks = totalMarks > 0 ? totalMarks : 100;
            foreach (var attempt in attempts)
            {
                var normalizedScore = (attempt.Score!.Value / (double)validTotalMarks) * 100;
                if (normalizedScore <= 20) ranges[0].Count++;
                else if (normalizedScore <= 40) ranges[1].Count++;
                else if (normalizedScore <= 60) ranges[2].Count++;
                else if (normalizedScore <= 80) ranges[3].Count++;
                else ranges[4].Count++;
            }

            var totalAttempts = attempts.Count;
            ranges.ForEach(range => range.Percentage = totalAttempts > 0 ? (decimal)range.Count / totalAttempts * 100 : 0);
            return ranges;
        }
    }
}