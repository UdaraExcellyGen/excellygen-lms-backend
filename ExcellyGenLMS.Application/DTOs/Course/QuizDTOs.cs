// ExcellyGenLMS.Application/DTOs/Course/QuizDTOs.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExcellyGenLMS.Application.DTOs.Course
{
    // DTOs for retrieving data
    public class QuizDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int TimeLimitMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int QuizSize { get; set; }
        public int QuizBankId { get; set; }
        public int LessonId { get; set; }
        public string LessonName { get; set; } = string.Empty;
    }

    public class QuizDetailDto : QuizDto
    {
        public List<QuizBankQuestionDto> Questions { get; set; } = new List<QuizBankQuestionDto>();
    }

    public class QuizBankDto
    {
        public int QuizBankId { get; set; }
        public int QuizBankSize { get; set; }
        public List<QuizBankQuestionDto> Questions { get; set; } = new List<QuizBankQuestionDto>();
    }

    public class QuizBankQuestionDto
    {
        public int QuizBankQuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "mcq";
        public int? QuestionBankOrder { get; set; }
        public List<MCQQuestionOptionDto> Options { get; set; } = new List<MCQQuestionOptionDto>();
    }

    public class MCQQuestionOptionDto
    {
        public int McqOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class LearnerQuizQuestionDto
    {
        public int QuizBankQuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "mcq";
        public List<LearnerMCQOptionDto> Options { get; set; } = new List<LearnerMCQOptionDto>();
    }

    public class LearnerMCQOptionDto
    {
        public int McqOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        // IsCorrect is deliberately omitted for learner view
    }

    // DTOs for creating/updating data
    public class CreateQuizDto
    {
        [Required]
        [MaxLength(200)]
        public string QuizTitle { get; set; } = string.Empty;

        [Required]
        [Range(1, 180)]
        public int TimeLimitMinutes { get; set; }

        [Required]
        [Range(1, 100)]
        public int QuizSize { get; set; }

        [Required]
        public int LessonId { get; set; }
    }

    public class CreateQuizBankDto
    {
        [Required]
        [Range(5, 1000)]
        public int QuizBankSize { get; set; }

        public List<CreateQuizBankQuestionDto>? Questions { get; set; }
    }

    public class CreateQuizBankQuestionDto
    {
        [Required]
        public string QuestionContent { get; set; } = string.Empty;

        [Required]
        public List<CreateMCQOptionDto> Options { get; set; } = new List<CreateMCQOptionDto>();

        public int? QuestionBankOrder { get; set; }
    }

    public class CreateMCQOptionDto
    {
        [Required]
        public string OptionText { get; set; } = string.Empty;

        [Required]
        public bool IsCorrect { get; set; }
    }

    public class UpdateQuizBankQuestionDto
    {
        public string? QuestionContent { get; set; }
        public List<CreateMCQOptionDto>? Options { get; set; }
        public int? QuestionBankOrder { get; set; }
    }

    // DTOs for quiz attempts
    public class QuizAttemptDto
    {
        public int QuizAttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? CompletionTime { get; set; }
        public int? Score { get; set; }
        public bool IsCompleted { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }

    public class QuizAttemptDetailDto : QuizAttemptDto
    {
        public List<QuizAttemptAnswerDto> Answers { get; set; } = new List<QuizAttemptAnswerDto>();
    }

    public class QuizAttemptAnswerDto
    {
        public int QuizAttemptAnswerId { get; set; }
        public int QuizBankQuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public int? SelectedOptionId { get; set; }
        public string SelectedOptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string CorrectOptionText { get; set; } = string.Empty;
    }

    public class StartQuizAttemptDto
    {
        [Required]
        public int QuizId { get; set; }
    }

    public class SubmitQuizAnswerDto
    {
        [Required]
        public int QuizAttemptId { get; set; }

        [Required]
        public int QuizBankQuestionId { get; set; }

        [Required]
        public int SelectedOptionId { get; set; }
    }

    public class CompleteQuizAttemptDto
    {
        [Required]
        public int QuizAttemptId { get; set; }
    }
}