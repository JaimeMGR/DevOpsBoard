using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Tests;

public class ProjectServiceTests
{
    private const string OwnerId =
        "fc731aa8-dc8d-4eee-bba2-1600523d25c6";

    [Fact]
    public async Task CreateAsync_ShouldCreateProject()
    {
        var repository = new FakeProjectRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new CreateProjectRequest(
            "DevOpsBoard API",
            "DBAPI",
            "Backend principal"
        );

        var result = await service.CreateAsync(
            request,
            OwnerId
        );

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(
            "DevOpsBoard API",
            result.Name
        );
        Assert.Equal(
            "DBAPI",
            result.Key
        );
        Assert.Equal(
            "Backend principal",
            result.Description
        );
        Assert.Equal(
            OwnerId,
            result.OwnerId
        );

        Assert.Single(repository.Projects);
    }

    [Fact]
    public async Task CreateAsync_ShouldNormalizeProjectKey()
    {
        var repository = new FakeProjectRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new CreateProjectRequest(
            "DevOpsBoard API",
            " dbapi ",
            null
        );

        var result = await service.CreateAsync(
            request,
            OwnerId
        );

        Assert.Equal(
            "DBAPI",
            result.Key
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectDuplicateKey()
    {
        var repository = new FakeProjectRepository();

        await repository.AddAsync(
            new Project(
                "Existing Project",
                "DBAPI",
                OwnerId
            )
        );

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new CreateProjectRequest(
            "Another Project",
            "DBAPI",
            null
        );

        await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateAsync(
                request,
                OwnerId
            )
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEmptyName()
    {
        var repository = new FakeProjectRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new CreateProjectRequest(
            "",
            "DBAPI",
            null
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(
                request,
                OwnerId
            )
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEmptyKey()
    {
        var repository = new FakeProjectRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new CreateProjectRequest(
            "DevOpsBoard API",
            "",
            null
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(
                request,
                OwnerId
            )
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProject_WhenAuthorized()
    {
        var repository = new FakeProjectRepository();

        var project = new Project(
            "Old Name",
            "DBAPI",
            OwnerId,
            "Old description"
        );

        await repository.AddAsync(project);

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new UpdateProjectRequest(
            "New Name",
            "New description"
        );

        var result = await service.UpdateAsync(
            project.Id,
            request,
            OwnerId
        );

        Assert.Equal("New Name", result.Name);
        Assert.Equal(
            "New description",
            result.Description
        );

        Assert.Equal(
            "DBAPI",
            result.Key
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var repository = new FakeProjectRepository();

        var project = new Project(
            "My Project",
            "DBAPI",
            OwnerId
        );

        await repository.AddAsync(project);

        var authorizationService =
            new FakeProjectAuthorizationService(false);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        var request = new UpdateProjectRequest(
            "Updated",
            "Updated description"
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.UpdateAsync(
                project.Id,
                request,
                "another-user-id"
            )
        );

        Assert.Equal(
            "My Project",
            project.Name
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProject_WhenAuthorized()
    {
        var repository = new FakeProjectRepository();

        var project = new Project(
            "Project to delete",
            "DBDEL",
            OwnerId
        );

        await repository.AddAsync(project);

        var authorizationService =
            new FakeProjectAuthorizationService(true);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        await service.DeleteAsync(
            project.Id,
            OwnerId
        );

        Assert.Empty(repository.Projects);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var repository = new FakeProjectRepository();

        var project = new Project(
            "Protected project",
            "DBPRO",
            OwnerId
        );

        await repository.AddAsync(project);

        var authorizationService =
            new FakeProjectAuthorizationService(false);

        var service = new ProjectService(
            repository,
            authorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.DeleteAsync(
                project.Id,
                "another-user-id"
            )
        );

        Assert.Single(repository.Projects);
    }

    private sealed class FakeProjectRepository
        : IProjectRepository
    {
        public List<Project> Projects { get; } = [];

        public Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Projects.FirstOrDefault(
                    project => project.Id == id
                )
            );
        }

        public Task<Project?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Projects.FirstOrDefault(
                    project => project.Id == id
                )
            );
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Project>
            >(
                Projects.ToList()
            );
        }

        public Task<bool> ExistsByKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Projects.Any(
                    project => project.Key == key
                )
            );
        }

        public Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            Projects.Add(project);

            return Task.CompletedTask;
        }

        public void Remove(Project project)
        {
            Projects.Remove(project);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProjectAuthorizationService
        : IProjectAuthorizationService
    {
        private readonly bool _canManage;

        public FakeProjectAuthorizationService(
            bool canManage)
        {
            _canManage = canManage;
        }

        public Task<bool> CanManageAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_canManage);
        }
    }
}