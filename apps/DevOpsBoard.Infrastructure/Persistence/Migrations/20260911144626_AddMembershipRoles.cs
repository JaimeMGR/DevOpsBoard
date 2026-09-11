using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "team_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "project_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "team_members");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "project_members");
        }
    }
}
