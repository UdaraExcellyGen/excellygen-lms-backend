// ExcellyGenLMS.API/Controllers/Course/QuizController.cs
using ExcellyGenLMS.Application.DTOs.Course;
using ExcellyGenLMS.Application.Interfaces.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExcellyGenLMS.API.Controllers.Course
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IQuizAttemptService _quizAttemptService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(
            IQuizService quizService,
            IQuizAttemptService quizAttemptService,
            ILogger<QuizController> logger)
        {
            _quizService = quizService ?? throw new ArgumentNullException(nameof(quizService));
            _quizAttemptService = quizAttemptService ?? throw new ArgumentNullException(nameof(quizAttemptService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID claim not found in token");
                throw new UnauthorizedAccessException("User ID claim not found");
            }
            return userId;
        }

        // Coordinator/Admin routes - Quiz Management

        [HttpGet("lesson/{lessonId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzesByLessonId(int lessonId)
        {
            try
            {
                var quizzes = await _quizService.GetQuizzesByLessonIdAsync(lessonId);
                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quizzes for lesson {LessonId}", lessonId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving quizzes");
            }
        }

        [HttpGet("{quizId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<QuizDetailDto>> GetQuizDetails(int quizId)
        {
            try
            {
                var quiz = await _quizService.GetQuizDetailsAsync(quizId);
                if (quiz == null)
                    return NotFound($"Quiz with ID {quizId} not found");

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz details for {QuizId}", quizId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving quiz details");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<QuizDto>> CreateQuiz([FromBody] CreateQuizDto createQuizDto)
        {
            try
            {
                var quiz = await _quizService.CreateQuizAsync(createQuizDto);
                return CreatedAtAction(nameof(GetQuizDetails), new { quizId = quiz.QuizId }, quiz);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating quiz");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating quiz");
            }
        }

        [HttpPut("{quizId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult> UpdateQuiz(int quizId, [FromBody] CreateQuizDto updateQuizDto)
        {
            try
            {
                await _quizService.UpdateQuizAsync(quizId, updateQuizDto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating quiz {QuizId}", quizId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz {QuizId}", quizId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating quiz");
            }
        }

        [HttpDelete("{quizId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult> DeleteQuiz(int quizId)
        {
            try
            {
                await _quizService.DeleteQuizAsync(quizId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when deleting quiz {QuizId}", quizId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz {QuizId}", quizId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting quiz");
            }
        }

        // Quiz Bank Management

        [HttpGet("bank/{quizBankId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<QuizBankDto>> GetQuizBank(int quizBankId)
        {
            try
            {
                var quizBank = await _quizService.GetQuizBankByIdAsync(quizBankId);
                if (quizBank == null)
                    return NotFound($"Quiz bank with ID {quizBankId} not found");

                return Ok(quizBank);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz bank {QuizBankId}", quizBankId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving quiz bank");
            }
        }

        [HttpPost("bank/lesson/{lessonId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<QuizBankDto>> CreateQuizBank(int lessonId, [FromBody] CreateQuizBankDto createQuizBankDto)
        {
            try
            {
                var quizBank = await _quizService.CreateQuizBankAsync(lessonId, createQuizBankDto);
                return CreatedAtAction(nameof(GetQuizBank), new { quizBankId = quizBank.QuizBankId }, quizBank);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating quiz bank for lesson {LessonId}", lessonId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz bank for lesson {LessonId}", lessonId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating quiz bank");
            }
        }

        // Question Management

        [HttpGet("question/{questionId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<QuizBankQuestionDto>> GetQuestion(int questionId)
        {
            try
            {
                var question = await _quizService.GetQuestionByIdAsync(questionId);
                if (question == null)
                    return NotFound($"Question with ID {questionId} not found");

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question {QuestionId}", questionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving question");
            }
        }

        [HttpGet("bank/{quizBankId}/questions")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<IEnumerable<QuizBankQuestionDto>>> GetQuestionsForBank(int quizBankId)
        {
            try
            {
                var questions = await _quizService.GetQuestionsForQuizBankAsync(quizBankId);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questions for quiz bank {QuizBankId}", quizBankId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving questions");
            }
        }

        [HttpPost("bank/{quizBankId}/question")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<QuizBankQuestionDto>> AddQuestionToBank(int quizBankId, [FromBody] CreateQuizBankQuestionDto createQuestionDto)
        {
            try
            {
                var question = await _quizService.AddQuestionToQuizBankAsync(quizBankId, createQuestionDto);
                return CreatedAtAction(nameof(GetQuestion), new { questionId = question.QuizBankQuestionId }, question);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when adding question to bank {QuizBankId}", quizBankId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding question to bank {QuizBankId}", quizBankId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding question");
            }
        }

        [HttpPut("question/{questionId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuizBankQuestionDto updateQuestionDto)
        {
            try
            {
                await _quizService.UpdateQuizBankQuestionAsync(questionId, updateQuestionDto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating question {QuestionId}", questionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question {QuestionId}", questionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating question");
            }
        }

        [HttpDelete("question/{questionId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult> DeleteQuestion(int questionId)
        {
            try
            {
                await _quizService.DeleteQuizBankQuestionAsync(questionId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when deleting question {QuestionId}", questionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question {QuestionId}", questionId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting question");
            }
        }

        // Learner routes

        [HttpGet("learner/{quizId}")]
        public async Task<ActionResult<IEnumerable<LearnerQuizQuestionDto>>> GetQuestionsForLearner(int quizId)
        {
            try
            {
                var questions = await _quizService.GetQuestionsForLearnerQuizAsync(quizId);
                return Ok(questions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting learner questions for quiz {QuizId}", quizId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting learner questions for quiz {QuizId}", quizId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving questions");
            }
        }

        [HttpPost("attempt/start")]
        public async Task<ActionResult<QuizAttemptDto>> StartQuizAttempt([FromBody] StartQuizAttemptDto startAttemptDto)
        {
            try
            {
                var userId = GetUserId();
                var attempt = await _quizAttemptService.StartQuizAttemptAsync(userId, startAttemptDto.QuizId);
                return Ok(attempt);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when starting quiz attempt");
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting quiz attempt");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error starting quiz attempt");
            }
        }

        [HttpPost("attempt/answer")]
        public async Task<ActionResult> SubmitQuizAnswer([FromBody] SubmitQuizAnswerDto submitAnswerDto)
        {
            try
            {
                var result = await _quizAttemptService.SubmitQuizAnswerAsync(submitAnswerDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when submitting quiz answer");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when submitting quiz answer");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz answer");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error submitting answer");
            }
        }

        [HttpPost("attempt/complete")]
        public async Task<ActionResult<QuizAttemptDto>> CompleteQuizAttempt([FromBody] CompleteQuizAttemptDto completeAttemptDto)
        {
            try
            {
                var result = await _quizAttemptService.CompleteQuizAttemptAsync(completeAttemptDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when completing quiz attempt");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when completing quiz attempt");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing quiz attempt");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error completing attempt");
            }
        }

        [HttpGet("attempt/{attemptId}")]
        public async Task<ActionResult<QuizAttemptDetailDto>> GetQuizAttemptDetails(int attemptId)
        {
            try
            {
                var attempt = await _quizAttemptService.GetQuizAttemptDetailsAsync(attemptId);
                if (attempt == null)
                    return NotFound($"Quiz attempt with ID {attemptId} not found");

                return Ok(attempt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz attempt details for {AttemptId}", attemptId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving attempt details");
            }
        }

        [HttpGet("attempts/user")]
        public async Task<ActionResult<IEnumerable<QuizAttemptDto>>> GetUserAttempts()
        {
            try
            {
                var userId = GetUserId();
                var attempts = await _quizAttemptService.GetAttemptsByUserAsync(userId);
                return Ok(attempts);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user quiz attempts");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving attempts");
            }
        }

        [HttpGet("attempts/quiz/{quizId}")]
        [Authorize(Roles = "Admin,CourseCoordinator")]
        public async Task<ActionResult<IEnumerable<QuizAttemptDto>>> GetQuizAttempts(int quizId)
        {
            try
            {
                var attempts = await _quizAttemptService.GetAttemptsByQuizAsync(quizId);
                return Ok(attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attempts for quiz {QuizId}", quizId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving attempts");
            }
        }
    }
}