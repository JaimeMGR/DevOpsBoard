using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevOpsBoard.Infrastructure.Persistence.Configurations;

public class LabelConfiguration
    : IEntityTypeConfiguration<Label>
{
    public void Configure(
        EntityTypeBuilder<Label> builder)
    {
        builder.ToTable("labels");

        builder.HasKey(
            label => label.Id
        );

        builder.Property(
                label => label.Name
            )
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(
                label => label.Color
            )
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(
                label => new
                {
                    label.ProjectId,
                    label.Name
                }
            )
            .IsUnique();

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(
                label => label.ProjectId
            )
            .OnDelete(
                DeleteBehavior.Cascade
            );
    }
}