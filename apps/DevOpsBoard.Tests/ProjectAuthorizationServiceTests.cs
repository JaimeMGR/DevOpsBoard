using DevOpsBoard.Application.Security;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;
using DevOpsBoard.Infrastructure.Authorization;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevOpsBoard.Tests;

public class ProjectAuthorizationServiceTests
    : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public ProjectAuthorizationServiceTests(
        PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnTrue_WhenUserIsOwner()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "owner"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            project.Id,
            owner.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnTrue_WhenUserIsMember()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "member-owner"
        );

        var member = await CreateUserAsync(
            userManager,
            "member"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        await AddProjectMemberAsync(
            db,
            project.Id,
            member.Id,
            ProjectRole.Viewer
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            project.Id,
            member.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnTrue_WhenUserIsAdmin()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "admin-owner"
        );

        var admin = await CreateUserAsync(
            userManager,
            "admin"
        );

        await EnsureAdminRoleAsync(
            scope.ServiceProvider,
            admin
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            project.Id,
            admin.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnFalse_WhenUserIsOutsider()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "outsider-owner"
        );

        var outsider = await CreateUserAsync(
            userManager,
            "outsider"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            project.Id,
            outsider.Id
        );

        Assert.False(result);
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "missing-owner"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            project.Id,
            $"missing-{Guid.NewGuid():N}"
        );

        Assert.False(result);
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnFalse_WhenProjectIdIsEmpty()
    {
        await using var scope = CreateScope();

        var userManager = GetUserManager(scope);

        var user = await CreateUserAsync(
            userManager,
            "empty-project"
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            Guid.Empty,
            user.Id
        );

        Assert.False(result);
    }

    [Fact]
    public async Task CanViewAsync_ShouldReturnFalse_WhenUserIdIsEmpty()
    {
        await using var scope = CreateScope();

        var service = GetAuthorizationService(scope);

        var result = await service.CanViewAsync(
            Guid.NewGuid(),
            string.Empty
        );

        Assert.False(result);
    }

    [Fact]
    public async Task CanManageAsync_ShouldReturnTrue_WhenUserIsOwner()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "manage-owner"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageAsync(
            project.Id,
            owner.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageAsync_ShouldReturnTrue_WhenUserIsAdmin()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "manage-admin-owner"
        );

        var admin = await CreateUserAsync(
            userManager,
            "manage-admin"
        );

        await EnsureAdminRoleAsync(
            scope.ServiceProvider,
            admin
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageAsync(
            project.Id,
            admin.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageAsync_ShouldReturnFalse_WhenUserIsMember()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "manage-member-owner"
        );

        var member = await CreateUserAsync(
            userManager,
            "manage-member"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        await AddProjectMemberAsync(
            db,
            project.Id,
            member.Id,
            ProjectRole.Manager
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageAsync(
            project.Id,
            member.Id
        );

        Assert.False(result);
    }

    [Fact]
    public async Task CanManageAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "manage-missing-owner"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageAsync(
            project.Id,
            $"missing-{Guid.NewGuid():N}"
        );

        Assert.False(result);
    }

    [Fact]
    public async Task CanManageAsync_ShouldReturnFalse_WhenUserIdIsEmpty()
    {
        await using var scope = CreateScope();

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageAsync(
            Guid.NewGuid(),
            string.Empty
        );

        Assert.False(result);
    }

    private AsyncServiceScope CreateScope()
    {
        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddSingleton(
            _fixture.Options
        );

        services.AddScoped(
            serviceProvider =>
                new DevOpsBoardDbContext(
                    serviceProvider.GetRequiredService<
                        DbContextOptions<DevOpsBoardDbContext>
                    >()
                )
        );

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<
                DevOpsBoardDbContext
            >();

        services.AddScoped<
            ProjectAuthorizationService
        >();

        var provider =
            services.BuildServiceProvider();

        return provider.CreateAsyncScope();
    }
    private static DevOpsBoardDbContext GetDbContext(
        AsyncServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<
                DevOpsBoardDbContext
            >();
    }

    private static UserManager<ApplicationUser>
        GetUserManager(
            AsyncServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>
            >();
    }

    private static ProjectAuthorizationService
        GetAuthorizationService(
            AsyncServiceScope scope)
    {
        return scope.ServiceProvider
            .GetRequiredService<
                ProjectAuthorizationService
            >();
    }

    private static async Task<ApplicationUser>
        CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string prefix)
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")[..12];

        var username =
            $"{prefix}-{suffix}";

        var user =
            new ApplicationUser
            {
                UserName = username,
                Email =
                    $"{username}@integration.local",
                DisplayName =
                    $"{prefix} integration user",
                EmailConfirmed = true,
                IsActive = true
            };

        var result =
            await userManager.CreateAsync(
                user,
                "DevOps123!"
            );

        Assert.True(
            result.Succeeded,
            string.Join(
                "; ",
                result.Errors.Select(
                    error =>
                        $"{error.Code}: {error.Description}"
                )
            )
        );

        return user;
    }

    private static async Task EnsureAdminRoleAsync(
        IServiceProvider serviceProvider,
        ApplicationUser user)
    {
        var roleManager =
            serviceProvider
                .GetRequiredService<
                    RoleManager<IdentityRole>
                >();

        var userManager =
            serviceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>
                >();

        if (!await roleManager.RoleExistsAsync(
                RoleNames.Admin))
        {
            var roleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(
                        RoleNames.Admin
                    )
                );

            Assert.True(
                roleResult.Succeeded,
                string.Join(
                    "; ",
                    roleResult.Errors.Select(
                        error =>
                            $"{error.Code}: {error.Description}"
                    )
                )
            );
        }

        var result =
            await userManager.AddToRoleAsync(
                user,
                RoleNames.Admin
            );

        Assert.True(
            result.Succeeded,
            string.Join(
                "; ",
                result.Errors.Select(
                    error =>
                        $"{error.Code}: {error.Description}"
                    )
                )
            );
    }

    private static async Task<Project>
        CreateProjectAsync(
            DevOpsBoardDbContext db,
            string ownerId)
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpperInvariant();

        var project =
            new Project(
                $"Authorization Test {suffix}",
                $"AUT{suffix}"[..10],
                ownerId
            );

        await db.Projects.AddAsync(
            project
        );

        await db.SaveChangesAsync();

        return project;
    }

    private static async Task AddProjectMemberAsync(
        DevOpsBoardDbContext db,
        Guid projectId,
        string userId,
        ProjectRole role)
    {
        var member =
            new ProjectMember(
                projectId,
                userId,
                role
            );

        await db.ProjectMembers.AddAsync(
            member
        );

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task CanManageSettingsAsync_ShouldReturnTrue_WhenUserIsOwner()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "settings-owner"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageSettingsAsync(
            project.Id,
            owner.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageSettingsAsync_ShouldReturnTrue_WhenUserIsAdmin()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "settings-admin-owner"
        );

        var admin = await CreateUserAsync(
            userManager,
            "settings-admin"
        );

        await EnsureAdminRoleAsync(
            scope.ServiceProvider,
            admin
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageSettingsAsync(
            project.Id,
            admin.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageSettingsAsync_ShouldReturnTrue_WhenUserIsManager()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "settings-manager-owner"
        );

        var manager = await CreateUserAsync(
            userManager,
            "settings-manager"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        await AddProjectMemberAsync(
            db,
            project.Id,
            manager.Id,
            ProjectRole.Manager
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageSettingsAsync(
            project.Id,
            manager.Id
        );

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageSettingsAsync_ShouldReturnFalse_WhenUserIsViewer()
    {
        await using var scope = CreateScope();

        var db = GetDbContext(scope);
        var userManager = GetUserManager(scope);

        var owner = await CreateUserAsync(
            userManager,
            "settings-viewer-owner"
        );

        var viewer = await CreateUserAsync(
            userManager,
            "settings-viewer"
        );

        var project = await CreateProjectAsync(
            db,
            owner.Id
        );

        await AddProjectMemberAsync(
            db,
            project.Id,
            viewer.Id,
            ProjectRole.Viewer
        );

        var service = GetAuthorizationService(scope);

        var result = await service.CanManageSettingsAsync(
            project.Id,
            viewer.Id
        );

        Assert.False(result);
    }
}