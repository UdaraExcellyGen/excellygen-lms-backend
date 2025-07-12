// OR ExcellyGenLMS.Application/Services/Course/CourseCoordinatorAnalyticsService.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course; 
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Core.Entities.Course;


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

        public async Task<IEnumerable<CourseEnrollmentAnalyticsDto>> GetEnrollmentAnalyticsAsync(string coordinatorId)
        {
        
            var allCourses = await _courseRepository.GetAllAsync(); 
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId).ToList();

            var analyticsData = new List<CourseEnrollmentAnalyticsDto>();

            foreach (var course in coordinatorCourses)
            {
              
                var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(course.Id);
                
                var enrollmentCount = enrollments.Count(e => e.Status.Equals("active", System.StringComparison.OrdinalIgnoreCase));
                analyticsData.Add(new CourseEnrollmentAnalyticsDto
                {
                    course = course.Title,
                    count = enrollmentCount
                });
            }
            return analyticsData;
        }

        public async Task<IEnumerable<CoordinatorCourseDto>> GetCoordinatorCoursesAsync(string coordinatorId)
        {
            
            var allCourses = await _courseRepository.GetAllAsync(); 
            var coordinatorCourses = allCourses.Where(c => c.CreatorId == coordinatorId);

            return coordinatorCourses.Select(c => new CoordinatorCourseDto
            {
                CourseId = c.Id,
                CourseTitle = c.Title
            }).ToList();
        }

        public async Task<IEnumerable<CourseQuizDto>> GetQuizzesForCourseAsync(int courseId, string coordinatorId)
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

        public async Task<IEnumerable<MarkRangeDataDto>> GetQuizPerformanceAsync(int quizId, string coordinatorId)
        {
            // ---  Use GetQuizByIdAsync from IQuizRepository ---
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null) return Enumerable.Empty<MarkRangeDataDto>();

            // ---  Use GetByIdAsync from ILessonRepository ---
            
            var lesson = await _lessonRepository.GetByIdAsync(quiz.LessonId);
            if (lesson == null) return Enumerable.Empty<MarkRangeDataDto>();

            // ---  Use GetByIdAsync from ICourseRepository ---
           
            var course = await _courseRepository.GetByIdAsync(lesson.CourseId);
            if (course == null || course.CreatorId != coordinatorId)
            {
                return Enumerable.Empty<MarkRangeDataDto>();
            }

            // ---  Use GetAttemptsByQuizIdAsync from IQuizAttemptRepository ---
           
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
                var score = attempt.Score!.Value; // Null-forgiving operator
                double normalizedScore = ((double)score / totalMarks) * 100;

                if (normalizedScore <= 20) ranges[0].count++;
                else if (normalizedScore <= 40) ranges[1].count++;
                else if (normalizedScore <= 60) ranges[2].count++;
                else if (normalizedScore <= 80) ranges[3].count++;
                else if (normalizedScore <= 100) ranges[4].count++;
            }

            return ranges;
        }
    }
}