// ExcellyGenLMS.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // For ValueComparer
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner; // Includes Forum entities
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
using ExcellyGenLMS.Core.Entities.ProjectManager; // Added PM module import
using ExcellyGenLMS.Core.Enums; // For enums
using System.Text.Json; // For JSON serialization
using System.Collections.Generic; // For List<>
using System.Linq; // For LINQ methods like SequenceEqual, Aggregate
using System; // For HashCode, DateTime

namespace ExcellyGenLMS.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        // --- DbSets ---
        // Auth Module
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        // Admin Module
        public DbSet<CourseCategory> CourseCategories { get; set; } = null!;
        public DbSet<Technology> Technologies { get; set; } = null!;

        // Learner Module
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

        // Notification Module
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Course Module
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<CourseDocument> CourseDocuments { get; set; } = null!;
        public DbSet<CourseTechnology> CourseTechnologies { get; set; } = null!; // Join table for Course<->Technology
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; } = null!;
        public DbSet<QuizBank> QuizBanks { get; set; } = null!;
        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;

        // Project Manager Module
        public DbSet<PMProject> PMProjects { get; set; } = null!;
        public DbSet<PMEmployeeAssignment> PMEmployeeAssignments { get; set; } = null!;
        public DbSet<PMProjectTechnology> PMProjectTechnologies { get; set; } = null!;
        public DbSet<PMProjectRequiredRole> PMProjectRequiredRoles { get; set; } = null!;
        public DbSet<PMRoleDefinition> PMRoleDefinitions { get; set; } = null!;
        public DbSet<PMNotification> PMNotifications { get; set; } = null!;


        // --- Model Configuration ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity with improved JSON serialization for Roles
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

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity => {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.ThumbnailImagePath).HasMaxLength(1024);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);

                // Relationship: Course -> User (Creator)
                entity.HasOne(c => c.Creator)
                      .WithMany()
                      .HasForeignKey(c => c.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship: Course -> CourseCategory
                entity.HasOne(c => c.Category)
                      .WithMany(cc => cc.Courses)
                      .HasForeignKey(c => c.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Lesson entity
            modelBuilder.Entity<Lesson>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LessonName).HasMaxLength(200).IsRequired();

                // Relationship: Lesson -> Course (Many-to-One)
                entity.HasOne(l => l.Course)
                      .WithMany(c => c.Lessons)
                      .HasForeignKey(l => l.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CourseDocument entity
            modelBuilder.Entity<CourseDocument>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1024).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();

                // Relationship: CourseDocument -> Lesson (Many-to-One)
                entity.HasOne(d => d.Lesson)
                     .WithMany(l => l.Documents)
                     .HasForeignKey(d => d.LessonId)
                     .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Technology entity
            modelBuilder.Entity<Technology>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            });

            // Configure CourseTechnology (Join Table for Many-to-Many: Course <-> Technology)
            modelBuilder.Entity<CourseTechnology>(entity =>
            {
                // Define the COMPOSITE primary key
                entity.HasKey(ct => new { ct.CourseId, ct.TechnologyId });

                // Relationship: CourseTechnology -> Course
                entity.HasOne(ct => ct.Course)
                    .WithMany(c => c.CourseTechnologies)
                    .HasForeignKey(ct => ct.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship: CourseTechnology -> Technology
                entity.HasOne(ct => ct.Technology)
                    .WithMany()
                    .HasForeignKey(ct => ct.TechnologyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CourseCategory entity
            modelBuilder.Entity<CourseCategory>().HasKey(e => e.Id);

            // --- FORUM ENTITIES CONFIGURATION ---
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

            // Configure CV entity
            modelBuilder.Entity<CV>().HasKey(e => e.CvId);

            // Configure Badge entity
            modelBuilder.Entity<Badge>().HasKey(e => e.Id);

            // Configure UserBadge entity
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

            // Configure UserTechnology entity
            modelBuilder.Entity<UserTechnology>().HasKey(e => e.Id);
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
            modelBuilder.Entity<Project>().HasKey(e => e.Id);

            // Configure ProjectTechnology entity
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

            // Configure UserProject entity
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

            // Configure Certification entity
            modelBuilder.Entity<Certification>().HasKey(e => e.Id);

            // Configure UserCertification entity
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

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity => {
                entity.HasKey(e => e.NotificationID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // Configure Quiz entities
            modelBuilder.Entity<Quiz>().HasKey(q => q.QuizId);
            modelBuilder.Entity<MCQQuestionOption>().HasKey(m => m.McqOptionId);
            modelBuilder.Entity<QuizBank>().HasKey(qb => qb.QuizBankId);
            modelBuilder.Entity<QuizBankQuestion>().HasKey(qbq => qbq.QuizBankQuestionId);

            // Configure Quiz relationships
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

            // Configure Enrollment entity
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

            // Configure Certificate entity
            modelBuilder.Entity<Certificate>().HasKey(c => c.Id);

            // ========= Project Manager Module Configuration ==========

            // Configure PMProject entity
            modelBuilder.Entity<PMProject>().HasKey(e => e.Id);
            modelBuilder.Entity<PMProject>()
                .HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PMEmployeeAssignment entity
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

            // Configure PMProjectTechnology entity
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

            // Configure PMProjectRequiredRole entity
            modelBuilder.Entity<PMProjectRequiredRole>().HasKey(e => e.Id);
            modelBuilder.Entity<PMProjectRequiredRole>()
                .HasOne(e => e.Project)
                .WithMany(p => p.RequiredRoles)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PMRoleDefinition entity
            modelBuilder.Entity<PMRoleDefinition>().HasKey(e => e.Id);

            // Configure PMNotification entity
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
        }
    }
}