using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserManagementAvatarU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if columns exist before trying to add them
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns 
                               WHERE Name = 'course_id' AND Object_ID = Object_ID('Enrollments'))
                BEGIN
                    ALTER TABLE [Enrollments] ADD [course_id] int NOT NULL DEFAULT 0;
                END
                
                IF NOT EXISTS (SELECT 1 FROM sys.columns 
                               WHERE Name = 'status' AND Object_ID = Object_ID('Enrollments'))
                BEGIN
                    ALTER TABLE [Enrollments] ADD [status] nvarchar(max) NOT NULL DEFAULT '';
                END
                
                IF NOT EXISTS (SELECT 1 FROM sys.columns 
                               WHERE Name = 'user_id' AND Object_ID = Object_ID('Enrollments'))
                BEGIN
                    ALTER TABLE [Enrollments] ADD [user_id] nvarchar(450) NOT NULL DEFAULT '';
                END");

            // Check if indexes exist before creating them
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                               WHERE Name = 'IX_Enrollments_course_id' AND Object_ID = Object_ID('Enrollments'))
                BEGIN
                    CREATE INDEX [IX_Enrollments_course_id] ON [Enrollments] ([course_id]);
                END
                
                IF NOT EXISTS (SELECT 1 FROM sys.indexes 
                               WHERE Name = 'IX_Enrollments_user_id' AND Object_ID = Object_ID('Enrollments'))
                BEGIN
                    CREATE INDEX [IX_Enrollments_user_id] ON [Enrollments] ([user_id]);
                END");

            // Check if foreign keys exist before creating them
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys 
                               WHERE name = 'FK_Enrollments_Courses_course_id' AND parent_object_id = OBJECT_ID('Enrollments'))
                BEGIN
                    ALTER TABLE [Enrollments] ADD CONSTRAINT [FK_Enrollments_Courses_course_id] 
                    FOREIGN KEY ([course_id]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE;
                END
                
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys 
                               WHERE name = 'FK_Enrollments_Users_user_id' AND parent_object_id = OBJECT_ID('Enrollments'))
                BEGIN
                    ALTER TABLE [Enrollments] ADD CONSTRAINT [FK_Enrollments_Users_user_id] 
                    FOREIGN KEY ([user_id]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_course_id",
                table: "Enrollments");
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_user_id",
                table: "Enrollments");
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_course_id",
                table: "Enrollments");
            migrationBuilder.DropIndex(
                name: "IX_Enrollments_user_id",
                table: "Enrollments");
            migrationBuilder.DropColumn(
                name: "course_id",
                table: "Enrollments");
            migrationBuilder.DropColumn(
                name: "status",
                table: "Enrollments");
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "Enrollments");
            // We're not adding enrollment_time back since it didn't exist
        }
    }
}