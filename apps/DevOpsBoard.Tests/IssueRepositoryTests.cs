using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;
using DevOpsBoard.Infrastructure.Persistence;
using DevOpsBoard.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace DevOpsBoard.Tests;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("devopsboard_tests")
            .WithUsername("devopsboard")
            .WithPassword("devopsboard")
            .Build();

    public DbContextOptions<DevOpsBoardDbContext> Options
    {
        get;
        private set;
    } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Options =
            new DbContextOptionsBuilder<DevOpsBoardDbContext>()
                .UseNpgsql(
                    _container.GetConnectionString()
                )
                .Options;

        await using var db =
            new DevOpsBoardDbContext(Options);

        await db.Database.MigrateAsync();

        await SeedUsersAsync(db);
    }

    private static async Task SeedUsersAsync(
        DevOpsBoardDbContext db)
    {
        var users = new[]
        {
            new ApplicationUser
            {
                Id = "integration-test-owner",
                UserName = "integration-test-owner",
                NormalizedUserName = "INTEGRATION-TEST-OWNER",
                Email = "integration-test-owner@devopsboard.local",
                NormalizedEmail = "INTEGRATION-TEST-OWNER@DEVOPSBOARD.LOCAL",
                DisplayName = "Integration Test Owner",
                EmailConfirmed = true,
                IsActive = true
            },

            new ApplicationUser
            {
                Id = "developer-1",
                UserName = "developer-1",
                NormalizedUserName = "DEVELOPER-1",
                Email = "developer-1@devopsboard.local",
                NormalizedEmail = "DEVELOPER-1@DEVOPSBOARD.LOCAL",
                DisplayName = "Developer 1",
                EmailConfirmed = true,
                IsActive = true
            },

            new ApplicationUser
            {
                Id = "developer-2",
                UserName = "developer-2",
                NormalizedUserName = "DEVELOPER-2",
                Email = "developer-2@devopsboard.local",
                NormalizedEmail = "DEVELOPER-2@DEVOPSBOARD.LOCAL",
                DisplayName = "Developer 2",
                EmailConfirmed = true,
                IsActive = true
            }
        };

        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

public class IssueRepositoryTests
    : IClassFixture<PostgreSqlFixture>
{
    private const string OwnerId =
        "integration-test-owner";

    private readonly PostgreSqlFixture _fixture;

    public IssueRepositoryTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    private DevOpsBoardDbContext CreateContext()
    {
        return new DevOpsBoardDbContext(
            _fixture.Options
        );
    }

    private async Task<Project> CreateProjectAsync(
        DevOpsBoardDbContext db)
    {
        var project = new Project(
            "Integration Project",
            $"T{Guid.NewGuid():N}"[..10],
            OwnerId
        );

        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();

        return project;
    }

    private static Issue CreateIssue(
        Guid projectId,
        string title,
        string? description = null,
        IssuePriority priority = IssuePriority.Medium,
        string? assigneeId = null)
    {
        var issue = new Issue(
            projectId,
            title,
            OwnerId,
            description,
            priority
        );

        if (assigneeId is not null)
        {
            issue.AssignTo(assigneeId);
        }

        return issue;
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldPaginateAndIgnoreSoftDeletedIssues()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        var issue1 = CreateIssue(
            project.Id,
            "Alpha"
        );

        var issue2 = CreateIssue(
            project.Id,
            "Beta"
        );

        var issue3 = CreateIssue(
            project.Id,
            "Gamma"
        );

        issue3.Delete();

        await db.Issues.AddRangeAsync(
            issue1,
            issue2,
            issue3
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    Page: 1,
                    PageSize: 1,
                    SortBy: "Title",
                    SortDirection: "asc"
                )
            );

        Assert.Equal(
            2,
            result.TotalCount
        );

        Assert.Equal(
            2,
            result.TotalPages
        );

        Assert.Single(
            result.Items
        );

        Assert.Equal(
            "Alpha",
            result.Items[0].Title
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldReturnSecondPage()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        await db.Issues.AddRangeAsync(
            CreateIssue(
                project.Id,
                "Alpha"
            ),
            CreateIssue(
                project.Id,
                "Beta"
            ),
            CreateIssue(
                project.Id,
                "Gamma"
            )
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    Page: 2,
                    PageSize: 2,
                    SortBy: "Title",
                    SortDirection: "asc"
                )
            );

        Assert.Equal(
            3,
            result.TotalCount
        );

        Assert.Equal(
            2,
            result.TotalPages
        );

        Assert.Single(
            result.Items
        );

        Assert.Equal(
            "Gamma",
            result.Items[0].Title
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldFilterByStatus()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        var todoIssue =
            CreateIssue(
                project.Id,
                "Todo issue"
            );

        var progressIssue =
            CreateIssue(
                project.Id,
                "Progress issue"
            );

        progressIssue.ChangeStatus(
            IssueStatus.InProgress
        );

        await db.Issues.AddRangeAsync(
            todoIssue,
            progressIssue
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    Status: "InProgress"
                )
            );

        Assert.Equal(
            1,
            result.TotalCount
        );

        Assert.Single(
            result.Items
        );

        Assert.Equal(
            IssueStatus.InProgress,
            result.Items[0].Status
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldFilterByPriority()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        await db.Issues.AddRangeAsync(
            CreateIssue(
                project.Id,
                "Low issue",
                priority: IssuePriority.Low
            ),
            CreateIssue(
                project.Id,
                "High issue",
                priority: IssuePriority.High
            ),
            CreateIssue(
                project.Id,
                "Medium issue",
                priority: IssuePriority.Medium
            )
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    Priority: "High"
                )
            );

        Assert.Equal(
            1,
            result.TotalCount
        );

        Assert.Single(
            result.Items
        );

        Assert.Equal(
            IssuePriority.High,
            result.Items[0].Priority
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldFilterByAssignee()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        await db.Issues.AddRangeAsync(
            CreateIssue(
                project.Id,
                "Assigned issue",
                assigneeId: "developer-1"
            ),
            CreateIssue(
                project.Id,
                "Other issue",
                assigneeId: "developer-2"
            )
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    AssigneeId: "developer-1"
                )
            );

        Assert.Equal(
            1,
            result.TotalCount
        );

        Assert.Single(
            result.Items
        );

        Assert.Equal(
            "developer-1",
            result.Items[0].AssigneeId
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldSearchTitleAndDescription()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        await db.Issues.AddRangeAsync(
            CreateIssue(
                project.Id,
                "Implement authentication",
                "JWT login flow"
            ),
            CreateIssue(
                project.Id,
                "Update documentation",
                "No authentication work"
            ),
            CreateIssue(
                project.Id,
                "Frontend dashboard",
                "React components"
            )
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    Search: "authentication"
                )
            );

        Assert.Equal(
            2,
            result.TotalCount
        );

        Assert.Equal(
            2,
            result.Items.Count
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldSortTitleAscending()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        await db.Issues.AddRangeAsync(
            CreateIssue(
                project.Id,
                "Charlie"
            ),
            CreateIssue(
                project.Id,
                "Alpha"
            ),
            CreateIssue(
                project.Id,
                "Bravo"
            )
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    SortBy: "title",
                    SortDirection: "asc"
                )
            );

        Assert.Equal(
            [
                "Alpha",
                "Bravo",
                "Charlie"
            ],
            result.Items
                .Select(issue => issue.Title)
                .ToArray()
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldSortTitleDescending()
    {
        await using var db = CreateContext();

        var project = await CreateProjectAsync(db);

        await db.Issues.AddRangeAsync(
            CreateIssue(
                project.Id,
                "Charlie"
            ),
            CreateIssue(
                project.Id,
                "Alpha"
            ),
            CreateIssue(
                project.Id,
                "Bravo"
            )
        );

        await db.SaveChangesAsync();

        var repository =
            new IssueRepository(db);

        var result =
            await repository.GetPagedByProjectIdAsync(
                project.Id,
                new IssueQueryParameters(
                    SortBy: "title",
                    SortDirection: "desc"
                )
            );

        Assert.Equal(
            [
                "Charlie",
                "Bravo",
                "Alpha"
            ],
            result.Items
                .Select(issue => issue.Title)
                .ToArray()
        );
    }
}