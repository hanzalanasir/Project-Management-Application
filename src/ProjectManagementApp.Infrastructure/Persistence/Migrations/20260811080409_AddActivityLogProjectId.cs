using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogProjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "activity_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_project_id",
                table: "activity_logs",
                column: "project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_activity_logs_project_id",
                table: "activity_logs");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "activity_logs");
        }
    }
}
