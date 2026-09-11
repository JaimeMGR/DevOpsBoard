using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMembershipRoleDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "team_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Member",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "team_members",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Member");
        }
    }
}
