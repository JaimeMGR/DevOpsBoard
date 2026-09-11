using DevOpsBoard.Domain.Enums;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration
    : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(
        EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("team_members");

        builder.HasKey(member => new
        {
            member.TeamId,
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

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(member => member.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}