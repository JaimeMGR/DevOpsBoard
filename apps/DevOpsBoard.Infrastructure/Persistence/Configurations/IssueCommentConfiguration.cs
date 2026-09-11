using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class IssueCommentConfiguration
    : IEntityTypeConfiguration<IssueComment>
{
    public void Configure(
        EntityTypeBuilder<IssueComment> builder)
    {
        builder.ToTable("issue_comments");

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.IssueId)
            .IsRequired();

        builder.Property(comment => comment.AuthorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(comment => comment.CreatedAt)
            .IsRequired();

        builder.Property(comment => comment.UpdatedAt)
            .IsRequired();

        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(comment => comment.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(comment => comment.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(comment => comment.IssueId);

        builder.HasIndex(comment => comment.AuthorId);

        builder.HasIndex(
            comment => new
            {
                comment.IssueId,
                comment.CreatedAt
            }
        );
    }
}