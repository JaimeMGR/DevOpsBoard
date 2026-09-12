using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class IssueLabelConfiguration
    : IEntityTypeConfiguration<IssueLabel>
{
    public void Configure(
        EntityTypeBuilder<IssueLabel> builder)
    {
        builder.ToTable("issue_labels");

        builder.HasKey(
            issueLabel => new
            {
                issueLabel.IssueId,
                issueLabel.LabelId
            }
        );

        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(
                issueLabel => issueLabel.IssueId
            )
            .OnDelete(
                DeleteBehavior.Cascade
            );

        builder.HasOne<Label>()
            .WithMany()
            .HasForeignKey(
                issueLabel => issueLabel.LabelId
            )
            .OnDelete(
                DeleteBehavior.Cascade
            );

        builder.HasIndex(
            issueLabel => issueLabel.LabelId
        );
    }
}