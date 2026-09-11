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

        var service = CreateService(
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

            var service = CreateService(
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

        var service = CreateService(
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

        var service = CreateService(
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

        var service = CreateService(
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
    public async Task CreateAsync_ShouldThrowForbidden_WhenNotAuthorized()
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

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService,
            historyRepository
        );

        var request = new CreateIssueRequest(
            "Unauthorized issue",
            null,
            "Medium"
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.CreateAsync(
                ProjectId,
                request,
                ReporterId
            )
        );

        Assert.Empty(
            issueRepository.Issues
        );

        Assert.Empty(
            historyRepository.Histories
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

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByProjectIdAsync(
            ProjectId,
            ReporterId
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

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetByProjectIdAsync(
            OtherProjectId,
            ReporterId
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

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByIdAsync(
                ProjectId,
                issue.Id,
                ReporterId
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

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByIdAsync(
            ProjectId,
            Guid.NewGuid(),
            ReporterId
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

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByIdAsync(
            ProjectId,
            Guid.NewGuid(),
            ReporterId
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldReturnFirstPage()
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
                ReporterId
            )
        );

        await issueRepository.AddAsync(
            new Issue(
                ProjectId,
                "Issue 2",
                ReporterId
            )
        );

        await issueRepository.AddAsync(
            new Issue(
                ProjectId,
                "Issue 3",
                ReporterId
            )
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    Page: 1,
                    PageSize: 2
                ),
                ReporterId
            );

        Assert.Equal(
            2,
            result.Items.Count
        );

        Assert.Equal(
            3,
            result.TotalCount
        );

        Assert.Equal(
            1,
            result.Page
        );

        Assert.Equal(
            2,
            result.PageSize
        );

        Assert.Equal(
            2,
            result.TotalPages
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldReturnSecondPage()
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
                ReporterId
            )
        );

        await issueRepository.AddAsync(
            new Issue(
                ProjectId,
                "Issue 2",
                ReporterId
            )
        );

        await issueRepository.AddAsync(
            new Issue(
                ProjectId,
                "Issue 3",
                ReporterId
            )
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    Page: 2,
                    PageSize: 2
                ),
                ReporterId
            );

        Assert.Single(
            result.Items
        );

        Assert.Equal(
            3,
            result.TotalCount
        );

        Assert.Equal(
            2,
            result.Page
        );

        Assert.Equal(
            2,
            result.PageSize
        );

        Assert.Equal(
            2,
            result.TotalPages
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldRejectInvalidPage()
    {
        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(),
            new FakeUserRepository(ReporterId),
            new FakeIssueAuthorizationService(true)
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    Page: 0,
                    PageSize: 20
                ),
                ReporterId
            )
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldRejectInvalidPageSize()
    {
        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(),
            new FakeUserRepository(ReporterId),
            new FakeIssueAuthorizationService(true)
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    Page: 1,
                    PageSize: 101
                ),
                ReporterId
            )
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldRejectInvalidStatus()
    {
        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(),
            new FakeUserRepository(ReporterId),
            new FakeIssueAuthorizationService(true)
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    Status: "Cancelled"
                ),
                ReporterId
            )
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldRejectInvalidPriority()
    {
        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(),
            new FakeUserRepository(ReporterId),
            new FakeIssueAuthorizationService(true)
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    Priority: "Urgent"
                ),
                ReporterId
            )
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldRejectInvalidSortBy()
    {
        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(),
            new FakeUserRepository(ReporterId),
            new FakeIssueAuthorizationService(true)
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    SortBy: "AssigneeId"
                ),
                ReporterId
            )
        );
    }

    [Fact]
    public async Task GetPagedByProjectIdAsync_ShouldRejectInvalidSortDirection()
    {
        var service = CreateService(
            new FakeIssueRepository(),
            new FakeProjectRepository(),
            new FakeUserRepository(ReporterId),
            new FakeIssueAuthorizationService(true)
        );

        await Assert.ThrowsAsync<ValidationException>(
            () => service.GetPagedByProjectIdAsync(
                ProjectId,
                new IssueQueryParameters(
                    SortDirection: "sideways"
                ),
                ReporterId
            )
        );
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

        var service = CreateService(
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

        var service = CreateService(
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

        var service = CreateService(
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

        var service = CreateService(
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

        var service = CreateService(
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
    public async Task DeleteAsync_ShouldSoftDeleteIssue_WhenAuthorized()
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

        var historyRepository =
            new FakeIssueHistoryRepository();

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
            authorizationService,
            historyRepository
        );

        await service.DeleteAsync(
            ProjectId,
            issue.Id,
            ReporterId
        );

        Assert.Single(
            issueRepository.Issues
        );

        Assert.True(
            issue.IsDeleted
        );

        Assert.NotNull(
            issue.DeletedAt
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            issue.Id,
            history.IssueId
        );

        Assert.Equal(
            ReporterId,
            history.ActorId
        );

        Assert.Equal(
            IssueHistoryAction.Deleted,
            history.Action
        );

        Assert.Null(
            history.OldValue
        );

        Assert.Null(
            history.NewValue
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

        var service = CreateService(
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

    [Fact]
    public async Task CreateAsync_ShouldCreateHistory()
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

        var historyRepository =
            new FakeIssueHistoryRepository();

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService,
            historyRepository
        );

        var request = new CreateIssueRequest(
            "Issue with history",
            "Test history creation",
            "High"
        );

        var result = await service.CreateAsync(
            ProjectId,
            request,
            ReporterId
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            result.Id,
            history.IssueId
        );

        Assert.Equal(
            ReporterId,
            history.ActorId
        );

        Assert.Equal(
            IssueHistoryAction.Created,
            history.Action
        );

        Assert.Null(
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
    public async Task UpdateAsync_ShouldCreateHistoryForChangedFields()
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

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issue = new Issue(
            ProjectId,
            "Old title",
            ReporterId,
            "Old description",
            IssuePriority.Low
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService,
            historyRepository
        );

        var request = new UpdateIssueRequest(
            "New title",
            "New description",
            "InProgress",
            "Critical",
            ReporterId
        );

        await service.UpdateAsync(
            ProjectId,
            issue.Id,
            request,
            ReporterId
        );

        Assert.Equal(
            5,
            historyRepository.Histories.Count
        );

        Assert.Contains(
            historyRepository.Histories,
            history =>
                history.Action ==
                IssueHistoryAction.TitleChanged &&
                history.OldValue == "Old title" &&
                history.NewValue == "New title"
        );

        Assert.Contains(
            historyRepository.Histories,
            history =>
                history.Action ==
                IssueHistoryAction.DescriptionChanged &&
                history.OldValue == "Old description" &&
                history.NewValue == "New description"
        );

        Assert.Contains(
            historyRepository.Histories,
            history =>
                history.Action ==
                IssueHistoryAction.StatusChanged &&
                history.OldValue == "Todo" &&
                history.NewValue == "InProgress"
        );

        Assert.Contains(
            historyRepository.Histories,
            history =>
                history.Action ==
                IssueHistoryAction.PriorityChanged &&
                history.OldValue == "Low" &&
                history.NewValue == "Critical"
        );

        Assert.Contains(
            historyRepository.Histories,
            history =>
                history.Action ==
                IssueHistoryAction.Assigned &&
                history.OldValue == null &&
                history.NewValue == ReporterId
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUseSameCorrelationIdForSingleOperation()
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

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issue = new Issue(
            ProjectId,
            "Old title",
            ReporterId,
            "Old description",
            IssuePriority.Low
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService,
            historyRepository
        );

        var request = new UpdateIssueRequest(
            "New title",
            "New description",
            "InProgress",
            "Critical",
            ReporterId
        );

        await service.UpdateAsync(
            ProjectId,
            issue.Id,
            request,
            ReporterId
        );

        Assert.NotEmpty(
            historyRepository.Histories
        );

        var correlationIds =
            historyRepository.Histories
                .Select(
                    history => history.CorrelationId
                )
                .Distinct()
                .ToList();

        Assert.Single(
            correlationIds
        );

        Assert.NotEqual(
            Guid.Empty,
            correlationIds[0]
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldCreateUnassignedHistory()
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

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.AssignTo(
            ReporterId
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService,
            historyRepository
        );

        var request = new UpdateIssueRequest(
            "Issue",
            null,
            "Todo",
            "Medium",
            null
        );

        await service.UpdateAsync(
            ProjectId,
            issue.Id,
            request,
            ReporterId
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            IssueHistoryAction.Unassigned,
            history.Action
        );

        Assert.Equal(
            ReporterId,
            history.OldValue
        );

        Assert.Null(
            history.NewValue
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowForbidden_WhenUserIsNotAuthorized()
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

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetByIdAsync(
                ProjectId,
                issue.Id,
                ReporterId
            )
        );
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldReturnIssues_WhenUserIsAuthorized()
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
            "Visible issue",
            ReporterId
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService
        );

        var result =
            await service.GetByProjectIdAsync(
                ProjectId,
                ReporterId
            );

        Assert.Single(result);

        Assert.Equal(
            issue.Id,
            result[0].Id
        );
    }
    private static IssueService CreateService(
    IIssueRepository issueRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IIssueAuthorizationService authorizationService,
    IIssueHistoryRepository? historyRepository = null)
    {
        return new IssueService(
            issueRepository,
            projectRepository,
            userRepository,
            authorizationService,
            historyRepository
                ?? new FakeIssueHistoryRepository()
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

    private sealed class FakeIssueHistoryRepository
    : IIssueHistoryRepository
    {
        public List<IssueHistory> Histories { get; } = [];

        public Task AddAsync(
            IssueHistory history,
            CancellationToken cancellationToken = default)
        {
            Histories.Add(history);

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
        private readonly bool _canModify;

        public Task<bool> CanViewAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canModify
            );
        }
        public FakeIssueAuthorizationService(
            bool canModify)
        {
            _canModify = canModify;
        }

        public Task<bool> CanCreateAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canModify
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
                _canModify
            );
        }
    }
}