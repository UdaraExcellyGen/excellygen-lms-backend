using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    quiz_attempt_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completion_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    score = table.Column<int>(type: "int", nullable: true),
                    is_completed = table.Column<bool>(type: "bit", nullable: false),
                    total_questions = table.Column<int>(type: "int", nullable: false),
                    correct_answers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.quiz_attempt_id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "Quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptAnswers",
                columns: table => new
                {
                    quiz_attempt_answer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_attempt_id = table.Column<int>(type: "int", nullable: false),
                    quiz_bank_question_id = table.Column<int>(type: "int", nullable: false),
                    selected_option_id = table.Column<int>(type: "int", nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptAnswers", x => x.quiz_attempt_answer_id);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_MCQQuestionOptions_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "MCQQuestionOptions",
                        principalColumn: "mcq_option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizAttempts_quiz_attempt_id",
                        column: x => x.quiz_attempt_id,
                        principalTable: "QuizAttempts",
                        principalColumn: "quiz_attempt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizBankQuestions_quiz_bank_question_id",
                        column: x => x.quiz_bank_question_id,
                        principalTable: "QuizBankQuestions",
                        principalColumn: "quiz_bank_question_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_quiz_attempt_id",
                table: "QuizAttemptAnswers",
                column: "quiz_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_quiz_bank_question_id",
                table: "QuizAttemptAnswers",
                column: "quiz_bank_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_selected_option_id",
                table: "QuizAttemptAnswers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_quiz_id",
                table: "QuizAttempts",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_user_id",
                table: "QuizAttempts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAttemptAnswers");

            migrationBuilder.DropTable(
                name: "QuizAttempts");
        }
    }
}
