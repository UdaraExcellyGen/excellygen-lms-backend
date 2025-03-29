using Microsoft.EntityFrameworkCore;
using ExcellyGenLMS.Core.Entities.Auth;
using ExcellyGenLMS.Core.Entities.Admin;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .Property(e => e.Roles)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>()
                );

            // Configure CourseCategory entity
            modelBuilder.Entity<CourseCategory>()
                .HasKey(e => e.Id);

            // Configure Technology entity
            modelBuilder.Entity<Technology>()
                .HasKey(e => e.Id);
        }
    }
}