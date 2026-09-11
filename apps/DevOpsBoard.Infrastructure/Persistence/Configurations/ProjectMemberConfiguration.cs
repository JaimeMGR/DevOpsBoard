using DevOpsBoard.Domain.Enums;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration
    : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(
        EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("project_members");

        builder.HasKey(member => new
        {
            member.ProjectId,
            member.UserId
        });

        builder.Property(member => member.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(member => member.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(member => member.JoinedAt)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(member => member.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}