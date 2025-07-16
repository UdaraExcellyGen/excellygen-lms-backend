using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
using ExcellyGenLMS.Core.Entities.ProjectManager;
using ExcellyGenLMS.Core.Enums;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ExcellyGenLMS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<CourseCategory> CourseCategories { get; set; } = null!;
        public DbSet<Technology> Technologies { get; set; } = null!;
        public DbSet<ForumThread> ForumThreads { get; set; } = null!;
        public DbSet<ThreadComment> ThreadComments { get; set; } = null!;
        public DbSet<ThreadComReply> ThreadComReplies { get; set; } = null!;
        public DbSet<CV> CVs { get; set; } = null!;
        public DbSet<Badge> Badges { get; set; } = null!;
        public DbSet<UserBadge> UserBadges { get; set; } = null!;
        public DbSet<UserTechnology> UserTechnologies { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectTechnology> ProjectTechnologies { get; set; } = null!;
        public DbSet<UserProject> UserProjects { get; set; } = null!;
        public DbSet<Certification> Certifications { get; set; } = null!;
        public DbSet<UserCertification> UserCertifications { get; set; } = null!;
        public DbSet<LearnerNotification> LearnerNotifications { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<CourseDocument> CourseDocuments { get; set; } = null!;
        public DbSet<CourseTechnology> CourseTechnologies { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; } = null!;
        public DbSet<QuizBank> QuizBanks { get; set; } = null!;
        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
        public DbSet<ExternalCertificate> ExternalCertificates { get; set; } = null!;
        public DbSet<LessonProgress> LessonProgress { get; set; } = null!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
        public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = null!;
        public DbSet<PMProject> PMProjects { get; set; } = null!;
        public DbSet<PMEmployeeAssignment> PMEmployeeAssignments { get; set; } = null!;
        public DbSet<PMProjectTechnology> PMProjectTechnologies { get; set; } = null!;
        public DbSet<PMProjectRequiredRole> PMProjectRequiredRoles { get; set; } = null!;
        public DbSet<PMRoleDefinition> PMRoleDefinitions { get; set; } = null!;
        public DbSet<PMNotification> PMNotifications { get; set; } = null!;
        public DbSet<UserActivityLog> UserActivityLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Roles)
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                          v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>()
                      )
                      .HasColumnType("nvarchar(max)")
                      .Metadata.SetValueComparer(
                          new ValueComparer<List<string>>(
                              (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                              c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c => c == null ? new List<string>() : c.ToList()
                          )
                      );
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.ThumbnailImagePath).HasMaxLength(1024);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.HasOne(c => c.Creator)
                      .WithMany()
                      .HasForeignKey(c => c.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Category)
                      .WithMany(cc => cc.Courses)
                      .HasForeignKey(c => c.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LessonName).HasMaxLength(200).IsRequired();
                entity.HasOne(l => l.Course)
                      .WithMany(c => c.Lessons)
                      .HasForeignKey(l => l.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CourseDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1024).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.HasOne(d => d.Lesson)
                     .WithMany(l => l.Documents)
                     .HasForeignKey(d => d.LessonId)
                     .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Technology>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<CourseTechnology>(entity =>
            {
                entity.HasKey(ct => new { ct.CourseId, ct.TechnologyId });
                entity.HasOne(ct => ct.Course)
                    .WithMany(c => c.CourseTechnologies)
                    .HasForeignKey(ct => ct.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ct => ct.Technology)
                    .WithMany()
                    .HasForeignKey(ct => ct.TechnologyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CourseCategory>().HasKey(e => e.Id);

            modelBuilder.Entity<ForumThread>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(ft => ft.Creator)
                    .WithMany()
                    .HasForeignKey(ft => ft.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ThreadComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(tc => tc.Commentor)
                    .WithMany()
                    .HasForeignKey(tc => tc.CommentorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(tc => tc.Thread)
                    .WithMany(ft => ft.Comments)
                    .HasForeignKey(tc => tc.ThreadId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ThreadComReply>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(tr => tr.Commentor)
                    .WithMany()
                    .HasForeignKey(tr => tr.CommentorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(tr => tr.Comment)
                    .WithMany(tc => tc.Replies)
                    .HasForeignKey(tr => tr.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CV>().HasKey(e => e.CvId);

            modelBuilder.Entity<Badge>().HasKey(e => e.Id);
            modelBuilder.Entity<UserBadge>().HasKey(e => e.Id);
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

            modelBuilder.Entity<UserTechnology>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Technology)
                    .WithMany()
                    .HasForeignKey(e => e.TechnologyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Project>().HasKey(e => e.Id);

            modelBuilder.Entity<ProjectTechnology>().HasKey(e => e.Id);
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

            modelBuilder.Entity<UserProject>().HasKey(e => e.Id);
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

            modelBuilder.Entity<Certification>().HasKey(e => e.Id);

            modelBuilder.Entity<UserCertification>().HasKey(e => e.Id);
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

            modelBuilder.Entity<LearnerNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProjectName).HasMaxLength(200);
                entity.Property(e => e.AssignerName).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.UserId, e.IsRead });
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            modelBuilder.Entity<Quiz>().HasKey(q => q.QuizId);
            modelBuilder.Entity<MCQQuestionOption>().HasKey(m => m.McqOptionId);
            modelBuilder.Entity<QuizBank>().HasKey(qb => qb.QuizBankId);
            modelBuilder.Entity<QuizBankQuestion>().HasKey(qbq => qbq.QuizBankQuestionId);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Lesson)
                .WithMany()
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.QuizBank)
                .WithMany(qb => qb.Quizzes)
                .HasForeignKey(q => q.QuizBankId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<QuizBank>()
                .HasMany(qb => qb.QuizBankQuestions)
                .WithOne(qbQ => qbQ.QuizBank)
                .HasForeignKey(qbQ => qbQ.QuizBankId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<QuizBankQuestion>()
                .HasMany(qbQ => qbQ.MCQQuestionOptions)
                .WithOne(mcqOption => mcqOption.QuizBankQuestion)
                .HasForeignKey(mcqOption => mcqOption.QuizBankQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttempt>(entity =>
            {
                entity.HasKey(e => e.QuizAttemptId);
                entity.HasOne(e => e.Quiz)
                      .WithMany()
                      .HasForeignKey(e => e.QuizId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QuizAttemptAnswer>(entity =>
            {
                entity.HasKey(e => e.QuizAttemptAnswerId);
                entity.HasOne(e => e.QuizAttempt)
                      .WithMany(a => a.Answers)
                      .HasForeignKey(e => e.QuizAttemptId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuizBankQuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedOption)
                      .WithMany()
                      .HasForeignKey(e => e.SelectedOptionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Enrollment>().HasKey(e => e.Id);
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certificate>().HasKey(c => c.Id);
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExternalCertificate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CompletionDate);
                entity.HasIndex(e => e.Platform);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Issuer).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Platform).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CredentialUrl).HasMaxLength(1000);
                entity.Property(e => e.CredentialId).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            });

            modelBuilder.Entity<LessonProgress>(entity =>
            {
                entity.HasKey(lp => lp.Id);
                entity.HasOne(lp => lp.User)
                      .WithMany()
                      .HasForeignKey(lp => lp.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(lp => lp.Lesson)
                      .WithMany()
                      .HasForeignKey(lp => lp.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(lp => new { lp.UserId, lp.LessonId }).IsUnique();
            });

            modelBuilder.Entity<PMProject>().HasKey(e => e.Id);
            modelBuilder.Entity<PMProject>()
                .HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PMEmployeeAssignment>().HasKey(e => e.Id);
            modelBuilder.Entity<PMEmployeeAssignment>()
                .HasOne(e => e.Project)
                .WithMany(p => p.EmployeeAssignments)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PMEmployeeAssignment>()
                .HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PMProjectTechnology>().HasKey(e => e.Id);
            modelBuilder.Entity<PMProjectTechnology>()
                .HasOne(e => e.Project)
                .WithMany(p => p.Technologies)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PMProjectTechnology>()
                .HasOne(e => e.Technology)
                .WithMany()
                .HasForeignKey(e => e.TechnologyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PMProjectRequiredRole>().HasKey(e => e.Id);
            modelBuilder.Entity<PMProjectRequiredRole>()
                .HasOne(e => e.Project)
                .WithMany(p => p.RequiredRoles)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PMRoleDefinition>().HasKey(e => e.Id);

            modelBuilder.Entity<PMNotification>().HasKey(e => e.Id);
            modelBuilder.Entity<PMNotification>()
                .HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<PMNotification>()
                .HasOne(e => e.Recipient)
                .WithMany()
                .HasForeignKey(e => e.RecipientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.UserId, e.ActivityTimestamp });
            });
        }
    }
}