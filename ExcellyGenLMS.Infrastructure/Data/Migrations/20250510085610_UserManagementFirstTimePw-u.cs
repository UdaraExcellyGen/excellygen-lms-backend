using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserManagementFirstTimePwu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequirePasswordChange",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirePasswordChange",
                table: "Users");
        }
    }
}
