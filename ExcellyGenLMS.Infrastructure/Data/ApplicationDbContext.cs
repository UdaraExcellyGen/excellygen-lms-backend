using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
using ExcellyGenLMS.Core.Entities.ProjectManager;
// Add using statements for your new entities if they are in different namespaces
// e.g., using ExcellyGenLMS.Core.Entities.ProjectManagement;
using System.Text.Json;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata; // Required for ValueComparer

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

        // --- NEW: Project Management Module ---
        public DbSet<Project> Projects { get; set; }
        public DbSet<EmployeeAssignment> EmployeeAssignments { get; set; }
        public DbSet<ProjectTechnology> ProjectTechnologies { get; set; }
        public DbSet<ProjectRole> ProjectRoles { get; set; }
        // Assuming you have an Employee entity. If not, you might need to add:
        // public DbSet<Employee> Employees { get; set; }
        // Or adjust EmployeeAssignment relationship to point to User if applicable.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Existing Configurations ---

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
                .WithMany() // Assuming ForumThread doesn't have a Comments collection property
                .HasForeignKey(c => c.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ThreadComReply entity
            modelBuilder.Entity<ThreadComReply>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ThreadComReply>()
                .HasOne(r => r.Thread)
                .WithMany() // Assuming ForumThread doesn't have a Replies collection property
                .HasForeignKey(r => r.ThreadId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete cycles if replies link directly to thread

            modelBuilder.Entity<ThreadComReply>()
                .HasOne(r => r.Comment)
                .WithMany() // Assuming ThreadComment doesn't have a Replies collection property
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
                .HasDefaultValueSql("GETUTCDATE()"); // Use GETUTCDATE() for UTC time

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
                .HasConversion<string>(); // Assuming Status is an Enum

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

            modelBuilder.Entity<CourseDocument>()
                .Property(e => e.DocumentType)
                .HasConversion<string>(); // Assuming DocumentType is an Enum

            // Configure Lesson-CourseDocument relationship
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Documents)
                .WithOne(d => d.Lesson)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Quiz entity --- UPDATED ---
            modelBuilder.Entity<Quiz>()
                 .HasKey(e => e.QuizId); // Changed from e.Id

            // Configure MCQQuestionOption entity --- UPDATED ---
            modelBuilder.Entity<MCQQuestionOption>()
                 .HasKey(e => e.McqOptionId); // Changed from e.Id

            // Configure QuizBank entity --- UPDATED ---
            modelBuilder.Entity<QuizBank>()
                 .HasKey(e => e.QuizBankId); // Changed from e.Id

            // Configure QuizBankQuestion entity --- UPDATED ---
            modelBuilder.Entity<QuizBankQuestion>()
                 .HasKey(e => e.QuizBankQuestionId); // Changed from e.Id

            // Configure QuizBank-QuizBankQuestion relationship
            modelBuilder.Entity<QuizBank>()
                .HasMany(qb => qb.QuizBankQuestions)
                .WithOne(qbQ => qbQ.QuizBank)
                .HasForeignKey(qbQ => qbQ.QuizBankId) // Ensure this matches the FK property name in QuizBankQuestion
                .OnDelete(DeleteBehavior.Cascade);

            // Configure QuizBankQuestion-MCQQuestionOption relationship
            modelBuilder.Entity<QuizBankQuestion>()
                .HasMany(qbQ => qbQ.MCQQuestionOptions)
                .WithOne(mcqOption => mcqOption.QuizBankQuestion)
                .HasForeignKey(mcqOption => mcqOption.QuizBankQuestionId) // Ensure this matches the FK property name in MCQQuestionOption
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Enrollment entity
            modelBuilder.Entity<Enrollment>()
                 .HasKey(e => e.Id); // Example if Id is the key

            // Configure Certificate entity
            modelBuilder.Entity<Certificate>()
                 .HasKey(e => e.Id); // Example if Id is the key


            // --- NEW: Project Manager configurations ---
            modelBuilder.Entity<Project>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<EmployeeAssignment>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<EmployeeAssignment>()
                .HasOne(e => e.Project)
                .WithMany(p => p.EmployeeAssignments) // Assumes Project has ICollection<EmployeeAssignment> EmployeeAssignments
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a project deletes its assignments

            modelBuilder.Entity<EmployeeAssignment>()
                .HasOne(e => e.Employee) // Assumes EmployeeAssignment has an Employee navigation property and Employee entity exists
                .WithMany() // Assumes Employee entity doesn't have a navigation property back to assignments
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting an Employee if they are assigned to a project

            modelBuilder.Entity<ProjectTechnology>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ProjectTechnology>()
                .HasOne(pt => pt.Project)
                .WithMany(p => p.ProjectTechnologies) // Assumes Project has ICollection<ProjectTechnology> ProjectTechnologies
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a project deletes its technology associations

            modelBuilder.Entity<ProjectTechnology>()
                .HasOne(pt => pt.Technology) // Assumes ProjectTechnology has a Technology navigation property
                .WithMany() // Assumes Technology entity doesn't have a navigation property back to project associations
                .HasForeignKey(pt => pt.TechnologyId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a Technology if it's used in a project

            modelBuilder.Entity<ProjectRole>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<ProjectRole>()
                .HasOne(pr => pr.Project)
                .WithMany(p => p.ProjectRoles) // Assumes Project has ICollection<ProjectRole> ProjectRoles
                .HasForeignKey(pr => pr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting a project deletes its role associations

            // Consider adding configurations for ProjectRole relationship with Employee/User if needed
            // Example:
            // modelBuilder.Entity<ProjectRole>()
            //     .HasOne(pr => pr.Employee) // Assuming ProjectRole has an Employee navigation property
            //     .WithMany() // Assuming Employee doesn't navigate back to ProjectRoles directly
            //     .HasForeignKey(pr => pr.EmployeeId)
            //     .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Employee if assigned a role
        }
    }
}