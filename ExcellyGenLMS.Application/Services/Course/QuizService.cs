// ExcellyGenLMS.Application/Services/Course/QuizService.cs
// This file belongs to the ExcellyGenLMS.Application project.
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Course // Correct namespace for Application project (Services layer)
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ILessonRepository _lessonRepository; // Correctly referencing LessonRepository via interface
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            IQuizRepository quizRepository,
            ILessonRepository lessonRepository,
            ILogger<QuizService> logger)
        {
            _quizRepository = quizRepository ?? throw new ArgumentNullException(nameof(quizRepository));
            _lessonRepository = lessonRepository ?? throw new ArgumentNullException(nameof(lessonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Quiz management
        public async Task<QuizDto?> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
                return null;

            return MapQuizToDto(quiz);
        }

        public async Task<QuizDetailDto?> GetQuizDetailsAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
                return null;

            var quizBank = await _quizRepository.GetQuizBankByIdAsync(quiz.QuizBankId);
            if (quizBank == null)
                return null;

            var questions = await _quizRepository.GetQuestionsForQuizBankAsync(quiz.QuizBankId);

            var quizDetailDto = new QuizDetailDto
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.QuizTitle,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                TotalMarks = quiz.TotalMarks,
                QuizSize = quiz.QuizSize,
                QuizBankId = quiz.QuizBankId,
                LessonId = quiz.LessonId,
                LessonName = quiz.Lesson?.LessonName ?? "Unknown Lesson",
                Questions = questions.Select(MapQuestionToDto).ToList()
            };

            return quizDetailDto;
        }

        public async Task<IEnumerable<QuizDto>> GetQuizzesByLessonIdAsync(int lessonId)
        {
            var quizzes = await _quizRepository.GetQuizzesByLessonIdAsync(lessonId);
            return quizzes.Select(MapQuizToDto);
        }

        public async Task<QuizDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            var lesson = await _lessonRepository.GetByIdAsync(createQuizDto.LessonId);
            if (lesson == null)
                throw new ArgumentException($"Lesson with ID {createQuizDto.LessonId} not found.");

            var quizBank = await _quizRepository.GetOrCreateQuizBankForLessonAsync(createQuizDto.LessonId);

            var quiz = new Quiz
            {
                QuizTitle = createQuizDto.QuizTitle,
                TimeLimitMinutes = createQuizDto.TimeLimitMinutes,
                QuizSize = createQuizDto.QuizSize,
                TotalMarks = createQuizDto.QuizSize, // Default: 1 mark per question
                LessonId = createQuizDto.LessonId,
                QuizBankId = quizBank.QuizBankId
            };

            var createdQuiz = await _quizRepository.CreateQuizAsync(quiz);
            return MapQuizToDto(createdQuiz);
        }

        public async Task UpdateQuizAsync(int quizId, CreateQuizDto updateQuizDto)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
                throw new ArgumentException($"Quiz with ID {quizId} not found.");

            if (quiz.LessonId != updateQuizDto.LessonId)
            {
                var newLesson = await _lessonRepository.GetByIdAsync(updateQuizDto.LessonId);
                if (newLesson == null)
                    throw new ArgumentException($"Lesson with ID {updateQuizDto.LessonId} not found.");
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
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
                throw new ArgumentException($"Quiz with ID {quizId} not found.");

            await _quizRepository.DeleteQuizAsync(quizId);
        }

        // Quiz Bank management
        public async Task<QuizBankDto?> GetQuizBankByIdAsync(int quizBankId)
        {
            var quizBank = await _quizRepository.GetQuizBankByIdAsync(quizBankId);
            if (quizBank == null)
                return null;

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
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new ArgumentException($"Lesson with ID {lessonId} not found.");

            var quizBank = new QuizBank
            {
                QuizBankSize = createQuizBankDto.QuizBankSize
            };

            var createdQuizBank = await _quizRepository.CreateQuizBankAsync(quizBank);

            if (createQuizBankDto.Questions != null && createQuizBankDto.Questions.Any())
            {
                foreach (var questionDto in createQuizBankDto.Questions)
                {
                    await AddQuestionToQuizBankAsync(createdQuizBank.QuizBankId, questionDto);
                }
            }

            return await GetQuizBankByIdAsync(createdQuizBank.QuizBankId) ??
                   throw new InvalidOperationException("Failed to retrieve created quiz bank.");
        }

        // Question management
        public async Task<QuizBankQuestionDto?> GetQuestionByIdAsync(int questionId)
        {
            var question = await _quizRepository.GetQuizBankQuestionByIdAsync(questionId);
            if (question == null)
                return null;

            return MapQuestionToDto(question);
        }

        public async Task<IEnumerable<QuizBankQuestionDto>> GetQuestionsForQuizBankAsync(int quizBankId)
        {
            var questions = await _quizRepository.GetQuestionsForQuizBankAsync(quizBankId);
            return questions.Select(MapQuestionToDto);
        }

        public async Task<QuizBankQuestionDto> AddQuestionToQuizBankAsync(int quizBankId, CreateQuizBankQuestionDto createQuestionDto)
        {
            var quizBank = await _quizRepository.GetQuizBankByIdAsync(quizBankId);
            if (quizBank == null)
                throw new ArgumentException($"Quiz bank with ID {quizBankId} not found.");

            var questionEntity = new QuizBankQuestion
            {
                QuizBankId = quizBankId,
                QuestionContent = createQuestionDto.QuestionContent,
                QuestionType = "mcq",
                QuestionBankOrder = createQuestionDto.QuestionBankOrder
            };

            var createdQuestion = await _quizRepository.AddQuestionToQuizBankAsync(questionEntity);

            foreach (var optionDto in createQuestionDto.Options)
            {
                var optionEntity = new MCQQuestionOption
                {
                    QuizBankQuestionId = createdQuestion.QuizBankQuestionId,
                    OptionText = optionDto.OptionText,
                    IsCorrect = optionDto.IsCorrect
                };

                await _quizRepository.AddOptionToQuestionAsync(optionEntity);
            }

            var refreshedQuestion = await _quizRepository.GetQuizBankQuestionByIdAsync(createdQuestion.QuizBankQuestionId);
            return MapQuestionToDto(refreshedQuestion!);
        }

        public async Task UpdateQuizBankQuestionAsync(int questionId, UpdateQuizBankQuestionDto updateQuestionDto)
        {
            var question = await _quizRepository.GetQuizBankQuestionByIdAsync(questionId);
            if (question == null)
                throw new ArgumentException($"Question with ID {questionId} not found.");

            if (!string.IsNullOrEmpty(updateQuestionDto.QuestionContent))
                question.QuestionContent = updateQuestionDto.QuestionContent;

            if (updateQuestionDto.QuestionBankOrder.HasValue)
                question.QuestionBankOrder = updateQuestionDto.QuestionBankOrder;

            await _quizRepository.UpdateQuizBankQuestionAsync(question);

            if (updateQuestionDto.Options != null && updateQuestionDto.Options.Any())
            {
                var existingOptions = await _quizRepository.GetOptionsForQuestionAsync(questionId);
                foreach (var option in existingOptions)
                {
                    await _quizRepository.DeleteOptionAsync(option.McqOptionId);
                }

                foreach (var optionDto in updateQuestionDto.Options)
                {
                    var optionEntity = new MCQQuestionOption
                    {
                        QuizBankQuestionId = questionId,
                        OptionText = optionDto.OptionText,
                        IsCorrect = optionDto.IsCorrect
                    };

                    await _quizRepository.AddOptionToQuestionAsync(optionEntity);
                }
            }
        }

        public async Task DeleteQuizBankQuestionAsync(int questionId)
        {
            var question = await _quizRepository.GetQuizBankQuestionByIdAsync(questionId);
            if (question == null)
                throw new ArgumentException($"Question with ID {questionId} not found.");

            var options = await _quizRepository.GetOptionsForQuestionAsync(questionId);
            foreach (var option in options)
            {
                await _quizRepository.DeleteOptionAsync(option.McqOptionId);
            }

            await _quizRepository.DeleteQuizBankQuestionAsync(questionId);
        }

        // Learner-facing quiz methods
        public async Task<IEnumerable<LearnerQuizQuestionDto>> GetQuestionsForLearnerQuizAsync(int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
                throw new ArgumentException($"Quiz with ID {quizId} not found.");

            var questions = await _quizRepository.GetRandomQuestionsForQuizAsync(quizId, quiz.QuizSize);

            return questions.Select(q => new LearnerQuizQuestionDto
            {
                QuizBankQuestionId = q.QuizBankQuestionId,
                QuestionContent = q.QuestionContent,
                QuestionType = q.QuestionType,
                Options = q.MCQQuestionOptions.Select(o => new LearnerMCQOptionDto
                {
                    McqOptionId = o.McqOptionId,
                    OptionText = o.OptionText
                }).ToList()
            });
        }

        // Helper methods
        private QuizDto MapQuizToDto(Quiz quiz)
        {
            return new QuizDto
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.QuizTitle,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                TotalMarks = quiz.TotalMarks,
                QuizSize = quiz.QuizSize,
                QuizBankId = quiz.QuizBankId,
                LessonId = quiz.LessonId,
                LessonName = quiz.Lesson?.LessonName ?? "Unknown Lesson"
            };
        }

        private QuizBankQuestionDto MapQuestionToDto(QuizBankQuestion question)
        {
            return new QuizBankQuestionDto
            {
                QuizBankQuestionId = question.QuizBankQuestionId,
                QuestionContent = question.QuestionContent,
                QuestionType = question.QuestionType,
                QuestionBankOrder = question.QuestionBankOrder,
                Options = question.MCQQuestionOptions.Select(MapOptionToDto).ToList()
            };
        }

        private MCQQuestionOptionDto MapOptionToDto(MCQQuestionOption option)
        {
            return new MCQQuestionOptionDto
            {
                McqOptionId = option.McqOptionId,
                OptionText = option.OptionText,
                IsCorrect = option.IsCorrect
            };
        }
    }
}