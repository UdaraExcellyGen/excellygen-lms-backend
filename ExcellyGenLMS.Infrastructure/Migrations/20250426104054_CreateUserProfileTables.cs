using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserProfileTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create new tables with the correct schema

            // Create Badges table with string ID
            migrationBuilder.Sql(@"
                -- Create a new Badges table with string IDs
                CREATE TABLE [BadgesNew] (
                    [Id] nvarchar(450) NOT NULL,
                    [Name] nvarchar(200) NOT NULL,
                    [Description] nvarchar(500) NULL,
                    [ImagePath] nvarchar(255) NULL,
                    [Color] nvarchar(50) NULL,
                    [Icon] nvarchar(100) NULL,
                    CONSTRAINT [PK_BadgesNew] PRIMARY KEY ([Id])
                );
                
                -- Copy data from old Badges table to new one with string IDs
                INSERT INTO [BadgesNew] ([Id], [Name], [Description], [ImagePath], [Color], [Icon])
                SELECT 
                    'BADGE-' + CAST([Id] AS NVARCHAR(10)), 
                    [Name], 
                    [Description], 
                    [Image] AS [ImagePath],
                    NULL AS [Color],
                    NULL AS [Icon]
                FROM [Badges];
            ");

            // Create Certifications table
            migrationBuilder.CreateTable(
                name: "Certifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IssuingOrganization = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CredentialId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                });

            // Create Projects table
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            // Step 2: Create junction tables

            // Drop foreign keys referencing the Badges table if any exist
            migrationBuilder.Sql(@"
                DECLARE @sql NVARCHAR(MAX) = '';
                SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' +
                              QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
                FROM sys.foreign_keys
                WHERE referenced_object_id = OBJECT_ID('Badges');
                
                EXEC sp_executesql @sql;
                
                -- Drop old Badges table and rename new one
                DROP TABLE [Badges];
                EXEC sp_rename 'BadgesNew', 'Badges';
            ");

            // Create UserBadges table
            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BadgeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBadges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create UserTechnologies table
            migrationBuilder.CreateTable(
                name: "UserTechnologies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TechnologyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTechnologies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTechnologies_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTechnologies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create UserCertifications table
            migrationBuilder.CreateTable(
                name: "UserCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CertificationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCertifications_Certifications_CertificationId",
                        column: x => x.CertificationId,
                        principalTable: "Certifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCertifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ProjectTechnologies table
            migrationBuilder.CreateTable(
                name: "ProjectTechnologies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TechnologyId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTechnologies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTechnologies_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTechnologies_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create UserProjects table
            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indices
            migrationBuilder.CreateIndex(
                name: "IX_ProjectTechnologies_ProjectId",
                table: "ProjectTechnologies",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTechnologies_TechnologyId",
                table: "ProjectTechnologies",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserId",
                table: "UserBadges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCertifications_CertificationId",
                table: "UserCertifications",
                column: "CertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCertifications_UserId",
                table: "UserCertifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_ProjectId",
                table: "UserProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_UserId",
                table: "UserProjects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTechnologies_TechnologyId",
                table: "UserTechnologies",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTechnologies_UserId",
                table: "UserTechnologies",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTechnologies");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "UserCertifications");

            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.DropTable(
                name: "UserTechnologies");

            migrationBuilder.DropTable(
                name: "Certifications");

            migrationBuilder.DropTable(
                name: "Projects");

            // Recreate the original Badges table
            migrationBuilder.Sql(@"
                -- Create a temporary table with the original schema
                CREATE TABLE [BadgesOld] (
                    [Id] int NOT NULL IDENTITY(1,1),
                    [Name] nvarchar(200) NOT NULL,
                    [Description] nvarchar(500) NULL,
                    [Image] nvarchar(255) NULL,
                    [EarnedDate] datetime2 NULL,
                    CONSTRAINT [PK_BadgesOld] PRIMARY KEY ([Id])
                );
                
                -- Copy data back (will lose some data as this is a destructive change)
                INSERT INTO [BadgesOld] ([Name], [Description], [Image])
                SELECT [Name], [Description], [ImagePath]
                FROM [Badges];
                
                -- Drop constraints referencing Badges
                DECLARE @sql NVARCHAR(MAX) = '';
                SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' +
                              QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
                FROM sys.foreign_keys
                WHERE referenced_object_id = OBJECT_ID('Badges');
                
                EXEC sp_executesql @sql;
                
                -- Drop new table and rename old one back
                DROP TABLE [Badges];
                EXEC sp_rename 'BadgesOld', 'Badges';
            ");
        }
    }
}