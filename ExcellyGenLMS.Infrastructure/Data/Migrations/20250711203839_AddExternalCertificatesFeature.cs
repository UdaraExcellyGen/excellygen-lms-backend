using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalCertificatesFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalCertificates",
                columns: table => new
                {
                    external_certificate_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    issuer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    platform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    completion_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    credential_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    credential_id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalCertificates", x => x.external_certificate_id);
                    table.ForeignKey(
                        name: "FK_ExternalCertificates_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_completion_date",
                table: "ExternalCertificates",
                column: "completion_date");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_platform",
                table: "ExternalCertificates",
                column: "platform");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCertificates_user_id",
                table: "ExternalCertificates",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalCertificates");
        }
    }
}
