using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(
       EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(team => team.Id);

        builder.Property(team => team.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(team => team.Description)
            .HasMaxLength(500);

        builder.Property(team => team.CreatedAt)
            .IsRequired();

        builder.Property(team => team.CreatedByUserId)
            .HasMaxLength(450);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(team => team.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(team => team.Name)
            .IsUnique();

        builder.HasIndex(team => team.CreatedByUserId);
    }
}