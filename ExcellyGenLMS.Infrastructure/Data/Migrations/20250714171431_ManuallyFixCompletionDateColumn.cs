using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcellyGenLMS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ManuallyFixCompletionDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This method is intentionally left empty because the 'completion_date' column
            // already exists in the database from a previous failed migration.
            // Running this empty migration will simply update the __EFMigrationsHistory table
            // to mark this state as complete without trying to alter the schema.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This method is also left empty. We do not want to drop the column on a rollback.
        }
    }
}