using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class IssueHistoryServiceTests
{
    private static readonly Guid ProjectId =
        Guid.NewGuid();

    private static readonly Guid IssueId =
        Guid.NewGuid();

    private static readonly Guid OtherIssueId =
        Guid.NewGuid();

    private static readonly Guid CorrelationId =
        Guid.NewGuid();

    private const string ActorId =
        "actor-user-id";

    [Fact]
    public async Task GetByIssueIdAsync_ShouldReturnHistory_WhenUserIsAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Issue",
            ActorId
        );

        await issueRepository.AddAsync(
            issue
        );

        historyRepository.Histories.Add(
            new IssueHistoryReadModel(
                Guid.NewGuid(),
                issue.Id,
                ActorId,
                "Developer",
                "developer@devopsboard.local",
                "StatusChanged",
                "Todo",
                "InProgress",
                CorrelationId,
                DateTime.UtcNow
            )
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            historyRepository,
            authorizationService
        );

        var result =
            await service.GetByIssueIdAsync(
                ProjectId,
                issue.Id,
                ActorId
            );

        Assert.Single(result);

        var history = result[0];

        Assert.Equal(
            ActorId,
            history.ActorId
        );

        Assert.Equal(
            "Developer",
            history.ActorDisplayName
        );

        Assert.Equal(
            "developer@devopsboard.local",
            history.ActorEmail
        );

        Assert.Equal(
            "StatusChanged",
            history.Action
        );

        Assert.Equal(
            "Todo",
            history.OldValue
        );

        Assert.Equal(
            "InProgress",
            history.NewValue
        );

        Assert.Equal(
            CorrelationId,
            history.CorrelationId
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowForbidden_WhenUserIsNotAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var authorizationService =
            new FakeIssueAuthorizationService(false);

        var issue = new Issue(
            ProjectId,
            "Protected issue",
            ActorId
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            historyRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetByIssueIdAsync(
                ProjectId,
                issue.Id,
                ActorId
            )
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowNotFound_WhenProjectDoesNotExist()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = CreateService(
            issueRepository,
            projectRepository,
            historyRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIssueIdAsync(
                Guid.NewGuid(),
                IssueId,
                ActorId
            )
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowNotFound_WhenIssueDoesNotExist()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = CreateService(
            issueRepository,
            projectRepository,
            historyRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIssueIdAsync(
                ProjectId,
                IssueId,
                ActorId
            )
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowNotFound_WhenIssueBelongsToAnotherProject()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();


        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            Guid.NewGuid(),
            "Issue",
            ActorId
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            historyRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByIssueIdAsync(
                ProjectId,
                issue.Id,
                ActorId
            )
        );
    }
    private static IssueHistoryService CreateService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IIssueHistoryRepository historyRepository,
        IIssueAuthorizationService authorizationService)
    {
        return new IssueHistoryService(
            issueRepository,
            projectRepository,
            historyRepository,
            authorizationService
        );
    }

    private sealed class FakeIssueRepository
        : IIssueRepository
    {
        public List<Issue> Issues { get; } = [];

        public Task<Issue?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Issues.FirstOrDefault(
                    issue => issue.Id == id
                )
            );
        }

        public Task<Issue?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Issues.FirstOrDefault(
                    issue => issue.Id == id
                )
            );
        }

        public Task<IReadOnlyList<Issue>>
            GetByProjectIdAsync(
                Guid projectId,
                CancellationToken cancellationToken = default)
        {
            var result = Issues
                .Where(
                    issue => issue.ProjectId == projectId
                )
                .ToList();

            return Task.FromResult<
                IReadOnlyList<Issue>
            >(result);
        }

                public Task<PagedResult<Issue>>
            GetPagedByProjectIdAsync(
                Guid projectId,
                IssueQueryParameters query,
                CancellationToken cancellationToken = default)
        {
            var filtered = Issues
                .Where(
                    issue => issue.ProjectId == projectId
                )
                .OrderByDescending(
                    issue => issue.CreatedAt
                )
                .ToList();

            var totalCount = filtered.Count;

            var items = filtered
                .Skip(
                    (query.Page - 1) * query.PageSize
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
            Issues.Add(issue);

            return Task.CompletedTask;
        }

        public void Remove(
            Issue issue)
        {
            Issues.Remove(issue);
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
        private readonly Project _project =
            new(
                "DevOpsBoard API",
                "DBAPI",
                ActorId
            );

        public Task<Project?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Project?>(
                id == ProjectId
                    ? _project
                    : null
            );
        }

        public Task<Project?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Project?>(
                id == ProjectId
                    ? _project
                    : null
            );
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<Project>
            >(
                [_project]
            );
        }

        public Task<bool> ExistsByKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                key == _project.Key
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

    private sealed class FakeIssueHistoryRepository
        : IIssueHistoryRepository
    {
        public List<IssueHistoryReadModel> Histories { get; } = [];

        public List<IssueHistory> Entities { get; } = [];

        public Task AddAsync(
            IssueHistory history,
            CancellationToken cancellationToken = default)
        {
            Entities.Add(history);

            return Task.CompletedTask;
        }

        public Task<
            IReadOnlyList<IssueHistoryReadModel>
        > GetByIssueIdAsync(
            Guid issueId,
            CancellationToken cancellationToken = default)
        {
            var result = Histories
                .Where(
                    history => history.IssueId == issueId
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

        public FakeIssueAuthorizationService(
            bool canView)
        {
            _canView = canView;
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
                _canView
            );
        }

        public Task<bool> CanModifyAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canView
            );
        }

        public Task<bool> CanDeleteAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canView
            );
        }
    }
}