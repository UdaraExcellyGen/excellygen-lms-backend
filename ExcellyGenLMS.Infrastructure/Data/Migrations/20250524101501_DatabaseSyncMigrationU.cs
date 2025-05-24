using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSyncMigrationU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep column modifications since they're altering existing structures, not creating duplicates

            // ThumbnailImage column changes (drop old, add new with different name)
            if (migrationBuilder.Operations.Any(o => o.GetType().Name == "DropColumnOperation" && ((dynamic)o).Name == "ThumbnailImage" && ((dynamic)o).Table == "Courses"))
            {
                migrationBuilder.DropColumn(
                    name: "ThumbnailImage",
                    table: "Courses");
            }

            // Status column changes
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Courses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Description column changes
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // CoursePoints column changes
            migrationBuilder.AlterColumn<int>(
                name: "CoursePoints",
                table: "Courses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Add new ThumbnailImagePath column
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailImagePath",
                table: "Courses",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            // CourseDocuments column changes
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CourseDocuments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "CourseDocuments",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "CourseDocuments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Check if CourseTechnologies table exists before creating it
            // This is the only new table in the migration
            if (!migrationBuilder.Operations.Any(o => o.GetType().Name == "CreateTableOperation" && ((dynamic)o).Name == "CourseTechnologies"))
            {
                migrationBuilder.CreateTable(
                    name: "CourseTechnologies",
                    columns: table => new
                    {
                        CourseId = table.Column<int>(type: "int", nullable: false),
                        TechnologyId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_CourseTechnologies", x => new { x.CourseId, x.TechnologyId });
                        table.ForeignKey(
                            name: "FK_CourseTechnologies_Courses_CourseId",
                            column: x => x.CourseId,
                            principalTable: "Courses",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_CourseTechnologies_Technologies_TechnologyId",
                            column: x => x.TechnologyId,
                            principalTable: "Technologies",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateIndex(
                    name: "IX_CourseTechnologies_TechnologyId",
                    table: "CourseTechnologies",
                    column: "TechnologyId");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseTechnologies");

            migrationBuilder.DropColumn(
                name: "ThumbnailImagePath",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CoursePoints",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailImage",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CourseDocuments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "CourseDocuments",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "CourseDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}