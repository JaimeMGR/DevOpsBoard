using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Tests;

public class LabelServiceTests
{
    private const string OwnerId =
        "owner-user-id";

    private const string ManagerId =
        "manager-user-id";

    private const string ViewerId =
        "viewer-user-id";

    [Fact]
    public async Task GetByProjectIdAsync_ShouldReturnLabels_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        labelRepository.Labels.Add(
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            )
        );

        labelRepository.Labels.Add(
            new Label(
                project.Id,
                "security",
                "#00FF00"
            )
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        var result =
            await service.GetByProjectIdAsync(
                project.Id,
                ViewerId
            );

        Assert.Equal(
            2,
            result.Count
        );

        Assert.Contains(
            result,
            label => label.Name == "backend"
        );

        Assert.Contains(
            result,
            label => label.Name == "security"
        );
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: false
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.GetByProjectIdAsync(
                    project.Id,
                    ViewerId
                )
        );
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldThrowNotFound_WhenProjectDoesNotExist()
    {
        var projectId =
            Guid.NewGuid();

        var service =
            CreateService(
                new FakeLabelRepository(),
                new FakeProjectRepository(),
                new FakeProjectAuthorizationService()
            );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.GetByProjectIdAsync(
                    projectId,
                    ViewerId
                )
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnLabel_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        var result =
            await service.GetByIdAsync(
                project.Id,
                label.Id,
                ViewerId
            );

        Assert.NotNull(result);

        Assert.Equal(
            label.Id,
            result.Id
        );

        Assert.Equal(
            project.Id,
            result.ProjectId
        );

        Assert.Equal(
            "backend",
            result.Name
        );

        Assert.Equal(
            "#FF0000",
            result.Color
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenLabelBelongsToAnotherProject()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true
            );

        var project = CreateProject();
        var otherProject = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        await projectRepository.AddAsync(
            otherProject
        );

        var label =
            new Label(
                otherProject.Id,
                "other",
                "#000000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        var result =
            await service.GetByIdAsync(
                project.Id,
                label.Id,
                ViewerId
            );

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateLabel_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        var result =
            await service.CreateAsync(
                project.Id,
                new CreateLabelRequest(
                    " backend ",
                    " #FF0000 "
                ),
                ManagerId
            );

        Assert.NotEqual(
            Guid.Empty,
            result.Id
        );

        Assert.Equal(
            project.Id,
            result.ProjectId
        );

        Assert.Equal(
            "backend",
            result.Name
        );

        Assert.Equal(
            "#FF0000",
            result.Color
        );

        Assert.Single(
            labelRepository.Labels
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: false
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.CreateAsync(
                    project.Id,
                    new CreateLabelRequest(
                        "backend",
                        "#FF0000"
                    ),
                    ViewerId
                )
        );

        Assert.Empty(
            labelRepository.Labels
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflict_WhenNameAlreadyExists()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        labelRepository.Labels.Add(
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            )
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<ConflictException>(
            () =>
                service.CreateAsync(
                    project.Id,
                    new CreateLabelRequest(
                        "backend",
                        "#00FF00"
                    ),
                    ManagerId
                )
        );

        Assert.Single(
            labelRepository.Labels
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidation_WhenNameIsEmpty()
    {
        var projectRepository =
            new FakeProjectRepository();

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var service =
            CreateService(
                new FakeLabelRepository(),
                projectRepository,
                new FakeProjectAuthorizationService()
            );

        await Assert.ThrowsAsync<ValidationException>(
            () =>
                service.CreateAsync(
                    project.Id,
                    new CreateLabelRequest(
                        "",
                        "#FF0000"
                    ),
                    ManagerId
                )
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateLabel_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        var result =
            await service.UpdateAsync(
                project.Id,
                label.Id,
                new UpdateLabelRequest(
                    "security",
                    "#00FF00"
                ),
                ManagerId
            );

        Assert.Equal(
            "security",
            result.Name
        );

        Assert.Equal(
            "#00FF00",
            result.Color
        );

        Assert.Equal(
            "security",
            label.Name
        );

        Assert.Equal(
            "#00FF00",
            label.Color
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowConflict_WhenNameAlreadyExists()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var backend =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var security =
            new Label(
                project.Id,
                "security",
                "#00FF00"
            );

        labelRepository.Labels.Add(
            backend
        );

        labelRepository.Labels.Add(
            security
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<ConflictException>(
            () =>
                service.UpdateAsync(
                    project.Id,
                    backend.Id,
                    new UpdateLabelRequest(
                        "security",
                        "#0000FF"
                    ),
                    ManagerId
                )
        );

        Assert.Equal(
            "backend",
            backend.Name
        );

        Assert.Equal(
            "#FF0000",
            backend.Color
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFound_WhenLabelBelongsToAnotherProject()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();
        var otherProject = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        await projectRepository.AddAsync(
            otherProject
        );

        var label =
            new Label(
                otherProject.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.UpdateAsync(
                    project.Id,
                    label.Id,
                    new UpdateLabelRequest(
                        "security",
                        "#00FF00"
                    ),
                    ManagerId
                )
        );

        Assert.Equal(
            "backend",
            label.Name
        );

        Assert.Equal(
            "#FF0000",
            label.Color
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: false
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.UpdateAsync(
                    project.Id,
                    label.Id,
                    new UpdateLabelRequest(
                        "security",
                        "#00FF00"
                    ),
                    ViewerId
                )
        );

        Assert.Equal(
            "backend",
            label.Name
        );

        Assert.Equal(
            "#FF0000",
            label.Color
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteLabel_WhenAuthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await service.DeleteAsync(
            project.Id,
            label.Id,
            ManagerId
        );

        Assert.Empty(
            labelRepository.Labels
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: false
            );

        var project = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.DeleteAsync(
                    project.Id,
                    label.Id,
                    ViewerId
                )
        );

        Assert.Single(
            labelRepository.Labels
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFound_WhenLabelBelongsToAnotherProject()
    {
        var projectRepository =
            new FakeProjectRepository();

        var labelRepository =
            new FakeLabelRepository();

        var authorizationService =
            new FakeProjectAuthorizationService(
                canView: true,
                canManageSettings: true
            );

        var project = CreateProject();
        var otherProject = CreateProject();

        await projectRepository.AddAsync(
            project
        );

        await projectRepository.AddAsync(
            otherProject
        );

        var label =
            new Label(
                otherProject.Id,
                "backend",
                "#FF0000"
            );

        labelRepository.Labels.Add(
            label
        );

        var service =
            CreateService(
                labelRepository,
                projectRepository,
                authorizationService
            );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.DeleteAsync(
                    project.Id,
                    label.Id,
                    ManagerId
                )
        );

        Assert.Single(
            labelRepository.Labels
        );

        Assert.Equal(
            "backend",
            label.Name
        );
    }

    private static LabelService CreateService(
        ILabelRepository labelRepository,
        IProjectRepository projectRepository,
        IProjectAuthorizationService authorizationService)
    {
        return new LabelService(
            labelRepository,
            projectRepository,
            authorizationService
        );
    }

    private static Project CreateProject()
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpperInvariant();

        return new Project(
            "Test Project",
            $"P{suffix}",
            OwnerId
        );
    }

    private sealed class FakeLabelRepository
        : ILabelRepository
    {
        public List<Label> Labels { get; } = [];

        public Task<Label?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Labels.FirstOrDefault(
                    label => label.Id == id
                )
            );
        }

        public Task<IReadOnlyList<Label>>
            GetByProjectIdAsync(
                Guid projectId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Label>
            >(
                Labels
                    .Where(
                        label =>
                            label.ProjectId == projectId
                    )
                    .OrderBy(
                        label => label.Name
                    )
                    .ToList()
            );
        }

        public Task<bool> ExistsByNameAsync(
            Guid projectId,
            string name,
            Guid? excludingId = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedName =
                name.Trim();

            return Task.FromResult(
                Labels.Any(
                    label =>
                        label.ProjectId == projectId &&
                        label.Name == normalizedName &&
                        (!excludingId.HasValue ||
                         label.Id != excludingId.Value)
                )
            );
        }

        public Task AddAsync(
            Label label,
            CancellationToken cancellationToken = default)
        {
            Labels.Add(label);

            return Task.CompletedTask;
        }

        public void Remove(
            Label label)
        {
            Labels.Remove(label);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
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

        public Task<IReadOnlyList<Project>>
            GetAllAsync(
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

        public void Remove(
            Project project)
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
        private readonly bool _canView;
        private readonly bool _canManageSettings;

        public FakeProjectAuthorizationService(
            bool canView = false,
            bool canManageSettings = false)
        {
            _canView = canView;
            _canManageSettings =
                canManageSettings;
        }

        public Task<bool> CanViewAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canView
            );
        }

        public Task<bool> CanManageSettingsAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canManageSettings
            );
        }

        public Task<bool> CanManageAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                false
            );
        }
    }
}
