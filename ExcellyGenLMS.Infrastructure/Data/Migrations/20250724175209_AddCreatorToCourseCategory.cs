using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorToCourseCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "CourseCategories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseCategories_CreatedById",
                table: "CourseCategories",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseCategories_Users_CreatedById",
                table: "CourseCategories",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseCategories_Users_CreatedById",
                table: "CourseCategories");

            migrationBuilder.DropIndex(
                name: "IX_CourseCategories_CreatedById",
                table: "CourseCategories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "CourseCategories");
        }
    }
}
