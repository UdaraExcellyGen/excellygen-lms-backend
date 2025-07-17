using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentsToCourseAndFixIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_lesson_id",
                table: "Quizzes",
                newName: "IX_Quizzes_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempts_quiz_id",
                table: "QuizAttempts",
                newName: "IX_QuizAttempts_QuizId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_user_id",
                table: "Enrollments",
                newName: "IX_Enrollments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_course_id",
                table: "Enrollments",
                newName: "IX_Enrollments_CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_Completion_Score",
                table: "QuizAttempts",
                columns: new[] { "is_completed", "score" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CompletionDate",
                table: "Enrollments",
                column: "completion_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_Completion_Score",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_CompletionDate",
                table: "Enrollments");

            migrationBuilder.RenameIndex(
                name: "IX_Quizzes_LessonId",
                table: "Quizzes",
                newName: "IX_Quizzes_lesson_id");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts",
                newName: "IX_QuizAttempts_quiz_id");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_UserId",
                table: "Enrollments",
                newName: "IX_Enrollments_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                newName: "IX_Enrollments_course_id");
        }
    }
}
