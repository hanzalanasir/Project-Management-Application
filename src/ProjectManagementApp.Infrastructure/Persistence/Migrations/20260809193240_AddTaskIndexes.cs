using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_tasks_assignee_id_status",
                table: "tasks",
                columns: new[] { "assignee_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_project_id_status",
                table: "tasks",
                columns: new[] { "project_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_tasks_status",
                table: "tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_title_trgm",
                table: "tasks",
                column: "title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tasks_assignee_id_status",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "ix_tasks_project_id_status",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "ix_tasks_status",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "ix_tasks_title_trgm",
                table: "tasks");
        }
    }
}
