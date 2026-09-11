using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class IssueConfiguration
    : IEntityTypeConfiguration<Issue>
{
    public void Configure(
        EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("issues");

        builder.HasKey(issue => issue.Id);

        builder.Property(issue => issue.ProjectId)
            .IsRequired();

        builder.Property(issue => issue.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(issue => issue.Description)
            .HasColumnType("text");

        builder.Property(issue => issue.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(issue => issue.Priority)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(issue => issue.ReporterId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(issue => issue.AssigneeId)
            .HasMaxLength(450);

        builder.Property(issue => issue.CreatedAt)
            .IsRequired();

        builder.Property(issue => issue.UpdatedAt)
            .IsRequired();

        builder.Property(issue => issue.IsDeleted)
            .IsRequired();

        builder.Property(issue => issue.DeletedAt);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(issue => issue.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(issue => issue.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(issue => issue.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(issue => issue.ProjectId);

        builder.HasIndex(issue => issue.ReporterId);

        builder.HasIndex(issue => issue.AssigneeId);

        builder.HasIndex(issue => issue.Status);

        builder.HasIndex(issue => issue.Priority);

        builder.HasIndex(issue => issue.IsDeleted);

        builder.HasQueryFilter(
            issue => !issue.IsDeleted
        );
    }
}