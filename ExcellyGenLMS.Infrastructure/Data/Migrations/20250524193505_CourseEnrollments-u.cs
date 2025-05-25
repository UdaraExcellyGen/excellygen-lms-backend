using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CourseEnrollmentsu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Certificates",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "course_id",
                table: "Certificates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "Certificates",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LessonProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonProgress_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_course_id",
                table: "Certificates",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_user_id",
                table: "Certificates",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgress_LessonId",
                table: "LessonProgress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgress_UserId_LessonId",
                table: "LessonProgress",
                columns: new[] { "UserId", "LessonId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Courses_course_id",
                table: "Certificates",
                column: "course_id",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Users_user_id",
                table: "Certificates",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Courses_course_id",
                table: "Certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Users_user_id",
                table: "Certificates");

            migrationBuilder.DropTable(
                name: "LessonProgress");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_course_id",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_user_id",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "course_id",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "Certificates");
        }
    }
}
