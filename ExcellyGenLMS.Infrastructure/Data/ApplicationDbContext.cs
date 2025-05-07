using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner; // Includes Forum entities
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
using System.Text.Json;
using System.Linq; // For SequenceEqual and Aggregate
using System; // For HashCode
using Microsoft.EntityFrameworkCore.ChangeTracking; // For ValueComparer

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
        public DbSet<ForumThread> ForumThreads { get; set; }       // <--- FORUM ENTITY
        public DbSet<ThreadComment> ThreadComments { get; set; }   // <--- FORUM ENTITY
        public DbSet<ThreadComReply> ThreadComReplies { get; set; } // <--- FORUM ENTITY
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
        public DbSet<Certificate> Certificates { get; set; } // This is Course.Certificate

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity (as per your original file)
            modelBuilder.Entity<User>()
                .Property(e => e.Roles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                    v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<User>()
                .Property(e => e.Roles)
                .Metadata.SetValueComparer(
                    new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
                        c => c != null ? c.ToList() : new List<string>()
                    )
                );

            // Configure RefreshToken entity (as per your original file)
            modelBuilder.Entity<RefreshToken>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(e => e.User)
                .WithMany() // No explicit collection property on User for RefreshTokens
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CourseCategory entity (as per your original file)
            modelBuilder.Entity<CourseCategory>()
                .HasKey(e => e.Id);

            // Configure Technology entity (as per your original file)
            modelBuilder.Entity<Technology>()
                .HasKey(e => e.Id);

            // --- MINIMAL FORUM ENTITIES CONFIGURATION ---
            modelBuilder.Entity<ForumThread>(entity =>
            {
                // The primary key is 'Id' (C#) which is mapped to 'ThreadId' (DB) via [Column] attribute
                entity.HasKey(e => e.Id);

                entity.HasOne(ft => ft.Creator) // ForumThread.Creator navigation property
                    .WithMany() // We assume User entity does NOT have a specific collection like 'CreatedForumThreads'
                    .HasForeignKey(ft => ft.CreatorId) // FK property in ForumThread C# entity (maps to 'Creator' column in DB)
                    .OnDelete(DeleteBehavior.Restrict); // Don't delete User if they have created threads
            });

            modelBuilder.Entity<ThreadComment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(tc => tc.Commentor) // ThreadComment.Commentor navigation property
                    .WithMany() // User does not have 'AuthoredComments' collection
                    .HasForeignKey(tc => tc.CommentorId) // FK in ThreadComment C# entity (maps to 'Commentor' column)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(tc => tc.Thread) // ThreadComment.Thread navigation property
                    .WithMany(ft => ft.Comments) // ForumThread.Comments navigation property
                    .HasForeignKey(tc => tc.ThreadId) // FK in ThreadComment C# entity
                    .OnDelete(DeleteBehavior.Cascade); // Delete comments if parent thread is deleted
            });

            modelBuilder.Entity<ThreadComReply>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(tr => tr.Commentor) // ThreadComReply.Commentor navigation property
                    .WithMany() // User does not have 'AuthoredReplies' collection
                    .HasForeignKey(tr => tr.CommentorId) // FK in ThreadComReply C# entity (maps to 'Commentor' column)
                    .OnDelete(DeleteBehavior.Restrict);

                // Removing the direct redundant ThreadComReply -> ForumThread link you had,
                // as our ThreadComReply C# entity does not have a direct ThreadId/Thread navigation.
                // It's linked via its parent ThreadComment.

                entity.HasOne(tr => tr.Comment) // ThreadComReply.Comment navigation property
                    .WithMany(tc => tc.Replies) // ThreadComment.Replies navigation property
                    .HasForeignKey(tr => tr.CommentId) // FK in ThreadComReply C# entity
                    .OnDelete(DeleteBehavior.Cascade); // Delete replies if parent comment is deleted
            });
            // --- END MINIMAL FORUM ENTITIES CONFIGURATION ---

            // Configure CV entity (as per your original file)
            modelBuilder.Entity<CV>()
                .HasKey(e => e.CvId);

            // Configure Badge entity (as per your original file)
            modelBuilder.Entity<Badge>()
                .HasKey(e => e.Id);

            // Configure UserBadge entity (as per your original file)
            modelBuilder.Entity<UserBadge>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserBadge>()
                .HasOne(e => e.User)
                .WithMany() // Assuming no u.UserBadges property in User.cs for now
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBadge>()
                .HasOne(e => e.Badge)
                .WithMany(b => b.UserBadges) // This implies Badge.cs has ICollection<UserBadge> UserBadges
                .HasForeignKey(e => e.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);


            // Configure UserTechnology entity (as per your original file)
            modelBuilder.Entity<UserTechnology>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserTechnology>()
                .HasOne(e => e.User)
                .WithMany() // Assuming no u.UserTechnologies property in User.cs
                .HasForeignKey(e => e.UserId) // Your file used TechnologyId here, assuming UserId
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTechnology>()
                .HasOne(e => e.Technology)
                .WithMany() // Assuming no direct back-navigation from Technology
                .HasForeignKey(e => e.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);


            // Configure Project entity (as per your original file)
            modelBuilder.Entity<Project>()
                .HasKey(e => e.Id);

            // Configure ProjectTechnology entity (as per your original file)
            modelBuilder.Entity<ProjectTechnology>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ProjectTechnology>()
                .HasOne(e => e.Project)
                .WithMany(p => p.Technologies) // Implies Project.cs has ICollection<ProjectTechnology> Technologies
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectTechnology>()
                .HasOne(e => e.Technology)
                .WithMany() // Assuming no direct back-navigation
                .HasForeignKey(e => e.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserProject entity (as per your original file)
            modelBuilder.Entity<UserProject>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserProject>()
                .HasOne(e => e.User)
                .WithMany() // Assuming no u.UserProjects property in User.cs
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProject>()
                .HasOne(e => e.Project)
                .WithMany(p => p.UserProjects) // Implies Project.cs has ICollection<UserProject> UserProjects
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Certification entity (Learner module - from your DbSets)
            modelBuilder.Entity<Certification>() // ExcellyGenLMS.Core.Entities.Learner.Certification
                .HasKey(e => e.Id);

            // Configure UserCertification entity (as per your original file)
            modelBuilder.Entity<UserCertification>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<UserCertification>()
                .HasOne(e => e.User)
                .WithMany() // Assuming no u.UserCertifications property in User.cs
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCertification>()
                .HasOne(e => e.Certification) // Learner.Certification
                .WithMany(c => c.UserCertifications) // Implies Learner.Certification has ICollection<UserCertification>
                .HasForeignKey(e => e.CertificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notification entity (as per your original file)
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

            // Configure Course entity (as per your original file)
            modelBuilder.Entity<Course>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Course>()
                .Property(e => e.Status)
                .HasConversion<string>();

            // Configure Course-Lesson relationship (as per your original file)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Lesson entity (as per your original file)
            modelBuilder.Entity<Lesson>()
                .HasKey(e => e.Id);

            // Configure CourseDocument entity (as per your original file)
            modelBuilder.Entity<CourseDocument>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<CourseDocument>()
                .Property(e => e.DocumentType)
                .HasConversion<string>();

            // Configure Lesson-CourseDocument relationship (as per your original file)
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Documents)
                .WithOne(d => d.Lesson)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Quiz entity (as per your original file)
            modelBuilder.Entity<Quiz>()
               .HasKey(q => q.QuizId);


            // Configure MCQQuestionOption entity (as per your original file)
            modelBuilder.Entity<MCQQuestionOption>()
               .HasKey(m => m.McqOptionId);


            // Configure QuizBank entity (as per your original file)
            modelBuilder.Entity<QuizBank>()
               .HasKey(qb => qb.QuizBankId);


            // Configure QuizBankQuestion entity (as per your original file)
            modelBuilder.Entity<QuizBankQuestion>()
                .HasKey(qbq => qbq.QuizBankQuestionId);


            // Configure QuizBank-QuizBankQuestion relationship (as per your original file)
            modelBuilder.Entity<QuizBank>()
                .HasMany(qb => qb.QuizBankQuestions)
                .WithOne(qbQ => qbQ.QuizBank)
                .HasForeignKey(qbQ => qbQ.QuizBankId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure QuizBankQuestion-MCQQuestionOption relationship (as per your original file)
            modelBuilder.Entity<QuizBankQuestion>()
                .HasMany(qbQ => qbQ.MCQQuestionOptions)
                .WithOne(mcqOption => mcqOption.QuizBankQuestion)
                .HasForeignKey(mcqOption => mcqOption.QuizBankQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Enrollment entity (as per your original file)
            modelBuilder.Entity<Enrollment>()
                .HasKey(e => e.Id);

            // Configure Certificate entity (Course Module - as per your original file)
            modelBuilder.Entity<Certificate>() // ExcellyGenLMS.Core.Entities.Course.Certificate
                .HasKey(c => c.Id);


            // Original Course - Category & Course - Creator relationships from your full CourseCategoryBackendWithAllModels.md
            modelBuilder.Entity<Course>()
               .HasOne(c => c.Category)
               .WithMany(cat => cat.Courses)
               .HasForeignKey(c => c.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Creator)
                .WithMany() // User can create many Courses
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}