using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence;

public class DevOpsBoardDbContext
    : IdentityDbContext<ApplicationUser>
{
    public DevOpsBoardDbContext(
        DbContextOptions<DevOpsBoardDbContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<Issue> Issues => Set<Issue>();

    public DbSet<IssueHistory> IssueHistories =>
    Set<IssueHistory>();

    public DbSet<IssueComment> IssueComments =>
    Set<IssueComment>();

    public DbSet<Label> Labels =>
    Set<Label>();

    public DbSet<IssueLabel> IssueLabels =>
        Set<IssueLabel>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DevOpsBoardDbContext).Assembly
        );
    }
}