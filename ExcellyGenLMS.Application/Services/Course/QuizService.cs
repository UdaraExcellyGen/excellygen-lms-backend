using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Course
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            IQuizRepository quizRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            ILogger<QuizService> logger)
        {
            _quizRepository = quizRepository ?? throw new ArgumentNullException(nameof(quizRepository));
            _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Other methods remain unchanged...

        public async Task<IEnumerable<QuizDto>> GetQuizzesByCourseIdAsync(int courseId)
        {
            try
            {
                _logger.LogInformation("Getting all quizzes for course ID: {CourseId}", courseId);
                // THIS IS THE FIX:
                // We now call the single, efficient repository method instead of looping.
                // This resolves the DbContext concurrency issue.
                var quizzes = await _quizRepository.GetQuizzesByCourseIdAsync(courseId);

                _logger.LogInformation("Found {Count} quizzes for course {CourseId}", quizzes.Count(), courseId);
                return quizzes.Select(MapQuizToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quizzes for course ID: {CourseId}", courseId);
                throw;
            }
        }

        public async Task<IEnumerable<QuizDto>> GetQuizzesByCourseIdsAsync(List<int> courseIds)
        {
            try
            {
                if (courseIds == null || !courseIds.Any())
                {
                    return new List<QuizDto>();
                }

                _logger.LogInformation("Getting all quizzes for {Count} courses.", courseIds.Count);
                var lessons = await _courseRepository.GetLessonsByCourseIdsAsync(courseIds);
                var lessonIds = lessons.Select(l => l.Id).ToList();

                var quizzes = await _quizRepository.GetQuizzesByLessonIdsAsync(lessonIds);

                _logger.LogInformation("Found {Count} quizzes for the provided courses.", quizzes.Count());
                return quizzes.Select(MapQuizToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quizzes for multiple course IDs.");
                throw;
            }
        }

        // All other methods from your provided file remain the same...
        // ... (CreateQuizAsync, UpdateQuizAsync, etc.)

        #region Unchanged Methods
        public async Task<QuizDto?> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            return quiz == null ? null : MapQuizToDto(quiz);
        }

        public async Task<QuizDetailDto?> GetQuizDetailsAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null) return null;
            var questions = await _quizRepository.GetQuestionsForQuizBankAsync(quiz.QuizBankId);
            return new QuizDetailDto
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.QuizTitle,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                TotalMarks = quiz.TotalMarks,
                QuizSize = quiz.QuizSize,
                QuizBankId = quiz.QuizBankId,
                LessonId = quiz.LessonId,
                LessonName = quiz.Lesson?.LessonName ?? "Unknown",
                Questions = questions.Select(MapQuestionToDto).ToList()
            };
        }

        public async Task<IEnumerable<QuizDto>> GetQuizzesByLessonIdAsync(int lessonId)
        {
            var quizzes = await _quizRepository.GetQuizzesByLessonIdAsync(lessonId);
            return quizzes.Select(MapQuizToDto);
        }

        public async Task<QuizDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            var lesson = await _lessonRepository.GetByIdAsync(createQuizDto.LessonId) ?? throw new ArgumentException($"Lesson with ID {createQuizDto.LessonId} not found.");
            if (await _quizRepository.GetQuizByLessonIdAsync(createQuizDto.LessonId) != null)
                throw new InvalidOperationException($"A quiz already exists for lesson {createQuizDto.LessonId}.");

            var quizBank = await _quizRepository.GetOrCreateQuizBankForLessonAsync(createQuizDto.LessonId);
            var quiz = new Quiz
            {
                QuizTitle = createQuizDto.QuizTitle,
                TimeLimitMinutes = createQuizDto.TimeLimitMinutes,
                QuizSize = createQuizDto.QuizSize,
                TotalMarks = createQuizDto.QuizSize,
                LessonId = createQuizDto.LessonId,
                QuizBankId = quizBank.QuizBankId
            };
            var createdQuiz = await _quizRepository.CreateQuizAsync(quiz);
            return MapQuizToDto(createdQuiz);
        }

        public async Task UpdateQuizAsync(int quizId, CreateQuizDto updateQuizDto)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId) ?? throw new ArgumentException($"Quiz with ID {quizId} not found.");
            if (quiz.LessonId != updateQuizDto.LessonId)
            {
                _ = await _lessonRepository.GetByIdAsync(updateQuizDto.LessonId) ?? throw new ArgumentException($"Lesson with ID {updateQuizDto.LessonId} not found.");
            }
            quiz.QuizTitle = updateQuizDto.QuizTitle;
            quiz.TimeLimitMinutes = updateQuizDto.TimeLimitMinutes;
            quiz.QuizSize = updateQuizDto.QuizSize;
            quiz.TotalMarks = updateQuizDto.QuizSize;
            quiz.LessonId = updateQuizDto.LessonId;
            await _quizRepository.UpdateQuizAsync(quiz);
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            _ = await _quizRepository.GetQuizByIdAsync(quizId) ?? throw new ArgumentException($"Quiz with ID {quizId} not found.");
            await _quizRepository.DeleteQuizAsync(quizId);
        }

        public async Task<QuizBankDto?> GetQuizBankByIdAsync(int quizBankId)
        {
            var quizBank = await _quizRepository.GetQuizBankByIdAsync(quizBankId);
            if (quizBank == null) return null;
            var questions = await _quizRepository.GetQuestionsForQuizBankAsync(quizBankId);
            return new QuizBankDto
            {
                QuizBankId = quizBank.QuizBankId,
                QuizBankSize = quizBank.QuizBankSize,
                Questions = questions.Select(MapQuestionToDto).ToList()
            };
        }

        public async Task<QuizBankDto> CreateQuizBankAsync(int lessonId, CreateQuizBankDto createQuizBankDto)
        {
            _ = await _lessonRepository.GetByIdAsync(lessonId) ?? throw new ArgumentException($"Lesson with ID {lessonId} not found.");
            var quizBank = await _quizRepository.GetOrCreateQuizBankForLessonAsync(lessonId);
            quizBank.QuizBankSize = createQuizBankDto.QuizBankSize;
            await _quizRepository.UpdateQuizBankAsync(quizBank);

            if (createQuizBankDto.Questions != null)
            {
                foreach (var qDto in createQuizBankDto.Questions)
                    await AddQuestionToQuizBankAsync(quizBank.QuizBankId, qDto);
            }
            return await GetQuizBankByIdAsync(quizBank.QuizBankId) ?? throw new InvalidOperationException("Failed to retrieve created quiz bank.");
        }

        public async Task<QuizBankQuestionDto?> GetQuestionByIdAsync(int questionId)
        {
            var question = await _quizRepository.GetQuizBankQuestionByIdAsync(questionId);
            return question == null ? null : MapQuestionToDto(question);
        }

        public async Task<IEnumerable<QuizBankQuestionDto>> GetQuestionsForQuizBankAsync(int quizBankId)
        {
            var questions = await _quizRepository.GetQuestionsForQuizBankAsync(quizBankId);
            return questions.Select(MapQuestionToDto);
        }

        public async Task<QuizBankQuestionDto> AddQuestionToQuizBankAsync(int quizBankId, CreateQuizBankQuestionDto createQuestionDto)
        {
            _ = await _quizRepository.GetQuizBankByIdAsync(quizBankId) ?? throw new ArgumentException($"Quiz bank with ID {quizBankId} not found.");
            var questionEntity = new QuizBankQuestion
            {
                QuizBankId = quizBankId,
                QuestionContent = createQuestionDto.QuestionContent,
                QuestionType = "mcq",
                QuestionBankOrder = createQuestionDto.QuestionBankOrder
            };
            var createdQuestion = await _quizRepository.AddQuestionToQuizBankAsync(questionEntity);
            foreach (var optDto in createQuestionDto.Options)
            {
                await _quizRepository.AddOptionToQuestionAsync(new MCQQuestionOption
                {
                    QuizBankQuestionId = createdQuestion.QuizBankQuestionId,
                    OptionText = optDto.OptionText,
                    IsCorrect = optDto.IsCorrect
                });
            }
            var refreshedQuestion = await _quizRepository.GetQuizBankQuestionByIdAsync(createdQuestion.QuizBankQuestionId);
            return MapQuestionToDto(refreshedQuestion!);
        }

        public async Task UpdateQuizBankQuestionAsync(int questionId, UpdateQuizBankQuestionDto updateQuestionDto)
        {
            var question = await _quizRepository.GetQuizBankQuestionByIdAsync(questionId) ?? throw new ArgumentException($"Question with ID {questionId} not found.");
            if (!string.IsNullOrEmpty(updateQuestionDto.QuestionContent))
                question.QuestionContent = updateQuestionDto.QuestionContent;
            if (updateQuestionDto.QuestionBankOrder.HasValue)
                question.QuestionBankOrder = updateQuestionDto.QuestionBankOrder;
            await _quizRepository.UpdateQuizBankQuestionAsync(question);

            if (updateQuestionDto.Options != null)
            {
                var existingOptions = (await _quizRepository.GetOptionsForQuestionAsync(questionId)).ToList();
                var optionsInUse = new List<int>();
                foreach (var opt in existingOptions)
                {
                    if (await _quizRepository.IsOptionUsedInAttemptsAsync(opt.McqOptionId))
                        optionsInUse.Add(opt.McqOptionId);
                    else
                        await _quizRepository.DeleteOptionAsync(opt.McqOptionId);
                }
                for (int i = 0; i < updateQuestionDto.Options.Count; i++)
                {
                    var optDto = updateQuestionDto.Options[i];
                    if (i < optionsInUse.Count)
                    {
                        var optToUpdate = await _quizRepository.GetMCQOptionByIdAsync(optionsInUse[i]);
                        if (optToUpdate != null)
                        {
                            optToUpdate.OptionText = optDto.OptionText;
                            optToUpdate.IsCorrect = optDto.IsCorrect;
                            await _quizRepository.UpdateOptionAsync(optToUpdate);
                        }
                    }
                    else
                    {
                        await _quizRepository.AddOptionToQuestionAsync(new MCQQuestionOption
                        {
                            QuizBankQuestionId = questionId,
                            OptionText = optDto.OptionText,
                            IsCorrect = optDto.IsCorrect
                        });
                    }
                }
            }
        }

        public async Task DeleteQuizBankQuestionAsync(int questionId)
        {
            _ = await _quizRepository.GetQuizBankQuestionByIdAsync(questionId) ?? throw new ArgumentException($"Question with ID {questionId} not found.");
            var options = await _quizRepository.GetOptionsForQuestionAsync(questionId);
            foreach (var opt in options)
                await _quizRepository.DeleteOptionAsync(opt.McqOptionId);
            await _quizRepository.DeleteQuizBankQuestionAsync(questionId);
        }

        public async Task<IEnumerable<LearnerQuizQuestionDto>> GetQuestionsForLearnerQuizAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId) ?? throw new ArgumentException($"Quiz with ID {quizId} not found.");
            var questions = await _quizRepository.GetRandomQuestionsForQuizAsync(quizId, quiz.QuizSize);
            return questions.Select(q => new LearnerQuizQuestionDto
            {
                QuizBankQuestionId = q.QuizBankQuestionId,
                QuestionContent = q.QuestionContent,
                QuestionType = q.QuestionType,
                Options = q.MCQQuestionOptions.Select(o => new LearnerMCQOptionDto { McqOptionId = o.McqOptionId, OptionText = o.OptionText }).ToList()
            });
        }

        private QuizDto MapQuizToDto(Quiz quiz) => new QuizDto { QuizId = quiz.QuizId, QuizTitle = quiz.QuizTitle, TimeLimitMinutes = quiz.TimeLimitMinutes, TotalMarks = quiz.TotalMarks, QuizSize = quiz.QuizSize, QuizBankId = quiz.QuizBankId, LessonId = quiz.LessonId, LessonName = quiz.Lesson?.LessonName ?? "Unknown" };
        private QuizBankQuestionDto MapQuestionToDto(QuizBankQuestion question) => new QuizBankQuestionDto { QuizBankQuestionId = question.QuizBankQuestionId, QuestionContent = question.QuestionContent, QuestionType = question.QuestionType, QuestionBankOrder = question.QuestionBankOrder, Options = question.MCQQuestionOptions.Select(MapOptionToDto).ToList() };
        private MCQQuestionOptionDto MapOptionToDto(MCQQuestionOption option) => new MCQQuestionOptionDto { McqOptionId = option.McqOptionId, OptionText = option.OptionText, IsCorrect = option.IsCorrect };
        #endregion
    }
}