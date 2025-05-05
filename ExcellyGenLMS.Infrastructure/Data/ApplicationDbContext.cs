using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
using System.Text.Json;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ExcellyGenLMS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        // Auth Module
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // Admin Module
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Technology> Technologies { get; set; }

        // Learner Module
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ThreadComment> ThreadComments { get; set; }
        public DbSet<ThreadComReply> ThreadComReplies { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserBadge> UserBadges { get; set; }
        public DbSet<UserTechnology> UserTechnologies { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectTechnology> ProjectTechnologies { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<UserCertification> UserCertifications { get; set; }

        // Notification Module
        public DbSet<Notification> Notifications { get; set; }

        // Course Module
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<CourseDocument> CourseDocuments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; }
        public DbSet<QuizBank> QuizBanks { get; set; }
        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Certificate> Certificates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity with improved JSON serialization
            modelBuilder.Entity<User>()
                .Property(e => e.Roles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                    v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)");

            // Add a value comparer for Roles collection to fix EF Core warning
            modelBuilder.Entity<User>()
                .Property(e => e.Roles)
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                        c => c != null ? c.ToList() : new List<string>()
                    )
                );

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CourseCategory entity
            modelBuilder.Entity<CourseCategory>()
                .HasKey(e => e.Id);

            // Configure Technology entity
            modelBuilder.Entity<Technology>()
                .HasKey(e => e.Id);

            // Configure ForumThread entity
            modelBuilder.Entity<ForumThread>()
                .HasKey(e => e.ThreadId);

            // Configure ThreadComment entity
            modelBuilder.Entity<ThreadComment>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ThreadComment>()
                .HasOne(c => c.Thread)
                .WithMany()
                .HasForeignKey(c => c.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ThreadComReply entity
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

            // Configure UserBadge entity
            modelBuilder.Entity<UserBadge>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserBadge>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasOne(e => e.Badge)
                .WithMany(b => b.UserBadges)
                .HasForeignKey(e => e.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserTechnology entity
            modelBuilder.Entity<UserTechnology>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserTechnology>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTechnology>()
                .HasOne(e => e.Technology)
                .WithMany()
                .HasForeignKey(e => e.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Project entity
            modelBuilder.Entity<Project>()
                .HasKey(e => e.Id);

            // Configure ProjectTechnology entity
            modelBuilder.Entity<ProjectTechnology>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ProjectTechnology>()
                .HasOne(e => e.Project)
                .WithMany(p => p.Technologies)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectTechnology>()
                .HasOne(e => e.Technology)
                .WithMany()
                .HasForeignKey(e => e.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserProject entity
            modelBuilder.Entity<UserProject>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserProject>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProject>()
                .HasOne(e => e.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Certification entity
            modelBuilder.Entity<Certification>()
                .HasKey(e => e.Id);

            // Configure UserCertification entity
            modelBuilder.Entity<UserCertification>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserCertification>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCertification>()
                .HasOne(e => e.Certification)
                .WithMany(c => c.UserCertifications)
                .HasForeignKey(e => e.CertificationId)
                .OnDelete(DeleteBehavior.Cascade);

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

            // Configure Certificate entity
            modelBuilder.Entity<Certificate>();
        }
    }
}