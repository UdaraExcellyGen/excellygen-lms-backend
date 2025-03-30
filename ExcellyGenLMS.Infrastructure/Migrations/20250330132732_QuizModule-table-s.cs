using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuizModuletables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizBanks",
                columns: table => new
                {
                    quiz_bank_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_bank_size = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizBanks", x => x.quiz_bank_id);
                });

            migrationBuilder.CreateTable(
                name: "QuizBankQuestions",
                columns: table => new
                {
                    quiz_bank_question_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_bank_id = table.Column<int>(type: "int", nullable: false),
                    question_content = table.Column<string>(type: "TEXT", nullable: false),
                    question_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    question_bank_order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizBankQuestions", x => x.quiz_bank_question_id);
                    table.ForeignKey(
                        name: "FK_QuizBankQuestions_QuizBanks_quiz_bank_id",
                        column: x => x.quiz_bank_id,
                        principalTable: "QuizBanks",
                        principalColumn: "quiz_bank_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    quiz_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    time_limit_minutes = table.Column<int>(type: "int", nullable: false),
                    total_marks = table.Column<int>(type: "int", nullable: false),
                    quiz_size = table.Column<int>(type: "int", nullable: false),
                    quiz_bank_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.quiz_id);
                    table.ForeignKey(
                        name: "FK_Quizzes_Lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quizzes_QuizBanks_quiz_bank_id",
                        column: x => x.quiz_bank_id,
                        principalTable: "QuizBanks",
                        principalColumn: "quiz_bank_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MCQQuestionOptions",
                columns: table => new
                {
                    mcq_option_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_bank_question_id = table.Column<int>(type: "int", nullable: false),
                    option_text = table.Column<string>(type: "TEXT", nullable: false),
                    is_correct = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MCQQuestionOptions", x => x.mcq_option_id);
                    table.ForeignKey(
                        name: "FK_MCQQuestionOptions_QuizBankQuestions_quiz_bank_question_id",
                        column: x => x.quiz_bank_question_id,
                        principalTable: "QuizBankQuestions",
                        principalColumn: "quiz_bank_question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MCQQuestionOptions_quiz_bank_question_id",
                table: "MCQQuestionOptions",
                column: "quiz_bank_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_QuizBankQuestions_quiz_bank_id",
                table: "QuizBankQuestions",
                column: "quiz_bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_lesson_id",
                table: "Quizzes",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_quiz_bank_id",
                table: "Quizzes",
                column: "quiz_bank_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MCQQuestionOptions");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizBankQuestions");

            migrationBuilder.DropTable(
                name: "QuizBanks");
        }
    }
}
