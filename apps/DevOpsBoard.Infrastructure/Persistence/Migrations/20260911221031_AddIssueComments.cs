using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevOpsBoard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "issue_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_issue_comments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_issue_comments_issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_AuthorId",
                table: "issue_comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_IssueId",
                table: "issue_comments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_IssueId_CreatedAt",
                table: "issue_comments",
                columns: new[] { "IssueId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "issue_comments");
        }
    }
}
