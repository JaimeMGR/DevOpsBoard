using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class IssueHistoryApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public IssueHistoryApiTests(
        ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Owner_ShouldReadIssueHistory()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                HistoryUrl(
                    scenario.ProjectId,
                    scenario.IssueId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var history =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueHistoryDto>
                >();

        Assert.NotNull(history);
        Assert.Equal(6, history.Count);

        Assert.Equal(
            "Created",
            history[0].Action
        );

        Assert.Equal(
            "TitleChanged",
            history[1].Action
        );

        Assert.Equal(
            "DescriptionChanged",
            history[2].Action
        );

        Assert.Equal(
            "StatusChanged",
            history[3].Action
        );

        Assert.Equal(
            "PriorityChanged",
            history[4].Action
        );

        Assert.Equal(
            "Assigned",
            history[5].Action
        );
    }

    [Fact]
    public async Task Owner_ShouldReceiveCompleteHistoryData()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                HistoryUrl(
                    scenario.ProjectId,
                    scenario.IssueId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var history =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueHistoryDto>
                >();

        Assert.NotNull(history);
        Assert.Equal(6, history.Count);

        var created =
            history[0];

        Assert.Equal(
            scenario.OwnerId,
            created.ActorId
        );

        Assert.Equal(
            "Integration Owner",
            created.ActorDisplayName
        );

        Assert.Equal(
            scenario.OwnerEmail,
            created.ActorEmail
        );

        Assert.Null(
            created.OldValue
        );

        Assert.Null(
            created.NewValue
        );

        Assert.NotEqual(
            Guid.Empty,
            created.Id
        );

        Assert.NotEqual(
            Guid.Empty,
            created.CorrelationId
        );

        var titleChanged =
            history.Single(
                entry =>
                    entry.Action ==
                    "TitleChanged"
            );

        Assert.Equal(
            "Original title",
            titleChanged.OldValue
        );

        Assert.Equal(
            "Updated title",
            titleChanged.NewValue
        );

        var descriptionChanged =
            history.Single(
                entry =>
                    entry.Action ==
                    "DescriptionChanged"
            );

        Assert.Equal(
            "Original description",
            descriptionChanged.OldValue
        );

        Assert.Equal(
            "Updated description",
            descriptionChanged.NewValue
        );

        var statusChanged =
            history.Single(
                entry =>
                    entry.Action ==
                    "StatusChanged"
            );

        Assert.Equal(
            "Todo",
            statusChanged.OldValue
        );

        Assert.Equal(
            "InProgress",
            statusChanged.NewValue
        );

        var priorityChanged =
            history.Single(
                entry =>
                    entry.Action ==
                    "PriorityChanged"
            );

        Assert.Equal(
            "Medium",
            priorityChanged.OldValue
        );

        Assert.Equal(
            "Critical",
            priorityChanged.NewValue
        );

        var assigned =
            history.Single(
                entry =>
                    entry.Action ==
                    "Assigned"
            );

        Assert.Null(
            assigned.OldValue
        );

        Assert.Equal(
            scenario.OwnerId,
            assigned.NewValue
        );
    }

    [Fact]
    public async Task Viewer_ShouldReadIssueHistory()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.GetAsync(
                HistoryUrl(
                    scenario.ProjectId,
                    scenario.IssueId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var history =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueHistoryDto>
                >();

        Assert.NotNull(history);
        Assert.Equal(
            6,
            history.Count
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotReadIssueHistory()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.GetAsync(
                HistoryUrl(
                    scenario.ProjectId,
                    scenario.IssueId
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task IssueFromAnotherProject_ShouldReturnNotFound()
    {
        var scenario =
            await CreateScenarioAsync();

        using var ownerClient =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var foreignProjectResponse =
            await ownerClient.PostAsJsonAsync(
                "/api/projects",
                new CreateProjectRequest(
                    "Foreign History Project",
                    $"F{Guid.NewGuid():N}"[..10],
                    "Proyecto externo"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            foreignProjectResponse.StatusCode
        );

        var foreignProject =
            await foreignProjectResponse.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(
            foreignProject
        );

        var foreignIssueResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{foreignProject.Id}/issues",
                new CreateIssueRequest(
                    "Foreign issue",
                    "Issue from another project",
                    "Low"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            foreignIssueResponse.StatusCode
        );

        var foreignIssue =
            await foreignIssueResponse.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(
            foreignIssue
        );

        var response =
            await ownerClient.GetAsync(
                HistoryUrl(
                    scenario.ProjectId,
                    foreignIssue.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task InvalidIds_ShouldReturnBadRequest()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{Guid.Empty}/issues/{scenario.IssueId}/history"
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    private async Task<Scenario>
        CreateScenarioAsync()
    {
        var owner =
            await _helper.CreateUserAsync(
                $"owner-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Owner"
            );

        var viewer =
            await _helper.CreateUserAsync(
                $"viewer-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Viewer"
            );

        var outsider =
            await _helper.CreateUserAsync(
                $"outsider-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Outsider"
            );

        var ownerToken =
            await _helper.GenerateTokenAsync(
                owner.Id
            );

        var viewerToken =
            await _helper.GenerateTokenAsync(
                viewer.Id
            );

        var outsiderToken =
            await _helper.GenerateTokenAsync(
                outsider.Id
            );

        using var ownerClient =
            CreateAuthenticatedClient(
                ownerToken
            );

        var projectResponse =
            await ownerClient.PostAsJsonAsync(
                "/api/projects",
                new CreateProjectRequest(
                    "Issue History Project",
                    $"H{Guid.NewGuid():N}"[..10],
                    "Proyecto para pruebas del historial"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            projectResponse.StatusCode
        );

        var project =
            await projectResponse.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(
            project
        );

        var memberResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/members",
                new AddProjectMemberRequest(
                    viewer.Id,
                    "Viewer"
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            memberResponse.StatusCode
        );

        var issueResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/issues",
                new CreateIssueRequest(
                    "Original title",
                    "Original description",
                    "Medium"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            issueResponse.StatusCode
        );

        var issue =
            await issueResponse.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(
            issue
        );

        var updateResponse =
            await ownerClient.PatchAsJsonAsync(
                $"/api/projects/{project.Id}/issues/{issue.Id}",
                new UpdateIssueRequest(
                    "Updated title",
                    "Updated description",
                    "InProgress",
                    "Critical",
                    owner.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode
        );

        return new Scenario(
            project.Id,
            issue.Id,
            owner.Id,
            owner.Email,
            ownerToken,
            viewerToken,
            outsiderToken
        );
    }

    private HttpClient CreateAuthenticatedClient(
        string token)
    {
        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        return client;
    }

    private static string HistoryUrl(
        Guid projectId,
        Guid issueId)
    {
        return
            $"/api/projects/{projectId}/issues/{issueId}/history";
    }

    private sealed record Scenario(
        Guid ProjectId,
        Guid IssueId,
        string OwnerId,
        string OwnerEmail,
        string OwnerToken,
        string ViewerToken,
        string OutsiderToken
    );
}