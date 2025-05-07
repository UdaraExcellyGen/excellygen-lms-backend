using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Data.SqlClient; // <--- ADDED THIS USING DIRECTIVE

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_TitleAndImageUrl_To_ForumThreads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- START: Changes related to Forum Tables ----

            // Handle the potential removal of the direct ThreadId FK from ThreadComReplies
            // Defensive try-catch for dropping FK
            try
            {
                migrationBuilder.DropForeignKey(
                   name: "FK_ThreadComReplies_ForumThreads_ThreadId",
                   table: "ThreadComReplies");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 3728 || ex.Message.ToLower().Contains("is not a constraint"))
            {
                // 3728: 'FK_... is not a constraint.' (SQL Server)
                // Or check message for common phrases if error number varies
                System.Console.WriteLine($"Info: FK 'FK_ThreadComReplies_ForumThreads_ThreadId' was not a constraint or did not exist. Skipping drop. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Warning: An unexpected error occurred while trying to drop FK_ThreadComReplies_ForumThreads_ThreadId. Proceeding with caution. Error: {ex.Message}");
            }

            // Drop Index and Column for the old ThreadId in ThreadComReplies
            // EF Core usually generates SQL that won't error if the index/column isn't there, but being defensive.
            // It's generally okay if these no-op on a fresh DB.
            try
            {
                migrationBuilder.DropIndex(
                    name: "IX_ThreadComReplies_ThreadId",
                    table: "ThreadComReplies");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.ToLower().Contains("cannot drop the index") && ex.Message.ToLower().Contains("because it does not exist"))
            {
                System.Console.WriteLine($"Info: Index 'IX_ThreadComReplies_ThreadId' did not exist. Skipping drop. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Warning: An unexpected error occurred while trying to drop index IX_ThreadComReplies_ThreadId. Proceeding with caution. Error: {ex.Message}");
            }

            try
            {
                migrationBuilder.DropColumn(
                    name: "ThreadId",
                    table: "ThreadComReplies");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.ToLower().Contains("invalid column name"))
            {
                System.Console.WriteLine($"Info: Column 'ThreadId' on 'ThreadComReplies' did not exist. Skipping drop. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Warning: An unexpected error occurred while trying to drop column ThreadId from ThreadComReplies. Proceeding with caution. Error: {ex.Message}");
            }


            migrationBuilder.AlterColumn<string>(
                name: "Commentor",
                table: "ThreadComReplies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Commentor",
                table: "ThreadComments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "ForumThreads",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ForumThreads",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ForumThreads",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ForumThreads",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadComReplies_Commentor",
                table: "ThreadComReplies",
                column: "Commentor");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadComments_Commentor",
                table: "ThreadComments",
                column: "Commentor");

            migrationBuilder.CreateIndex(
                name: "IX_ForumThreads_Creator",
                table: "ForumThreads",
                column: "Creator");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumThreads_Users_Creator",
                table: "ForumThreads",
                column: "Creator",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadComments_Users_Commentor",
                table: "ThreadComments",
                column: "Commentor",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadComReplies_Users_Commentor",
                table: "ThreadComReplies",
                column: "Commentor",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumThreads_Users_Creator",
                table: "ForumThreads");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadComments_Users_Commentor",
                table: "ThreadComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ThreadComReplies_Users_Commentor",
                table: "ThreadComReplies");

            migrationBuilder.DropIndex(
                name: "IX_ThreadComReplies_Commentor",
                table: "ThreadComReplies");

            migrationBuilder.DropIndex(
                name: "IX_ThreadComments_Commentor",
                table: "ThreadComments");

            migrationBuilder.DropIndex(
                name: "IX_ForumThreads_Creator",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ForumThreads");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ForumThreads");

            migrationBuilder.AlterColumn<string>(
                name: "Commentor",
                table: "ThreadComReplies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "ThreadId",
                table: "ThreadComReplies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ThreadComReplies_ThreadId",
                table: "ThreadComReplies",
                column: "ThreadId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadComReplies_ForumThreads_ThreadId",
                table: "ThreadComReplies",
                column: "ThreadId",
                principalTable: "ForumThreads",
                principalColumn: "ThreadId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AlterColumn<string>(
                name: "Commentor",
                table: "ThreadComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                table: "ForumThreads",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ForumThreads",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
