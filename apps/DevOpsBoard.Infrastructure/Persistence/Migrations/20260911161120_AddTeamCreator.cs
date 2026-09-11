using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "teams",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_CreatedByUserId",
                table: "teams",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_teams_AspNetUsers_CreatedByUserId",
                table: "teams",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teams_AspNetUsers_CreatedByUserId",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_teams_CreatedByUserId",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "teams");
        }
    }
}
