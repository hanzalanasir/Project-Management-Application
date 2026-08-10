using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamMemberIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ux_team_members_project_id_user_id",
                table: "team_members",
                columns: new[] { "project_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_team_members_project_id_user_id",
                table: "team_members");
        }
    }
}
