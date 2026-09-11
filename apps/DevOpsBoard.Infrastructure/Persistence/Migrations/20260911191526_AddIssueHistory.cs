using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "issues",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "issues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "issue_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_issue_history_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_issue_history_issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_issues_IsDeleted",
                table: "issues",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_issue_history_ActorId",
                table: "issue_history",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_history_CorrelationId",
                table: "issue_history",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_history_IssueId",
                table: "issue_history",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_history_IssueId_CreatedAt",
                table: "issue_history",
                columns: new[] { "IssueId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "issue_history");

            migrationBuilder.DropIndex(
                name: "IX_issues_IsDeleted",
                table: "issues");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "issues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "issues");
        }
    }
}
