using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
using ExcellyGenLMS.Core.Entities.Learner;
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
        }
    }
}