using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;

using ExcellyGenLMS.Core.Entities.Learner;

using ExcellyGenLMS.Core.Entities.Notifications;
using ExcellyGenLMS.Core.Entities.Course; // Make sure this namespace is included

using System.Collections.Generic;

namespace ExcellyGenLMS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Technology> Technologies { get; set; }

        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ThreadComment> ThreadComments { get; set; }
        public DbSet<ThreadComReply> ThreadComReplies { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<Badge> Badges { get; set; } // Added the Badge DbSet

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<CourseDocument> CourseDocuments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; }
        public DbSet<QuizBank> QuizBanks { get; set; }
        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Certificate> Certificates { get; set; } // Add this line to include Certificate DbSet


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .Property(e => e.Roles)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>()
                );


            modelBuilder.Entity<CourseCategory>()
                .HasKey(e => e.Id);


            modelBuilder.Entity<Technology>()
                .HasKey(e => e.Id);


            modelBuilder.Entity<ForumThread>()
                .HasKey(e => e.ThreadId);


            modelBuilder.Entity<ThreadComment>()
                .HasKey(e => e.Id);


            modelBuilder.Entity<ThreadComment>()
                .HasOne(c => c.Thread)
                .WithMany()
                .HasForeignKey(c => c.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ThreadComReply>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ThreadComReply>()
                .HasOne(r => r.Thread)
                .WithMany()
                .HasForeignKey(r => r.ThreadId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<ThreadComReply>()
                .HasOne(r => r.Comment)
                .WithMany()
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CV entity
            modelBuilder.Entity<CV>()
                .HasKey(e => e.CvId);

            // Configure Badge entity
            modelBuilder.Entity<Badge>()
                .HasKey(e => e.Id);

            // Configure Notification entity
            modelBuilder.Entity<Notification>()
                .HasKey(e => e.NotificationID);

            modelBuilder.Entity<Notification>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Notification>()
                .Property(e => e.IsRead)
                .HasDefaultValue(false);

            modelBuilder.Entity<Notification>()
                .Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Configure Course entity
            modelBuilder.Entity<Course>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Course>()
                .Property(e => e.Status)
                .HasConversion<string>();

            // Configure Course-Lesson relationship
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Lesson entity
            modelBuilder.Entity<Lesson>()
                .HasKey(e => e.Id);

            // Configure CourseDocument entity
            modelBuilder.Entity<CourseDocument>()
                .HasKey(e => e.Id);

            // Configure CourseDocument entity
            modelBuilder.Entity<CourseDocument>()
                .Property(e => e.DocumentType)
                .HasConversion<string>();

            // Configure Lesson-CourseDocument relationship
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Documents)
                .WithOne(d => d.Lesson)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Quiz entity
            modelBuilder.Entity<Quiz>();

            // Configure MCQQuestionOption entity
            modelBuilder.Entity<MCQQuestionOption>();

            // Configure QuizBank entity
            modelBuilder.Entity<QuizBank>();

            // Configure QuizBankQuestion entity
            modelBuilder.Entity<QuizBankQuestion>();

            // Configure QuizBank-QuizBankQuestion relationship
            modelBuilder.Entity<QuizBank>()
                .HasMany(qb => qb.QuizBankQuestions)
                .WithOne(qbQ => qbQ.QuizBank)
                .HasForeignKey(qbQ => qbQ.QuizBankId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure QuizBankQuestion-MCQQuestionOption relationship
            modelBuilder.Entity<QuizBankQuestion>()
                .HasMany(qbQ => qbQ.MCQQuestionOptions)
                .WithOne(mcqOption => mcqOption.QuizBankQuestion)
                .HasForeignKey(mcqOption => mcqOption.QuizBankQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Enrollment entity
            modelBuilder.Entity<Enrollment>();

            // Configure Certificate entity (Add this section)
            modelBuilder.Entity<Certificate>(); // Basic configuration for Certificate entity.

        }
    }
}