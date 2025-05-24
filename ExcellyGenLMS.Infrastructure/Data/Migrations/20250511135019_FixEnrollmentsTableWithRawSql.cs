using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixEnrollmentsTableWithRawSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop constraints first if they exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Enrollments_Courses_course_id]') AND parent_object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
                ALTER TABLE [dbo].[Enrollments] DROP CONSTRAINT [FK_Enrollments_Courses_course_id]
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Enrollments_Users_user_id]') AND parent_object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
                ALTER TABLE [dbo].[Enrollments] DROP CONSTRAINT [FK_Enrollments_Users_user_id]
                
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_course_id' AND object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
                DROP INDEX [IX_Enrollments_course_id] ON [dbo].[Enrollments]
                
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_user_id' AND object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
                DROP INDEX [IX_Enrollments_user_id] ON [dbo].[Enrollments]
            ");

            // Drop and recreate the table
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Enrollments')
                BEGIN
                    DROP TABLE [Enrollments]
                END

                CREATE TABLE [Enrollments] (
                    [enrollment_id] INT NOT NULL IDENTITY(1,1),
                    [user_id] NVARCHAR(450) NOT NULL,
                    [course_id] INT NOT NULL,
                    [enrollment_date] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
                    [status] NVARCHAR(50) NOT NULL DEFAULT ('active'),
                    CONSTRAINT [PK_Enrollments] PRIMARY KEY ([enrollment_id]),
                    CONSTRAINT [FK_Enrollments_Users_user_id] FOREIGN KEY ([user_id]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
                    CONSTRAINT [FK_Enrollments_Courses_course_id] FOREIGN KEY ([course_id]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION
                )

                CREATE INDEX [IX_Enrollments_user_id] ON [Enrollments] ([user_id])
                CREATE INDEX [IX_Enrollments_course_id] ON [Enrollments] ([course_id])
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // On revert, just drop the recreated table
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Enrollments') DROP TABLE [Enrollments]");

            // Recreate the original table structure
            migrationBuilder.Sql(@"
                CREATE TABLE [Enrollments] (
                    [enrollment_id] INT NOT NULL IDENTITY(1,1),
                    [enrollment_date] DATETIME2 NOT NULL,
                    [enrollment_time] TIME NOT NULL,
                    CONSTRAINT [PK_Enrollments] PRIMARY KEY ([enrollment_id])
                )
            ");
        }
    }
}