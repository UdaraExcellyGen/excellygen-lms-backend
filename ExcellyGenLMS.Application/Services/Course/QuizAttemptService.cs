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

        public async Task<QuizAttemptDto?> GetQuizAttemptByIdAsync(int attemptId)
        {
            var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(attemptId);
            if (attempt == null)
                return null;

            return MapAttemptToDto(attempt);
        }

        public async Task<QuizAttemptDetailDto?> GetQuizAttemptDetailsAsync(int attemptId)
        {
            var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(attemptId);
            if (attempt == null)
                return null;

            var answers = await _quizAttemptRepository.GetAnswersByAttemptIdAsync(attemptId);

            var detailDto = new QuizAttemptDetailDto
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

            return detailDto;
        }

        public async Task<IEnumerable<QuizAttemptDto>> GetAttemptsByUserAsync(string userId)
        {
            var attempts = await _quizAttemptRepository.GetAttemptsByUserIdAsync(userId);
            return attempts.Select(MapAttemptToDto);
        }

        public async Task<IEnumerable<QuizAttemptDto>> GetAttemptsByQuizAsync(int quizId)
        {
            var attempts = await _quizAttemptRepository.GetAttemptsByQuizIdAsync(quizId);
            return attempts.Select(MapAttemptToDto);
        }

        public async Task<QuizAttemptDto> StartQuizAttemptAsync(string userId, int quizId)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(quizId);
            if (quiz == null)
                throw new ArgumentException($"Quiz with ID {quizId} not found.");

            // Check if user has an incomplete attempt
            var userAttempts = await _quizAttemptRepository.GetAttemptsByUserAndQuizAsync(userId, quizId);
            var incompleteAttempt = userAttempts.FirstOrDefault(a => !a.IsCompleted);
            if (incompleteAttempt != null)
                return MapAttemptToDto(incompleteAttempt);

            // Get random questions for the quiz
            var questions = await _quizRepository.GetRandomQuestionsForQuizAsync(quizId, quiz.QuizSize);

            // Create new attempt
            var attempt = new QuizAttempt
            {
                QuizId = quizId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                TotalQuestions = questions.Count()
            };

            var createdAttempt = await _quizAttemptRepository.CreateQuizAttemptAsync(attempt);
            return MapAttemptToDto(createdAttempt);
        }

        public async Task<bool> SubmitQuizAnswerAsync(SubmitQuizAnswerDto submitAnswerDto)
        {
            var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(submitAnswerDto.QuizAttemptId);
            if (attempt == null)
                throw new ArgumentException($"Quiz attempt with ID {submitAnswerDto.QuizAttemptId} not found.");

            if (attempt.IsCompleted)
                throw new InvalidOperationException("Cannot submit answers to a completed quiz attempt.");

            var question = await _quizRepository.GetQuizBankQuestionByIdAsync(submitAnswerDto.QuizBankQuestionId);
            if (question == null)
                throw new ArgumentException($"Question with ID {submitAnswerDto.QuizBankQuestionId} not found.");

            var option = await _quizRepository.GetMCQOptionByIdAsync(submitAnswerDto.SelectedOptionId);
            if (option == null)
                throw new ArgumentException($"Option with ID {submitAnswerDto.SelectedOptionId} not found.");

            // Check if this question already has an answer
            var existingAnswers = await _quizAttemptRepository.GetAnswersByAttemptIdAsync(attempt.QuizAttemptId);
            var existingAnswer = existingAnswers.FirstOrDefault(a => a.QuizBankQuestionId == submitAnswerDto.QuizBankQuestionId);

            if (existingAnswer != null)
            {
                // Update existing answer
                existingAnswer.SelectedOptionId = submitAnswerDto.SelectedOptionId;
                existingAnswer.IsCorrect = option.IsCorrect;
                await _quizAttemptRepository.UpdateQuizAttemptAnswerAsync(existingAnswer);
            }
            else
            {
                // Create new answer
                var answer = new QuizAttemptAnswer
                {
                    QuizAttemptId = attempt.QuizAttemptId,
                    QuizBankQuestionId = submitAnswerDto.QuizBankQuestionId,
                    SelectedOptionId = submitAnswerDto.SelectedOptionId,
                    IsCorrect = option.IsCorrect
                };
                await _quizAttemptRepository.AddAnswerToAttemptAsync(answer);
            }

            return true;
        }

        public async Task<QuizAttemptDto> CompleteQuizAttemptAsync(CompleteQuizAttemptDto completeAttemptDto)
        {
            var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(completeAttemptDto.QuizAttemptId);
            if (attempt == null)
                throw new ArgumentException($"Quiz attempt with ID {completeAttemptDto.QuizAttemptId} not found.");

            if (attempt.IsCompleted)
                throw new InvalidOperationException("Quiz attempt is already completed.");

            // Get all answers for this attempt
            var answers = await _quizAttemptRepository.GetAnswersByAttemptIdAsync(attempt.QuizAttemptId);

            // Calculate score
            int correctAnswers = answers.Count(a => a.IsCorrect);

            // Update attempt
            attempt.IsCompleted = true;
            attempt.CompletionTime = DateTime.UtcNow;
            attempt.CorrectAnswers = correctAnswers;
            attempt.Score = correctAnswers; // Assuming 1 point per correct answer

            await _quizAttemptRepository.UpdateQuizAttemptAsync(attempt);

            return MapAttemptToDto(attempt);
        }

        public async Task<bool> IsQuizAttemptCompletedAsync(int attemptId)
        {
            var attempt = await _quizAttemptRepository.GetQuizAttemptByIdAsync(attemptId);
            return attempt?.IsCompleted ?? false;
        }

        public async Task<bool> HasUserCompletedQuizAsync(string userId, int quizId)
        {
            var attempts = await _quizAttemptRepository.GetAttemptsByUserAndQuizAsync(userId, quizId);
            return attempts.Any(a => a.IsCompleted);
        }

        // Helper methods
        private QuizAttemptDto MapAttemptToDto(QuizAttempt attempt)
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
            // Find the correct option for this question
            var correctOption = answer.Question?.MCQQuestionOptions.FirstOrDefault(o => o.IsCorrect);

            return new QuizAttemptAnswerDto
            {
                QuizAttemptAnswerId = answer.QuizAttemptAnswerId,
                QuizBankQuestionId = answer.QuizBankQuestionId,
                QuestionContent = answer.Question?.QuestionContent ?? "Unknown Question",
                SelectedOptionId = answer.SelectedOptionId,
                SelectedOptionText = answer.SelectedOption?.OptionText ?? "No selection",
                IsCorrect = answer.IsCorrect,
                CorrectOptionText = correctOption?.OptionText ?? "Unknown"
            };
        }
    }
}