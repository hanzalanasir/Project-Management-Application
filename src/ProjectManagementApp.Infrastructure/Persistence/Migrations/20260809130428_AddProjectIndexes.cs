using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Permitted DDL, not data access (Constitution IV.1 prohibits raw SQL for queries, not
            // for enabling a database extension — research.md R-3). Required before the GIN trigram
            // index below: ILIKE '%term%' cannot use an index without it.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.CreateIndex(
                name: "ix_projects_name_trgm",
                table: "projects",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_projects_owner_id_status",
                table: "projects",
                columns: new[] { "owner_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_projects_status",
                table: "projects",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_projects_name_trgm",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_projects_owner_id_status",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_projects_status",
                table: "projects");
        }
    }
}
