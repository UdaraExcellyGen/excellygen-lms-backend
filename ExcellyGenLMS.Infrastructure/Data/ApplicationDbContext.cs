// ExcellyGenLMS.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // For ValueComparer
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner; // Includes Forum entities
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
<<<<<<< HEAD
using ExcellyGenLMS.Core.Enums; // For enums
using System.Text.Json; // For JSON serialization
using System.Collections.Generic; // For List<>
using System.Linq; // For LINQ methods like SequenceEqual, Aggregate
using System; // For HashCode, DateTime
=======
using ExcellyGenLMS.Core.Entities.ProjectManager; // Added PM module import
using System.Text.Json;
using System.Linq; // For SequenceEqual and Aggregate
using System; // For HashCode
using Microsoft.EntityFrameworkCore.ChangeTracking; // For ValueComparer
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93

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
        public DbSet<User> Users { get; set; } = null!; // Initialize with null-forgiving operator or `= default!;`
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        // Admin Module
        public DbSet<CourseCategory> CourseCategories { get; set; } = null!;
        public DbSet<Technology> Technologies { get; set; } = null!;

        // Learner Module
<<<<<<< HEAD
        public DbSet<ForumThread> ForumThreads { get; set; } = null!;
        public DbSet<ThreadComment> ThreadComments { get; set; } = null!;
        public DbSet<ThreadComReply> ThreadComReplies { get; set; } = null!;
        public DbSet<CV> CVs { get; set; } = null!;
        public DbSet<Badge> Badges { get; set; } = null!;
=======
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
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93

        // Notification Module
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Course Module
<<<<<<< HEAD
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<CourseDocument> CourseDocuments { get; set; } = null!;
        public DbSet<CourseTechnology> CourseTechnologies { get; set; } = null!; // Join table for Course<->Technology
        // Quiz related - keep if still needed eventually
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; } = null!;
        public DbSet<QuizBank> QuizBanks { get; set; } = null!;
        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; } = null!;
        // Other course related
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;
=======
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<CourseDocument> CourseDocuments { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; }
        public DbSet<QuizBank> QuizBanks { get; set; }
        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Certificate> Certificates { get; set; } // This is Course.Certificate

        // Project Manager Module
        public DbSet<PMProject> PMProjects { get; set; }
        public DbSet<PMEmployeeAssignment> PMEmployeeAssignments { get; set; }
        public DbSet<PMProjectTechnology> PMProjectTechnologies { get; set; }
        public DbSet<PMProjectRequiredRole> PMProjectRequiredRoles { get; set; }
        public DbSet<PMRoleDefinition> PMRoleDefinitions { get; set; }
        public DbSet<PMNotification> PMNotifications { get; set; }
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93


        // --- Model Configuration ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

<<<<<<< HEAD
            // Configure User entity with improved JSON serialization for Roles
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Roles)
                      .HasConversion(
                          // Serialize List<string> to JSON string
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          // Deserialize JSON string back to List<string>
                          v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                      )
                      .HasColumnType("nvarchar(max)") // Ensure storage can handle long JSON strings
                      .Metadata.SetValueComparer( // Add value comparer for proper change tracking of collection
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
                entity.HasOne(e => e.User) // Relationship to User
                      .WithMany()          // Assuming User doesn't have a direct navigation property back to RefreshTokens
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete refresh tokens if user is deleted
            });

            // --- Course Module Configuration ---

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Store enum as string with max length for db consistency
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.ThumbnailImagePath).HasMaxLength(1024); // Set MaxLength for path/URL
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000); // Match DTO if applicable

                // Relationship: Course -> User (Creator) - Define behavior on user deletion
                entity.HasOne(c => c.Creator)
                      .WithMany() // One user can create many courses
                      .HasForeignKey(c => c.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict); // PREVENT deleting a User if they have created courses. Consider SetNull if CreatorId is nullable.

                // Relationship: Course -> CourseCategory - Define behavior on category deletion
                entity.HasOne(c => c.Category)
                      .WithMany(cc => cc.Courses) // One category can have many courses
                      .HasForeignKey(c => c.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // PREVENT deleting a Category if courses are assigned to it.
            });

            // Configure Lesson entity
            modelBuilder.Entity<Lesson>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LessonName).HasMaxLength(200).IsRequired();

                // Relationship: Lesson -> Course (Many-to-One)
                entity.HasOne(l => l.Course)         // Each Lesson belongs to one Course
                      .WithMany(c => c.Lessons)      // Course has many Lessons
                      .HasForeignKey(l => l.CourseId) // Foreign key property
                      .OnDelete(DeleteBehavior.Cascade); // *** CRITICAL: If a Course is deleted, all its Lessons are also deleted.
            });

            // Configure CourseDocument entity
            modelBuilder.Entity<CourseDocument>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(50).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(1024).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();

                // Relationship: CourseDocument -> Lesson (Many-to-One)
                entity.HasOne(d => d.Lesson)       // Each Document belongs to one Lesson
                     .WithMany(l => l.Documents)   // Lesson has many Documents
                     .HasForeignKey(d => d.LessonId) // Foreign key property
                     .OnDelete(DeleteBehavior.Cascade); // *** CRITICAL: If a Lesson is deleted, all its Documents are also deleted.
            });

            // Configure Technology entity (Ensure Key and Properties match Core/Entities/Admin/Technology.cs)
            modelBuilder.Entity<Technology>(entity => {
                entity.HasKey(e => e.Id); // Assuming Id is string and primary key
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            });


            // Configure CourseTechnology (Join Table for Many-to-Many: Course <-> Technology)
            modelBuilder.Entity<CourseTechnology>(entity =>
            {
                // Define the COMPOSITE primary key
                entity.HasKey(ct => new { ct.CourseId, ct.TechnologyId });

                // Relationship: CourseTechnology -> Course
                entity.HasOne(ct => ct.Course)                // One side of the join
                    .WithMany(c => c.CourseTechnologies)     // Navigation property back in Course entity
                    .HasForeignKey(ct => ct.CourseId)          // Foreign key in the join table
                    .OnDelete(DeleteBehavior.Cascade);        // *** If a Course is deleted, its entries in CourseTechnologies are removed.

                // Relationship: CourseTechnology -> Technology
                entity.HasOne(ct => ct.Technology)            // Other side of the join
                    .WithMany()                              // No navigation property needed back from Technology in this setup
                    .HasForeignKey(ct => ct.TechnologyId)      // Foreign key in the join table
                    .OnDelete(DeleteBehavior.Cascade);        // *** If a Technology is deleted, its links to Courses are removed. Consider Restrict if Tech deletion should be blocked when used.
            });


            // --- Other Module Configurations ---
            // Keep existing configurations for Admin, Learner, Notification, Quiz etc.
            modelBuilder.Entity<CourseCategory>().HasKey(e => e.Id);
            // ... ensure all configurations from your original file are here ...
            modelBuilder.Entity<ForumThread>().HasKey(e => e.ThreadId);
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
                .OnDelete(DeleteBehavior.NoAction); // Prevent cycles if needed
            modelBuilder.Entity<ThreadComReply>()
                .HasOne(r => r.Comment)
                .WithMany()
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CV>().HasKey(e => e.CvId);
            modelBuilder.Entity<Badge>().HasKey(e => e.Id);

            // Notification defaults
            modelBuilder.Entity<Notification>(entity => {
                entity.HasKey(e => e.NotificationID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });


            // Configure Quiz system entities (keep if relevant)
            modelBuilder.Entity<Quiz>();
            modelBuilder.Entity<MCQQuestionOption>();
            modelBuilder.Entity<QuizBank>();
            modelBuilder.Entity<QuizBankQuestion>();

            // Quiz Relationships (keep if relevant)
=======
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
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
            modelBuilder.Entity<QuizBank>()
                .HasMany(qb => qb.QuizBankQuestions)
                .WithOne(qbQ => qbQ.QuizBank)
                .HasForeignKey(qbQ => qbQ.QuizBankId)
                .OnDelete(DeleteBehavior.Cascade);

<<<<<<< HEAD
=======
            // Configure QuizBankQuestion-MCQQuestionOption relationship (as per your original file)
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
            modelBuilder.Entity<QuizBankQuestion>()
                .HasMany(qbQ => qbQ.MCQQuestionOptions)
                .WithOne(mcqOption => mcqOption.QuizBankQuestion)
                .HasForeignKey(mcqOption => mcqOption.QuizBankQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

<<<<<<< HEAD
            modelBuilder.Entity<Enrollment>();
            modelBuilder.Entity<Certificate>();

=======
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
                
            // ========= Project Manager Module Configuration ==========

            // Configure PMProject entity
            modelBuilder.Entity<PMProject>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<PMProject>()
                .HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PMEmployeeAssignment entity
            modelBuilder.Entity<PMEmployeeAssignment>()
                .HasKey(e => e.Id);

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
            modelBuilder.Entity<PMProjectTechnology>()
                .HasKey(e => e.Id);

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
            modelBuilder.Entity<PMProjectRequiredRole>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<PMProjectRequiredRole>()
                .HasOne(e => e.Project)
                .WithMany(p => p.RequiredRoles)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PMRoleDefinition entity
            modelBuilder.Entity<PMRoleDefinition>()
                .HasKey(e => e.Id);

            // Configure PMNotification entity
            modelBuilder.Entity<PMNotification>()
                .HasKey(e => e.Id);

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
>>>>>>> 1d0db7143db1398cf4c8b7ab2577208b67f84a93
        }
    }
}