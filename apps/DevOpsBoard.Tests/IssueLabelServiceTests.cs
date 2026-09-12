using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class IssueLabelServiceTests
{
    private const string OwnerId =
        "owner-user-id";

    private const string DeveloperId =
        "developer-user-id";

    private const string ViewerId =
        "viewer-user-id";

    [Fact]
    public async Task GetByIssueIdAsync_ShouldReturnLabels_WhenAuthorized()
    {
        var project = CreateProject();
        var issue = CreateIssue(project.Id);

        var backend = new Label(
            project.Id,
            "backend",
            "#FF0000"
        );

        var security = new Label(
            project.Id,
            "security",
            "#00FF00"
        );

        var backendRelation = new IssueLabel(
            issue.Id,
            backend.Id
        );

        var securityRelation = new IssueLabel(
            issue.Id,
            security.Id
        );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        issueLabelRepository.Add(
            backendRelation,
            backend
        );

        issueLabelRepository.Add(
            securityRelation,
            security
        );

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(
                backend,
                security
            ),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canView: true
            ),
            historyRepository
        );

        var result =
            await service.GetByIssueIdAsync(
                project.Id,
                issue.Id,
                ViewerId
            );

        Assert.Equal(
            2,
            result.Count
        );

        Assert.Equal(
            "backend",
            result[0].Name
        );

        Assert.Equal(
            "security",
            result[1].Name
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var project = CreateProject();
        var issue = CreateIssue(project.Id);

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canView: false
            ),
            new FakeIssueHistoryRepository()
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.GetByIssueIdAsync(
                    project.Id,
                    issue.Id,
                    ViewerId
                )
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowNotFound_WhenProjectDoesNotExist()
    {
        var issue =
            CreateIssue(
                Guid.NewGuid()
            );

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(),
            new FakeLabelRepository(),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canView: true
            ),
            new FakeIssueHistoryRepository()
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.GetByIssueIdAsync(
                    Guid.NewGuid(),
                    issue.Id,
                    ViewerId
                )
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowNotFound_WhenIssueDoesNotExist()
    {
        var project =
            CreateProject();

        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(project),
            new FakeLabelRepository(),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canView: true
            ),
            new FakeIssueHistoryRepository()
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.GetByIssueIdAsync(
                    project.Id,
                    Guid.NewGuid(),
                    ViewerId
                )
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowNotFound_WhenIssueBelongsToAnotherProject()
    {
        var project =
            CreateProject();

        var otherProject =
            CreateProject();

        var issue =
            CreateIssue(
                otherProject.Id
            );

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canView: true
            ),
            new FakeIssueHistoryRepository()
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.GetByIssueIdAsync(
                    project.Id,
                    issue.Id,
                    ViewerId
                )
        );
    }

    [Fact]
    public async Task AddAsync_ShouldAssignLabel_WhenAuthorized()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        var result =
            await service.AddAsync(
                project.Id,
                issue.Id,
                label.Id,
                DeveloperId
            );

        Assert.Equal(
            label.Id,
            result.Id
        );

        Assert.Equal(
            label.Name,
            result.Name
        );

        Assert.Single(
            issueLabelRepository.IssueLabels
        );

        var relation =
            issueLabelRepository.IssueLabels[0];

        Assert.Equal(
            issue.Id,
            relation.IssueId
        );

        Assert.Equal(
            label.Id,
            relation.LabelId
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            IssueHistoryAction.LabelAdded,
            history.Action
        );

        Assert.Equal(
            issue.Id,
            history.IssueId
        );

        Assert.Equal(
            DeveloperId,
            history.ActorId
        );

        Assert.Null(
            history.OldValue
        );

        Assert.Equal(
            label.Name,
            history.NewValue
        );

        Assert.NotEqual(
            Guid.Empty,
            history.CorrelationId
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: false
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.AddAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    ViewerId
                )
        );

        Assert.Empty(
            issueLabelRepository.IssueLabels
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFound_WhenLabelDoesNotExist()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            new FakeIssueHistoryRepository()
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.AddAsync(
                    project.Id,
                    issue.Id,
                    Guid.NewGuid(),
                    DeveloperId
                )
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFound_WhenLabelBelongsToAnotherProject()
    {
        var project =
            CreateProject();

        var otherProject =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                otherProject.Id,
                "backend",
                "#FF0000"
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.AddAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    DeveloperId
                )
        );

        Assert.Empty(
            issueLabelRepository.IssueLabels
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowConflict_WhenLabelIsAlreadyAssigned()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var existingRelation =
            new IssueLabel(
                issue.Id,
                label.Id
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        issueLabelRepository.Add(
            existingRelation,
            label
        );

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<ConflictException>(
            () =>
                service.AddAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    DeveloperId
                )
        );

        Assert.Single(
            issueLabelRepository.IssueLabels
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task AddAsync_ShouldThrowNotFound_WhenIssueBelongsToAnotherProject()
    {
        var project =
            CreateProject();

        var otherProject =
            CreateProject();

        var issue =
            CreateIssue(
                otherProject.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.AddAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    DeveloperId
                )
        );

        Assert.Empty(
            issueLabelRepository.IssueLabels
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveLabel_WhenAuthorized()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var relation =
            new IssueLabel(
                issue.Id,
                label.Id
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        issueLabelRepository.Add(
            relation,
            label
        );

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        await service.RemoveAsync(
            project.Id,
            issue.Id,
            label.Id,
            DeveloperId
        );

        Assert.Empty(
            issueLabelRepository.IssueLabels
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            IssueHistoryAction.LabelRemoved,
            history.Action
        );

        Assert.Equal(
            issue.Id,
            history.IssueId
        );

        Assert.Equal(
            DeveloperId,
            history.ActorId
        );

        Assert.Equal(
            label.Name,
            history.OldValue
        );

        Assert.Null(
            history.NewValue
        );

        Assert.NotEqual(
            Guid.Empty,
            history.CorrelationId
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var relation =
            new IssueLabel(
                issue.Id,
                label.Id
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        issueLabelRepository.Add(
            relation,
            label
        );

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: false
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () =>
                service.RemoveAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    ViewerId
                )
        );

        Assert.Single(
            issueLabelRepository.IssueLabels
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowNotFound_WhenRelationDoesNotExist()
    {
        var project =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.RemoveAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    DeveloperId
                )
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowNotFound_WhenLabelBelongsToAnotherProject()
    {
        var project =
            CreateProject();

        var otherProject =
            CreateProject();

        var issue =
            CreateIssue(
                project.Id
            );

        var label =
            new Label(
                otherProject.Id,
                "backend",
                "#FF0000"
            );

        var issueLabelRepository =
            new FakeIssueLabelRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            new FakeIssueRepository(issue),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            issueLabelRepository,
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            historyRepository
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.RemoveAsync(
                    project.Id,
                    issue.Id,
                    label.Id,
                    DeveloperId
                )
        );

        Assert.Empty(
            issueLabelRepository.IssueLabels
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowNotFound_WhenIssueDoesNotExist()
    {
        var project =
            CreateProject();

        var label =
            new Label(
                project.Id,
                "backend",
                "#FF0000"
            );

        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(project),
            new FakeLabelRepository(label),
            new FakeIssueLabelRepository(),
            new FakeIssueAuthorizationService(
                canModify: true
            ),
            new FakeIssueHistoryRepository()
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                service.RemoveAsync(
                    project.Id,
                    Guid.NewGuid(),
                    label.Id,
                    DeveloperId
                )
        );
    }

    private static Project CreateProject()
    {
        var suffix =
            Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpperInvariant();

        return new Project(
            $"Test Project {suffix}",
            $"T{suffix}",
            OwnerId
        );
    }

    private static Issue CreateIssue(
        Guid projectId)
    {
        return new Issue(
            projectId,
            "Test issue",
            OwnerId,
            "Issue description",
            IssuePriority.Medium
        );
    }

    private static IssueLabelService CreateService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        ILabelRepository labelRepository,
        IIssueLabelRepository issueLabelRepository,
        IIssueAuthorizationService issueAuthorizationService,
        IIssueHistoryRepository issueHistoryRepository)
    {
        return new IssueLabelService(
            issueRepository,
            projectRepository,
            labelRepository,
            issueLabelRepository,
            issueAuthorizationService,
            issueHistoryRepository
        );
    }

    private sealed class FakeIssueRepository
        : IIssueRepository
    {
        private readonly List<Issue> _issues;

        public FakeIssueRepository(
            params Issue[] issues)
        {
            _issues =
                issues.ToList();
        }

        public Task<Issue?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _issues.FirstOrDefault(
                    issue => issue.Id == id
                )
            );
        }

        public Task<Issue?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _issues.FirstOrDefault(
                    issue => issue.Id == id
                )
            );
        }

        public Task<IReadOnlyList<Issue>>
            GetByProjectIdAsync(
                Guid projectId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Issue>
            >(
                _issues
                    .Where(
                        issue =>
                            issue.ProjectId == projectId
                    )
                    .ToList()
            );
        }

        public Task<PagedResult<Issue>>
            GetPagedByProjectIdAsync(
                Guid projectId,
                IssueQueryParameters query,
                CancellationToken cancellationToken = default)
        {
            var filtered =
                _issues
                    .Where(
                        issue =>
                            issue.ProjectId == projectId
                    )
                    .ToList();

            var totalCount =
                filtered.Count;

            var items =
                filtered
                    .Skip(
                        (query.Page - 1) *
                        query.PageSize
                    )
                    .Take(
                        query.PageSize
                    )
                    .ToList();

            return Task.FromResult(
                new PagedResult<Issue>(
                    items,
                    query.Page,
                    query.PageSize,
                    totalCount
                )
            );
        }

        public Task AddAsync(
            Issue issue,
            CancellationToken cancellationToken = default)
        {
            _issues.Add(
                issue
            );

            return Task.CompletedTask;
        }

        public void Remove(
            Issue issue)
        {
            _issues.Remove(
                issue
            );
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
        private readonly Project? _project;

        public FakeProjectRepository(
            Project? project = null)
        {
            _project = project;
        }

        public Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Project?>(
                _project is not null &&
                _project.Id == id
                    ? _project
                    : null
            );
        }

        public Task<Project?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Project?>(
                _project is not null &&
                _project.Id == id
                    ? _project
                    : null
            );
        }

        public Task<IReadOnlyList<Project>>
            GetAllAsync(
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Project>
            >(
                _project is null
                    ? []
                    : [_project]
            );
        }

        public Task<bool> ExistsByKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _project is not null &&
                _project.Key == key
            );
        }

        public Task AddAsync(
            Project project,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(
            Project project)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLabelRepository
        : ILabelRepository
    {
        private readonly List<Label> _labels;

        public FakeLabelRepository(
            params Label[] labels)
        {
            _labels =
                labels.ToList();
        }

        public Task<Label?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _labels.FirstOrDefault(
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
                _labels
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
                _labels.Any(
                    label =>
                        label.ProjectId == projectId &&
                        label.Name == normalizedName &&
                        (
                            !excludingId.HasValue ||
                            label.Id != excludingId.Value
                        )
                )
            );
        }

        public Task AddAsync(
            Label label,
            CancellationToken cancellationToken = default)
        {
            _labels.Add(
                label
            );

            return Task.CompletedTask;
        }

        public void Remove(
            Label label)
        {
            _labels.Remove(
                label
            );
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeIssueLabelRepository
        : IIssueLabelRepository
    {
        private readonly List<IssueLabel> _issueLabels = [];

        private readonly Dictionary<Guid, Label> _labels =
            [];

        public IReadOnlyList<IssueLabel> IssueLabels =>
            _issueLabels;

        public void Add(
            IssueLabel issueLabel,
            Label label)
        {
            _issueLabels.Add(
                issueLabel
            );

            _labels[label.Id] =
                label;
        }

        public Task<IReadOnlyList<Label>>
            GetLabelsByIssueIdAsync(
                Guid issueId,
                CancellationToken cancellationToken = default)
        {
            var result =
                _issueLabels
                    .Where(
                        relation =>
                            relation.IssueId == issueId
                    )
                    .Select(
                        relation =>
                            _labels.TryGetValue(
                                relation.LabelId,
                                out var label
                            )
                                ? label
                                : null
                    )
                    .Where(
                        label => label is not null
                    )
                    .Cast<Label>()
                    .OrderBy(
                        label => label.Name
                    )
                    .ToList();

            return Task.FromResult<
                IReadOnlyList<Label>
            >(result);
        }

        public Task<bool> ExistsAsync(
            Guid issueId,
            Guid labelId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _issueLabels.Any(
                    relation =>
                        relation.IssueId == issueId &&
                        relation.LabelId == labelId
                )
            );
        }

        public Task AddAsync(
            IssueLabel issueLabel,
            CancellationToken cancellationToken = default)
        {
            _issueLabels.Add(
                issueLabel
            );

            return Task.CompletedTask;
        }

        public Task<IssueLabel?> GetAsync(
            Guid issueId,
            Guid labelId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _issueLabels.FirstOrDefault(
                    relation =>
                        relation.IssueId == issueId &&
                        relation.LabelId == labelId
                )
            );
        }

        public void Remove(
            IssueLabel issueLabel)
        {
            _issueLabels.Remove(
                issueLabel
            );
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeIssueHistoryRepository
        : IIssueHistoryRepository
    {
        public List<IssueHistory> Histories { get; } = [];

        public Task AddAsync(
            IssueHistory history,
            CancellationToken cancellationToken = default)
        {
            Histories.Add(
                history
            );

            return Task.CompletedTask;
        }

        public Task<
            IReadOnlyList<IssueHistoryReadModel>
        > GetByIssueIdAsync(
            Guid issueId,
            CancellationToken cancellationToken = default)
        {
            var result =
                Histories
                    .Where(
                        history =>
                            history.IssueId == issueId
                    )
                    .Select(
                        history =>
                            new IssueHistoryReadModel(
                                history.Id,
                                history.IssueId,
                                history.ActorId,
                                "Actor",
                                "actor@devopsboard.local",
                                history.Action.ToString(),
                                history.OldValue,
                                history.NewValue,
                                history.CorrelationId,
                                history.CreatedAt
                            )
                    )
                    .ToList();

            return Task.FromResult<
                IReadOnlyList<IssueHistoryReadModel>
            >(result);
        }
    }

    private sealed class FakeIssueAuthorizationService
        : IIssueAuthorizationService
    {
        private readonly bool _canView;
        private readonly bool _canCreate;
        private readonly bool _canModify;
        private readonly bool _canDelete;

        public FakeIssueAuthorizationService(
            bool canView = true,
            bool canCreate = true,
            bool canModify = true,
            bool canDelete = true)
        {
            _canView = canView;
            _canCreate = canCreate;
            _canModify = canModify;
            _canDelete = canDelete;
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

        public Task<bool> CanCreateAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canCreate
            );
        }

        public Task<bool> CanModifyAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canModify
            );
        }

        public Task<bool> CanDeleteAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canDelete
            );
        }
    }
}