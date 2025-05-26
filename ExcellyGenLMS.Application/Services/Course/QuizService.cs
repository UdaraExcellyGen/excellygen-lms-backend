// ExcellyGenLMS.Application/Services/Course/QuizService.cs
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
            try
            {
                _logger.LogInformation($"Getting quiz details for quiz ID: {quizId}");

                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    _logger.LogWarning($"Quiz with ID {quizId} not found");
                    return null;
                }

                var quizBank = await _quizRepository.GetQuizBankByIdAsync(quiz.QuizBankId);
                if (quizBank == null)
                {
                    _logger.LogWarning($"Quiz bank with ID {quiz.QuizBankId} not found for quiz {quizId}");
                    return null;
                }

                var questions = await _quizRepository.GetQuestionsForQuizBankAsync(quiz.QuizBankId);
                _logger.LogInformation($"Found {questions.Count()} questions for quiz bank {quiz.QuizBankId}");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting quiz details for quiz ID: {quizId}");
                throw;
            }
        }

        public async Task<IEnumerable<QuizDto>> GetQuizzesByLessonIdAsync(int lessonId)
        {
            var quizzes = await _quizRepository.GetQuizzesByLessonIdAsync(lessonId);
            return quizzes.Select(MapQuizToDto);
        }

        public async Task<QuizDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            try
            {
                _logger.LogInformation($"Creating quiz for lesson {createQuizDto.LessonId}");

                var lesson = await _lessonRepository.GetByIdAsync(createQuizDto.LessonId);
                if (lesson == null)
                    throw new ArgumentException($"Lesson with ID {createQuizDto.LessonId} not found.");

                // Check if quiz already exists for this lesson
                var existingQuiz = await _quizRepository.GetQuizByLessonIdAsync(createQuizDto.LessonId);
                if (existingQuiz != null)
                {
                    throw new InvalidOperationException($"A quiz already exists for lesson {createQuizDto.LessonId}");
                }

                // Create or get quiz bank for this lesson
                var quizBank = await _quizRepository.GetOrCreateQuizBankForLessonAsync(createQuizDto.LessonId);
                _logger.LogInformation($"Using QuizBank ID: {quizBank.QuizBankId}");

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
                _logger.LogInformation($"Created quiz with ID: {createdQuiz.QuizId} linked to QuizBank ID: {createdQuiz.QuizBankId}");

                return MapQuizToDto(createdQuiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating quiz for lesson {createQuizDto.LessonId}");
                throw;
            }
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
            try
            {
                _logger.LogInformation($"Creating quiz bank for lesson {lessonId} with {createQuizBankDto.Questions?.Count ?? 0} questions");

                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new ArgumentException($"Lesson with ID {lessonId} not found.");

                // Get or create quiz bank
                var quizBank = await _quizRepository.GetOrCreateQuizBankForLessonAsync(lessonId);

                // Update quiz bank size
                quizBank.QuizBankSize = createQuizBankDto.QuizBankSize;
                await _quizRepository.UpdateQuizBankAsync(quizBank);

                _logger.LogInformation($"Using QuizBank ID: {quizBank.QuizBankId}, Size: {quizBank.QuizBankSize}");

                if (createQuizBankDto.Questions != null && createQuizBankDto.Questions.Any())
                {
                    _logger.LogInformation($"Adding {createQuizBankDto.Questions.Count} questions to quiz bank");

                    foreach (var questionDto in createQuizBankDto.Questions)
                    {
                        var addedQuestion = await AddQuestionToQuizBankAsync(quizBank.QuizBankId, questionDto);
                        _logger.LogInformation($"Added question {addedQuestion.QuizBankQuestionId}: '{addedQuestion.QuestionContent}'");
                    }
                }

                var result = await GetQuizBankByIdAsync(quizBank.QuizBankId);
                if (result == null)
                    throw new InvalidOperationException("Failed to retrieve created quiz bank.");

                _logger.LogInformation($"Successfully created quiz bank with {result.Questions.Count} questions");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating quiz bank for lesson {lessonId}");
                throw;
            }
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

        // FIXED: Learner-facing quiz methods with better error handling and logging
        public async Task<IEnumerable<LearnerQuizQuestionDto>> GetQuestionsForLearnerQuizAsync(int quizId)
        {
            try
            {
                _logger.LogInformation($"🔍 Getting learner questions for quiz ID: {quizId}");

                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                {
                    _logger.LogError($"❌ Quiz with ID {quizId} not found");
                    throw new ArgumentException($"Quiz with ID {quizId} not found.");
                }

                _logger.LogInformation($"✅ Quiz found: {quiz.QuizTitle}, requesting {quiz.QuizSize} questions from quiz bank {quiz.QuizBankId}");

                // First, let's check if the quiz bank has questions
                var allQuestionsInBank = await _quizRepository.GetQuestionsForQuizBankAsync(quiz.QuizBankId);
                var allQuestionsList = allQuestionsInBank.ToList();

                _logger.LogInformation($"📊 Quiz bank {quiz.QuizBankId} contains {allQuestionsList.Count} total questions");

                if (allQuestionsList.Count == 0)
                {
                    _logger.LogWarning($"⚠️ Quiz bank {quiz.QuizBankId} is empty - no questions available");
                    return new List<LearnerQuizQuestionDto>();
                }

                // Get random questions for the quiz
                var questions = await _quizRepository.GetRandomQuestionsForQuizAsync(quizId, quiz.QuizSize);
                var questionsList = questions.ToList();

                _logger.LogInformation($"🎲 Retrieved {questionsList.Count} random questions for quiz {quizId}");

                if (!questionsList.Any())
                {
                    _logger.LogWarning($"⚠️ No questions returned by GetRandomQuestionsForQuizAsync for quiz {quizId}");
                    return new List<LearnerQuizQuestionDto>();
                }

                var learnerQuestions = questionsList.Select(q => {
                    var learnerQuestion = new LearnerQuizQuestionDto
                    {
                        QuizBankQuestionId = q.QuizBankQuestionId,
                        QuestionContent = q.QuestionContent,
                        QuestionType = q.QuestionType,
                        Options = q.MCQQuestionOptions.Select(o => new LearnerMCQOptionDto
                        {
                            McqOptionId = o.McqOptionId,
                            OptionText = o.OptionText
                            // IsCorrect is deliberately omitted for learner view
                        }).ToList()
                    };

                    _logger.LogInformation($"   📝 Question {q.QuizBankQuestionId}: '{q.QuestionContent}' with {learnerQuestion.Options.Count} options");

                    return learnerQuestion;
                }).ToList();

                _logger.LogInformation($"✅ Successfully mapped {learnerQuestions.Count} learner questions for quiz {quizId}");
                return learnerQuestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 Error getting learner questions for quiz {quizId}");
                throw;
            }
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