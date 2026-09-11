using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class IssueServiceTests
{
    private static readonly Guid ProjectId =
        Guid.NewGuid();

    private static readonly Guid OtherProjectId =
        Guid.NewGuid();

    private const string ReporterId =
        "reporter-user-id";

    [Fact]
    public async Task CreateAsync_ShouldCreateIssue()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new CreateIssueRequest(
            "Implement login",
            "Implement JWT login flow",
            "High"
        );

        var result = await service.CreateAsync(
            ProjectId,
            request,
            ReporterId
        );

        Assert.NotEqual(
            Guid.Empty,
            result.Id
        );

        Assert.Equal(
            ProjectId,
            result.ProjectId
        );

        Assert.Equal(
            "Implement login",
            result.Title
        );

        Assert.Equal(
            "Implement JWT login flow",
            result.Description
        );

        Assert.Equal(
            "Todo",
            result.Status
        );

        Assert.Equal(
            "High",
            result.Priority
        );

        Assert.Equal(
            ReporterId,
            result.ReporterId
        );

        Assert.Null(
            result.AssigneeId
        );

        Assert.Single(
            issueRepository.Issues
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldAcceptAllValidPriorities()
    {
        var priorities = Enum.GetValues<IssuePriority>();

        foreach (var priority in priorities)
        {
            var issueRepository =
                new FakeIssueRepository();

            var projectRepository =
                new FakeProjectRepository();

            var userRepository =
                new FakeUserRepository(
                    ReporterId
                );

            var authorizationService =
                new FakeIssueAuthorizationService(true);

            var service = new IssueService(
                issueRepository,
                projectRepository,
                userRepository,
                authorizationService
            );

            var request = new CreateIssueRequest(
                "Issue",
                null,
                priority.ToString()
            );

            var result = await service.CreateAsync(
                ProjectId,
                request,
                ReporterId
            );

            Assert.Equal(
                priority.ToString(),
                result.Priority
            );
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidPriority()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new CreateIssueRequest(
            "Issue",
            null,
            "Urgent"
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.CreateAsync(
                ProjectId,
                request,
                ReporterId
            )
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingProject()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new CreateIssueRequest(
            "Issue",
            null,
            "Medium"
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(
                OtherProjectId,
                request,
                ReporterId
            )
        );

        Assert.Empty(
            issueRepository.Issues
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingReporter()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new CreateIssueRequest(
            "Issue",
            null,
            "Medium"
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(
                ProjectId,
                request,
                "unknown-user-id"
            )
        );

        Assert.Empty(
            issueRepository.Issues
        );
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldReturnIssues()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        await issueRepository.AddAsync(
            new Issue(
                ProjectId,
                "Issue 1",
                ReporterId,
                "Description 1",
                IssuePriority.Low
            )
        );

        await issueRepository.AddAsync(
            new Issue(
                ProjectId,
                "Issue 2",
                ReporterId,
                "Description 2",
                IssuePriority.High
            )
        );

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByProjectIdAsync(
                ProjectId
            );

        Assert.Equal(
            2,
            result.Count
        );

        Assert.Contains(
            result,
            issue => issue.Title == "Issue 1"
        );

        Assert.Contains(
            result,
            issue => issue.Title == "Issue 2"
        );
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldRejectMissingProject()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByProjectIdAsync(
                OtherProjectId
            )
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnIssue()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Implement API",
            ReporterId,
            "Create endpoint",
            IssuePriority.Medium
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByIdAsync(
                ProjectId,
                issue.Id
            );

        Assert.NotNull(result);

        Assert.Equal(
            issue.Id,
            result!.Id
        );

        Assert.Equal(
            "Implement API",
            result.Title
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIssueBelongsToAnotherProject()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            OtherProjectId,
            "Other issue",
            ReporterId
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByIdAsync(
                ProjectId,
                issue.Id
            );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIssueDoesNotExist()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByIdAsync(
                ProjectId,
                Guid.NewGuid()
            );

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateIssue_WhenAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Old title",
            ReporterId,
            "Old description",
            IssuePriority.Low
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new UpdateIssueRequest(
            "New title",
            "New description",
            "InProgress",
            "High",
            ReporterId
        );

        var result = await service.UpdateAsync(
            ProjectId,
            issue.Id,
            request,
            ReporterId
        );

        Assert.Equal(
            "New title",
            result.Title
        );

        Assert.Equal(
            "New description",
            result.Description
        );

        Assert.Equal(
            "InProgress",
            result.Status
        );

        Assert.Equal(
            "High",
            result.Priority
        );

        Assert.Equal(
            ReporterId,
            result.AssigneeId
        );

        Assert.Equal(
            ProjectId,
            result.ProjectId
        );

        Assert.Equal(
            ReporterId,
            result.ReporterId
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(false);

        var issue = new Issue(
            ProjectId,
            "Original title",
            ReporterId
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new UpdateIssueRequest(
            "Should not update",
            "Should not update",
            "Done",
            "Critical",
            null
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.UpdateAsync(
                ProjectId,
                issue.Id,
                request,
                "unauthorized-user"
            )
        );

        Assert.Equal(
            "Original title",
            issue.Title
        );

        Assert.Equal(
            IssueStatus.Todo,
            issue.Status
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectInvalidStatus()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new UpdateIssueRequest(
            "Issue",
            null,
            "Cancelled",
            "Medium",
            null
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.UpdateAsync(
                ProjectId,
                issue.Id,
                request,
                ReporterId
            )
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectInvalidPriority()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new UpdateIssueRequest(
            "Issue",
            null,
            "Done",
            "Urgent",
            null
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.UpdateAsync(
                ProjectId,
                issue.Id,
                request,
                ReporterId
            )
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectUnknownAssignee()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var request = new UpdateIssueRequest(
            "Issue",
            null,
            "InProgress",
            "Medium",
            "unknown-assignee"
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateAsync(
                ProjectId,
                issue.Id,
                request,
                ReporterId
            )
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteIssue_WhenAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(true);

        var issue = new Issue(
            ProjectId,
            "Issue to delete",
            ReporterId
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        await service.DeleteAsync(
            ProjectId,
            issue.Id,
            ReporterId
        );

        Assert.Empty(
            issueRepository.Issues
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbidden_WhenNotAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var userRepository =
            new FakeUserRepository(
                ReporterId
            );

        var authorizationService =
            new FakeIssueAuthorizationService(false);

        var issue = new Issue(
            ProjectId,
            "Protected issue",
            ReporterId
        );

        await issueRepository.AddAsync(issue);

        var service = new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.DeleteAsync(
                ProjectId,
                issue.Id,
                "unauthorized-user"
            )
        );

        Assert.Single(
            issueRepository.Issues
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
                .OrderByDescending(
                    issue => issue.CreatedAt
                )
                .ToList();

            return Task.FromResult<
                IReadOnlyList<Issue>
            >(result);
        }

        public Task AddAsync(
            Issue issue,
            CancellationToken cancellationToken = default)
        {
            Issues.Add(issue);

            return Task.CompletedTask;
        }

        public void Remove(Issue issue)
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
                ReporterId
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

        public void Remove(Project project)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository
        : IUserRepository
    {
        private readonly UserSummaryDto _user;

        public FakeUserRepository(
            string userId)
        {
            _user = new UserSummaryDto(
                userId,
                "Reporter",
                "reporter@devopsboard.local"
            );
        }

        public Task<bool> ExistsAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                userId == _user.Id
            );
        }

        public Task<UserSummaryDto?> GetSummaryByIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                userId == _user.Id
                    ? _user
                    : null
            );
        }
    }

    private sealed class FakeIssueAuthorizationService
        : IIssueAuthorizationService
    {
        private readonly bool _canModify;

        public FakeIssueAuthorizationService(
            bool canModify)
        {
            _canModify = canModify;
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
                _canModify
            );
        }
    }
}