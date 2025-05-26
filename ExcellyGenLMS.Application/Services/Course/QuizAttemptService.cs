// ExcellyGenLMS.Application/Services/Course/QuizAttemptService.cs
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
    public class QuizAttemptService : IQuizAttemptService
    {
        private readonly IQuizAttemptRepository _quizAttemptRepository;
        private readonly IQuizRepository _quizRepository;
        private readonly ILogger<QuizAttemptService> _logger;

        public QuizAttemptService(
            IQuizAttemptRepository quizAttemptRepository,
            IQuizRepository quizRepository,
            ILogger<QuizAttemptService> logger)
        {
            _quizAttemptRepository = quizAttemptRepository ?? throw new ArgumentNullException(nameof(quizAttemptRepository));
            _quizRepository = quizRepository ?? throw new ArgumentNullException(nameof(quizRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<QuizAttemptDto> StartQuizAttemptAsync(string userId, int quizId)
        {
            try
            {
                _logger.LogInformation($"Starting quiz attempt for user {userId} on quiz {quizId}");

                var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
                if (quiz == null)
                    throw new ArgumentException($"Quiz with ID {quizId} not found.");

                // Check if user already has an active attempt for this quiz
                var existingAttempt = await _quizAttemptRepository.GetActiveAttemptByUserAndQuizAsync(userId, quizId);
                if (existingAttempt != null)
                {
                    _logger.LogInformation($"Found existing active attempt {existingAttempt.QuizAttemptId} for user {userId} on quiz {quizId}");
                    return MapQuizAttemptToDto(existingAttempt);
                }

                // Create new attempt
                var newAttempt = new QuizAttempt
                {
                    QuizId = quizId,
                    UserId = userId,
                    StartTime = DateTime.UtcNow,
                    IsCompleted = false,
                    TotalQuestions = quiz.QuizSize,
                    CorrectAnswers = 0
                };

                var createdAttempt = await _quizAttemptRepository.CreateQuizAttemptAsync(newAttempt);

                // Load the quiz details for the response
                var attemptWithQuiz = await _quizAttemptRepository.GetQuizAttemptByIdAsync(createdAttempt.QuizAttemptId);

                _logger.LogInformation($"Created new quiz attempt {createdAttempt.QuizAttemptId} for user {userId} on quiz {quizId}");
                return MapQuizAttemptToDto(attemptWithQuiz!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting quiz attempt for user {userId} on quiz {quizId}");
                throw;
            }
        }

        public async Task<bool> SubmitQuizAnswerAsync(SubmitQuizAnswerDto submitAnswerDto)
        {
            try
            {
                var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(submitAnswerDto.QuizAttemptId);
                if (attempt == null)
                    throw new ArgumentException($"Quiz attempt with ID {submitAnswerDto.QuizAttemptId} not found.");

                if (attempt.IsCompleted)
                    throw new InvalidOperationException("Cannot submit answer to a completed quiz attempt.");

                // Check if answer already exists for this question
                var existingAnswer = await _quizAttemptRepository.GetAnswerByAttemptAndQuestionAsync(
                    submitAnswerDto.QuizAttemptId, submitAnswerDto.QuizBankQuestionId);

                if (existingAnswer != null)
                {
                    // Update existing answer
                    existingAnswer.SelectedOptionId = submitAnswerDto.SelectedOptionId;

                    // Check if answer is correct
                    var selectedOption = await _quizRepository.GetMCQOptionByIdAsync(submitAnswerDto.SelectedOptionId);
                    existingAnswer.IsCorrect = selectedOption?.IsCorrect ?? false;

                    await _quizAttemptRepository.UpdateQuizAttemptAnswerAsync(existingAnswer);
                }
                else
                {
                    // Create new answer
                    var selectedOption = await _quizRepository.GetMCQOptionByIdAsync(submitAnswerDto.SelectedOptionId);
                    var isCorrect = selectedOption?.IsCorrect ?? false;

                    var newAnswer = new QuizAttemptAnswer
                    {
                        QuizAttemptId = submitAnswerDto.QuizAttemptId,
                        QuizBankQuestionId = submitAnswerDto.QuizBankQuestionId,
                        SelectedOptionId = submitAnswerDto.SelectedOptionId,
                        IsCorrect = isCorrect
                    };

                    await _quizAttemptRepository.CreateQuizAttemptAnswerAsync(newAnswer);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting quiz answer for attempt {submitAnswerDto.QuizAttemptId}");
                throw;
            }
        }

        public async Task<QuizAttemptDto> CompleteQuizAttemptAsync(CompleteQuizAttemptDto completeAttemptDto)
        {
            try
            {
                var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(completeAttemptDto.QuizAttemptId);
                if (attempt == null)
                    throw new ArgumentException($"Quiz attempt with ID {completeAttemptDto.QuizAttemptId} not found.");

                if (attempt.IsCompleted)
                    throw new InvalidOperationException("Quiz attempt is already completed.");

                // Calculate score
                var answers = await _quizAttemptRepository.GetAnswersByAttemptIdAsync(completeAttemptDto.QuizAttemptId);
                var correctAnswers = answers.Count(a => a.IsCorrect);

                // Update attempt
                attempt.CompletionTime = DateTime.UtcNow;
                attempt.IsCompleted = true;
                attempt.CorrectAnswers = correctAnswers;
                attempt.Score = correctAnswers; // Simple scoring: 1 point per correct answer

                await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

                var updatedAttempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(completeAttemptDto.QuizAttemptId);
                return MapQuizAttemptToDto(updatedAttempt!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing quiz attempt {completeAttemptDto.QuizAttemptId}");
                throw;
            }
        }

        public async Task<QuizAttemptDetailDto?> GetQuizAttemptDetailsAsync(int attemptId)
        {
            var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(attemptId);
            if (attempt == null)
                return null;

            var answers = await _quizAttemptRepository.GetAnswersByAttemptIdAsync(attemptId);

            return new QuizAttemptDetailDto
            {
                QuizAttemptId = attempt.QuizAttemptId,
                QuizId = attempt.QuizId,
                QuizTitle = attempt.Quiz?.QuizTitle ?? "Unknown Quiz",
                StartTime = attempt.StartTime,
                CompletionTime = attempt.CompletionTime,
                Score = attempt.Score,
                IsCompleted = attempt.IsCompleted,
                TotalQuestions = attempt.TotalQuestions,
                CorrectAnswers = attempt.CorrectAnswers,
                Answers = answers.Select(MapAnswerToDto).ToList()
            };
        }

        public async Task<IEnumerable<QuizAttemptDto>> GetAttemptsByUserAsync(string userId)
        {
            var attempts = await _quizAttemptRepository.GetAttemptsByUserIdAsync(userId);
            return attempts.Select(MapQuizAttemptToDto);
        }

        public async Task<IEnumerable<QuizAttemptDto>> GetAttemptsByQuizAsync(int quizId)
        {
            var attempts = await _quizAttemptRepository.GetAttemptsByQuizIdAsync(quizId);
            return attempts.Select(MapQuizAttemptToDto);
        }

        public async Task<bool> HasUserCompletedQuizAsync(string userId, int quizId)
        {
            try
            {
                _logger.LogInformation("Checking if user {UserId} has completed quiz {QuizId}", userId, quizId);

                var attempts = await _quizAttemptRepository.GetAttemptsByUserIdAsync(userId);
                bool hasCompleted = attempts.Any(a => a.QuizId == quizId && a.IsCompleted);

                _logger.LogInformation("User {UserId} quiz {QuizId} completion status: {IsCompleted}",
                    userId, quizId, hasCompleted);

                return hasCompleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking quiz completion for user {UserId} and quiz {QuizId}", userId, quizId);
                return false;
            }
        }

        private QuizAttemptDto MapQuizAttemptToDto(QuizAttempt attempt)
        {
            return new QuizAttemptDto
            {
                QuizAttemptId = attempt.QuizAttemptId,
                QuizId = attempt.QuizId,
                QuizTitle = attempt.Quiz?.QuizTitle ?? "Unknown Quiz",
                StartTime = attempt.StartTime,
                CompletionTime = attempt.CompletionTime,
                Score = attempt.Score,
                IsCompleted = attempt.IsCompleted,
                TotalQuestions = attempt.TotalQuestions,
                CorrectAnswers = attempt.CorrectAnswers
            };
        }

        private QuizAttemptAnswerDto MapAnswerToDto(QuizAttemptAnswer answer)
        {
            return new QuizAttemptAnswerDto
            {
                QuizAttemptAnswerId = answer.QuizAttemptAnswerId,
                QuizAttemptId = answer.QuizAttemptId,
                QuizBankQuestionId = answer.QuizBankQuestionId,
                QuestionContent = answer.Question?.QuestionContent ?? "Unknown Question",
                SelectedOptionId = answer.SelectedOptionId,
                SelectedOptionText = answer.SelectedOption?.OptionText ?? "No answer selected",
                IsCorrect = answer.IsCorrect,
                CorrectOptionText = answer.Question?.MCQQuestionOptions?.FirstOrDefault(o => o.IsCorrect)?.OptionText ?? "Unknown"
            };
        }
    }
}