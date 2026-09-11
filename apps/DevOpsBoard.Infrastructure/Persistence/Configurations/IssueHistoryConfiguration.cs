using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class IssueHistoryConfiguration
    : IEntityTypeConfiguration<IssueHistory>
{
    public void Configure(
        EntityTypeBuilder<IssueHistory> builder)
    {
        builder.ToTable("issue_history");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.IssueId)
            .IsRequired();

        builder.Property(history => history.ActorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(history => history.CorrelationId)
            .IsRequired();

        builder.Property(history => history.Action)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(history => history.OldValue)
            .HasColumnType("text");

        builder.Property(history => history.NewValue)
            .HasColumnType("text");

        builder.Property(history => history.CreatedAt)
            .IsRequired();

        builder.HasOne<Issue>()
            .WithMany()
            .HasForeignKey(history => history.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(history => history.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(history => history.IssueId);

        builder.HasIndex(history => history.ActorId);

        builder.HasIndex(history => history.CorrelationId);

        builder.HasIndex(
            history => new
            {
                history.IssueId,
                history.CreatedAt
            }
        );
    }
}