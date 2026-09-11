using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration
    : IEntityTypeConfiguration<Project>
{
    public void Configure(
        EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(project => project.Key)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(project => project.Description)
            .HasMaxLength(1000);

        builder.Property(project => project.CreatedAt)
            .IsRequired();

        builder.Property(project => project.OwnerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(project => project.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(project => project.Key)
            .IsUnique();

        builder.HasIndex(project => project.OwnerId);
    }
}