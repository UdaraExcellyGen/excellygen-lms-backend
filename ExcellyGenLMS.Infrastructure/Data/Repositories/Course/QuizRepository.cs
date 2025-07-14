using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Interfaces.Repositories.Course;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExcellyGenLMS.Infrastructure.Data;

namespace ExcellyGenLMS.Infrastructure.Data.Repositories.Course
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UpdateQuizBankAsync(QuizBank quizBank)
        {
            try
            {
                var existingQuizBank = await _context.QuizBanks
                    .FirstOrDefaultAsync(qb => qb.QuizBankId == quizBank.QuizBankId);

                if (existingQuizBank == null)
                {
                    throw new ArgumentException($"QuizBank with ID {quizBank.QuizBankId} not found.");
                }

                existingQuizBank.QuizBankSize = quizBank.QuizBankSize;

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Quiz?> GetQuizByIdAsync(int quizId)
        {
            return await _context.Quizzes
                                 .Include(q => q.Lesson)
                                 .Include(q => q.QuizBank)
                                 .FirstOrDefaultAsync(q => q.QuizId == quizId);
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByLessonIdAsync(int lessonId)
        {
            return await _context.Quizzes
                                 .Where(q => q.LessonId == lessonId)
                                 .Include(q => q.Lesson)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByCourseIdAsync(int courseId)
        {
            // FIX: Added a null check for q.Lesson before accessing its properties
            return await _context.Quizzes
                .Where(q => q.Lesson != null && q.Lesson.CourseId == courseId)
                .Include(q => q.Lesson)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizByLessonIdAsync(int lessonId)
        {
            return await _context.Quizzes
                                 .Where(q => q.LessonId == lessonId)
                                 .Include(q => q.Lesson)
                                 .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesByLessonIdsAsync(List<int> lessonIds)
        {
            return await _context.Quizzes
                                 .Where(q => lessonIds.Contains(q.LessonId))
                                 .Include(q => q.Lesson)
                                 .ToListAsync();
        }

        public async Task<Quiz> CreateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _context.Entry(quiz).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"Quiz with ID {quizId} not found for deletion.");
            }
        }

        public async Task<QuizBank?> GetQuizBankByIdAsync(int quizBankId)
        {
            return await _context.QuizBanks
                                 .Include(qb => qb.QuizBankQuestions)
                                     .ThenInclude(qbq => qbq.MCQQuestionOptions)
                                 .FirstOrDefaultAsync(qb => qb.QuizBankId == quizBankId);
        }

        public async Task<QuizBank> CreateQuizBankAsync(QuizBank quizBank)
        {
            _context.QuizBanks.Add(quizBank);
            await _context.SaveChangesAsync();
            return quizBank;
        }

        public async Task<QuizBank> GetOrCreateQuizBankForLessonAsync(int lessonId)
        {
            var existingQuiz = await _context.Quizzes
                                             .Include(q => q.QuizBank)
                                             .FirstOrDefaultAsync(q => q.LessonId == lessonId);

            if (existingQuiz?.QuizBank != null)
            {
                return existingQuiz.QuizBank;
            }

            var newQuizBank = new QuizBank { QuizBankSize = 0 };
            _context.QuizBanks.Add(newQuizBank);
            await _context.SaveChangesAsync();
            return newQuizBank;
        }

        public async Task<QuizBankQuestion?> GetQuizBankQuestionByIdAsync(int questionId)
        {
            return await _context.QuizBankQuestions
                                 .Include(qbq => qbq.MCQQuestionOptions)
                                 .FirstOrDefaultAsync(qbq => qbq.QuizBankQuestionId == questionId);
        }

        public async Task<IEnumerable<QuizBankQuestion>> GetQuestionsForQuizBankAsync(int quizBankId)
        {
            return await _context.QuizBankQuestions
                                 .Where(qbq => qbq.QuizBankId == quizBankId)
                                 .Include(qbq => qbq.MCQQuestionOptions)
                                 .OrderBy(qbq => qbq.QuestionBankOrder)
                                 .ToListAsync();
        }

        public async Task<QuizBankQuestion> AddQuestionToQuizBankAsync(QuizBankQuestion question)
        {
            _context.QuizBankQuestions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task UpdateQuizBankQuestionAsync(QuizBankQuestion question)
        {
            _context.Entry(question).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizBankQuestionAsync(int questionId)
        {
            var question = await _context.QuizBankQuestions.FindAsync(questionId);
            if (question != null)
            {
                _context.QuizBankQuestions.Remove(question);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException($"Question with ID {questionId} not found for deletion.");
            }
        }

        public async Task<MCQQuestionOption?> GetMCQOptionByIdAsync(int optionId)
        {
            return await _context.MCQQuestionOptions.FindAsync(optionId);
        }

        public async Task<IEnumerable<MCQQuestionOption>> GetOptionsForQuestionAsync(int questionId)
        {
            return await _context.MCQQuestionOptions
                                 .Where(o => o.QuizBankQuestionId == questionId)
                                 .ToListAsync();
        }

        public async Task AddOptionToQuestionAsync(MCQQuestionOption option)
        {
            _context.MCQQuestionOptions.Add(option);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOptionAsync(MCQQuestionOption option)
        {
            _context.Entry(option).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsOptionUsedInAttemptsAsync(int optionId)
        {
            return await _context.QuizAttemptAnswers
                                .AnyAsync(qaa => qaa.SelectedOptionId == optionId);
        }

        public async Task DeleteOptionAsync(int optionId)
        {
            try
            {
                var option = await _context.MCQQuestionOptions.FindAsync(optionId);
                if (option != null)
                {
                    bool isUsed = await IsOptionUsedInAttemptsAsync(optionId);
                    if (!isUsed)
                    {
                        _context.MCQQuestionOptions.Remove(option);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    throw new ArgumentException($"Option with ID {optionId} not found for deletion.");
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("REFERENCE constraint") == true)
            {
                Console.WriteLine($"Cannot delete option {optionId} because it's referenced in quiz attempts");
            }
        }

        public async Task<IEnumerable<QuizBankQuestion>> GetRandomQuestionsForQuizAsync(int quizId, int count)
        {
            try
            {
                Console.WriteLine($"=== GetRandomQuestionsForQuizAsync called for Quiz ID: {quizId}, Count: {count} ===");

                var quiz = await _context.Quizzes
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(q => q.QuizId == quizId);

                if (quiz == null)
                {
                    Console.WriteLine($"❌ Quiz with ID {quizId} not found");
                    return new List<QuizBankQuestion>();
                }

                Console.WriteLine($"✅ Found Quiz: ID={quiz.QuizId}, Title='{quiz.QuizTitle}', QuizBankId={quiz.QuizBankId}");

                var quizBank = await _context.QuizBanks
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(qb => qb.QuizBankId == quiz.QuizBankId);

                if (quizBank == null)
                {
                    Console.WriteLine($"❌ QuizBank with ID {quiz.QuizBankId} not found");
                    return new List<QuizBankQuestion>();
                }

                Console.WriteLine($"✅ Found QuizBank: ID={quizBank.QuizBankId}, Size={quizBank.QuizBankSize}");

                var questions = await _context.QuizBankQuestions
                                              .Where(qbq => qbq.QuizBankId == quiz.QuizBankId)
                                              .Include(qbq => qbq.MCQQuestionOptions)
                                              .ToListAsync();

                Console.WriteLine($"📊 Found {questions.Count} questions in QuizBank {quiz.QuizBankId}");

                if (questions.Count == 0)
                {
                    Console.WriteLine($"❌ No questions found in QuizBank {quiz.QuizBankId} for Quiz {quizId}");
                    Console.WriteLine("🔍 Debug: Let's check all quiz banks and questions in the database...");

                    var allQuizBanks = await _context.QuizBanks.ToListAsync();
                    Console.WriteLine($"📋 Total QuizBanks in database: {allQuizBanks.Count}");
                    foreach (var qb in allQuizBanks)
                    {
                        var questionCount = await _context.QuizBankQuestions.CountAsync(q => q.QuizBankId == qb.QuizBankId);
                        Console.WriteLine($"   QuizBank ID {qb.QuizBankId}: {questionCount} questions");
                    }

                    return new List<QuizBankQuestion>();
                }

                foreach (var q in questions)
                {
                    Console.WriteLine($"   Question {q.QuizBankQuestionId}: '{q.QuestionContent}' ({q.MCQQuestionOptions.Count} options)");
                }

                var selectedQuestions = questions
                                       .OrderBy(q => Guid.NewGuid())
                                       .Take(count)
                                       .ToList();

                Console.WriteLine($"✅ Selected {selectedQuestions.Count} random questions for Quiz {quizId}");

                return selectedQuestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error in GetRandomQuestionsForQuizAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> HasQuizForLessonAsync(int lessonId)
        {
            return await _context.Quizzes.AnyAsync(q => q.LessonId == lessonId);
        }
    }
}