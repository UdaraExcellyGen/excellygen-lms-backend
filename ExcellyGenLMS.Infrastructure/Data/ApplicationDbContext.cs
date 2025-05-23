//using Microsoft.EntityFrameworkCore;
//using ExcellyGenLMS.Core.Entities.Auth;
//using ExcellyGenLMS.Core.Entities.Admin;
//using ExcellyGenLMS.Core.Entities.Learner;
//using ExcellyGenLMS.Core.Entities.Course;
//using ExcellyGenLMS.Core.Entities.Notifications;
//using System.Text.Json;
//using System.Linq;
//using System;
//using Microsoft.EntityFrameworkCore.ChangeTracking;

//using ExcellyGenLMS.Core.Enums;

//namespace ExcellyGenLMS.Infrastructure.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//        : base(options)
//        {
//        }

//        // Auth Module
//        public DbSet<User> Users { get; set; }
//        public DbSet<RefreshToken> RefreshTokens { get; set; }

//        // Admin Module
//        public DbSet<CourseCategory> CourseCategories { get; set; }
//        public DbSet<Technology> Technologies { get; set; }

//        // Learner Module
//        public DbSet<ForumThread> ForumThreads { get; set; }
//        public DbSet<ThreadComment> ThreadComments { get; set; }
//        public DbSet<ThreadComReply> ThreadComReplies { get; set; }
//        public DbSet<CV> CVs { get; set; }
//        public DbSet<Badge> Badges { get; set; }

//        // Notification Module
//        public DbSet<Notification> Notifications { get; set; }

//        // Course Module
//        public DbSet<Course> Courses { get; set; }
//        public DbSet<Lesson> Lessons { get; set; }
//        public DbSet<CourseDocument> CourseDocuments { get; set; }
//        public DbSet<Quiz> Quizzes { get; set; }
//        public DbSet<MCQQuestionOption> MCQQuestionOptions { get; set; }
//        public DbSet<QuizBank> QuizBanks { get; set; }
//        public DbSet<QuizBankQuestion> QuizBankQuestions { get; set; }
//        public DbSet<Enrollment> Enrollments { get; set; }
//        public DbSet<Certificate> Certificates { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            // Configure User entity with improved JSON serialization
//            modelBuilder.Entity<User>()
//                .Property(e => e.Roles)
//                .HasConversion(
//                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
//                    v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>()
//                )
//                .HasColumnType("nvarchar(max)");

//            // Add a value comparer for Roles collection to fix EF Core warning
//            modelBuilder.Entity<User>()
//                .Property(e => e.Roles)
//                .Metadata.SetValueComparer(
//                    new ValueComparer<List<string>>(
//                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
//                        c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
//                        c => c != null ? c.ToList() : new List<string>()
//                    )
//                );

//            // Configure RefreshToken entity
//            modelBuilder.Entity<RefreshToken>()
//                .HasKey(e => e.Id);

//            modelBuilder.Entity<RefreshToken>()
//                .HasOne(e => e.User)
//                .WithMany()
//                .HasForeignKey(e => e.UserId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure CourseCategory entity
//            modelBuilder.Entity<CourseCategory>()
//                .HasKey(e => e.Id);

//            // Configure Technology entity
//            modelBuilder.Entity<Technology>()
//                .HasKey(e => e.Id);

//            // Configure ForumThread entity
//            modelBuilder.Entity<ForumThread>()
//                .HasKey(e => e.ThreadId);

//            // Configure ThreadComment entity
//            modelBuilder.Entity<ThreadComment>()
//                .HasKey(e => e.Id);

//            modelBuilder.Entity<ThreadComment>()
//                .HasOne(c => c.Thread)
//                .WithMany()
//                .HasForeignKey(c => c.ThreadId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure ThreadComReply entity
//            modelBuilder.Entity<ThreadComReply>()
//                .HasKey(e => e.Id);

//            modelBuilder.Entity<ThreadComReply>()
//                .HasOne(r => r.Thread)
//                .WithMany()
//                .HasForeignKey(r => r.ThreadId)
//                .OnDelete(DeleteBehavior.NoAction);

//            modelBuilder.Entity<ThreadComReply>()
//                .HasOne(r => r.Comment)
//                .WithMany()
//                .HasForeignKey(r => r.CommentId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure CV entity
//            modelBuilder.Entity<CV>()
//                .HasKey(e => e.CvId);

//            // Configure Badge entity
//            modelBuilder.Entity<Badge>()
//                .HasKey(e => e.Id);

//            // Configure Notification entity
//            modelBuilder.Entity<Notification>()
//                .HasKey(e => e.NotificationID);

//            modelBuilder.Entity<Notification>()
//                .Property(e => e.CreatedAt)
//                .HasDefaultValueSql("GETUTCDATE()");

//            modelBuilder.Entity<Notification>()
//                .Property(e => e.IsRead)
//                .HasDefaultValue(false);

//            modelBuilder.Entity<Notification>()
//                .Property(e => e.IsDeleted)
//                .HasDefaultValue(false);

//            // Configure Course entity
//            modelBuilder.Entity<Course>()
//                .HasKey(e => e.Id);

//            modelBuilder.Entity<Course>()
//                .Property(e => e.Status)
//                .HasConversion<string>();

//            // Configure Course-Lesson relationship
//            modelBuilder.Entity<Course>()
//                .HasMany(c => c.Lessons)
//                .WithOne(l => l.Course)
//                .HasForeignKey(l => l.CourseId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure Lesson entity
//            modelBuilder.Entity<Lesson>()
//                .HasKey(e => e.Id);

//            // Configure CourseDocument entity
//            modelBuilder.Entity<CourseDocument>()
//                .HasKey(e => e.Id);

//            // Configure CourseDocument entity
//            modelBuilder.Entity<CourseDocument>()
//                .Property(e => e.DocumentType)
//                .HasConversion<string>();

//            // Configure Lesson-CourseDocument relationship
//            modelBuilder.Entity<Lesson>()
//                .HasMany(l => l.Documents)
//                .WithOne(d => d.Lesson)
//                .HasForeignKey(d => d.LessonId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure Quiz entity
//            modelBuilder.Entity<Quiz>();

//            // Configure MCQQuestionOption entity
//            modelBuilder.Entity<MCQQuestionOption>();

//            // Configure QuizBank entity
//            modelBuilder.Entity<QuizBank>();

//            // Configure QuizBankQuestion entity
//            modelBuilder.Entity<QuizBankQuestion>();

//            // Configure QuizBank-QuizBankQuestion relationship
//            modelBuilder.Entity<QuizBank>()
//                .HasMany(qb => qb.QuizBankQuestions)
//                .WithOne(qbQ => qbQ.QuizBank)
//                .HasForeignKey(qbQ => qbQ.QuizBankId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure QuizBankQuestion-MCQQuestionOption relationship
//            modelBuilder.Entity<QuizBankQuestion>()
//                .HasMany(qbQ => qbQ.MCQQuestionOptions)
//                .WithOne(mcqOption => mcqOption.QuizBankQuestion)
//                .HasForeignKey(mcqOption => mcqOption.QuizBankQuestionId)
//                .OnDelete(DeleteBehavior.Cascade);

//            // Configure Enrollment entity
//            modelBuilder.Entity<Enrollment>();

//            // Configure Certificate entity
//            modelBuilder.Entity<Certificate>();
//        }
//    }
//}

// ExcellyGenLMS.Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // For ValueComparer
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Entities.Course;
using ExcellyGenLMS.Core.Entities.Notifications;
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
        public DbSet<User> Users { get; set; } = null!; // Initialize with null-forgiving operator or `= default!;`
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

        // Notification Module
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Course Module
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


        // --- Model Configuration ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<Enrollment>();
            modelBuilder.Entity<Certificate>();

        }
    }
}