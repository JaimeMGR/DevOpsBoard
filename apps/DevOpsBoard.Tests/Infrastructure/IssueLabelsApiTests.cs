using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class IssueLabelsApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public IssueLabelsApiTests(
        ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Manager_ShouldAddLabelToIssue()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    scenario.LabelId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var label =
            await response.Content
                .ReadFromJsonAsync<LabelDto>();

        Assert.NotNull(label);

        Assert.Equal(
            scenario.LabelId,
            label.Id
        );

        Assert.Equal(
            scenario.LabelName,
            label.Name
        );
    }

    [Fact]
    public async Task Manager_ShouldReadIssueLabels()
    {
        var scenario =
            await CreateScenarioAsync();

        using var managerClient =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var addResponse =
            await managerClient.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    scenario.LabelId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            addResponse.StatusCode
        );

        var response =
            await managerClient.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var labels =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<LabelDto>
                >();

        Assert.NotNull(labels);
        Assert.Single(labels);

        Assert.Equal(
            scenario.LabelId,
            labels[0].Id
        );
    }

    [Fact]
    public async Task Manager_ShouldRemoveLabelFromIssue()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var addResponse =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    scenario.LabelId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            addResponse.StatusCode
        );

        var deleteResponse =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels/{scenario.LabelId}"
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        var getResponse =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode
        );

        var labels =
            await getResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<LabelDto>
                >();

        Assert.NotNull(labels);
        Assert.Empty(labels);
    }

    [Fact]
    public async Task Viewer_ShouldReadIssueLabels()
    {
        var scenario =
            await CreateScenarioAsync();

        using var managerClient =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var addResponse =
            await managerClient.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    scenario.LabelId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            addResponse.StatusCode
        );

        using var viewerClient =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await viewerClient.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var labels =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<LabelDto>
                >();

        Assert.NotNull(labels);
        Assert.Single(labels);

        Assert.Equal(
            scenario.LabelId,
            labels[0].Id
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotAddLabelToIssue()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    scenario.LabelId
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotRemoveLabelFromIssue()
    {
        var scenario =
            await CreateScenarioAsync();

        using var managerClient =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var addResponse =
            await managerClient.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    scenario.LabelId
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            addResponse.StatusCode
        );

        using var viewerClient =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await viewerClient.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels/{scenario.LabelId}"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );

        var verifyResponse =
            await managerClient.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var labels =
            await verifyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<LabelDto>
                >();

        Assert.NotNull(labels);
        Assert.Single(labels);
    }

    [Fact]
    public async Task Outsider_ShouldNotReadIssueLabels()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task ShouldRejectLabelFromAnotherProject()
    {
        var scenario =
            await CreateScenarioAsync();

        var foreignProjectResponse =
        await CreateProjectAsync(
            scenario.OwnerToken,
            "Foreign Label Project",
            $"F{Guid.NewGuid():N}"[..10]
        );

        var foreignProject =
            await foreignProjectResponse.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(foreignProject);

        var foreignLabelResponse =
            await CreateLabelAsync(
                scenario.OwnerToken,
                foreignProject.Id
            );

        var foreignLabel =
            await foreignLabelResponse.Content
                .ReadFromJsonAsync<LabelDto>();

        Assert.NotNull(foreignLabel);

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels",
                new AssignIssueLabelRequest(
                    foreignLabel.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );

        var labelsResponse =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            labelsResponse.StatusCode
        );

        var labels =
            await labelsResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<LabelDto>
                >();

        Assert.NotNull(labels);
        Assert.Empty(labels);
    }

    private async Task<Scenario>
        CreateScenarioAsync()
    {
        var owner =
            await _helper.CreateUserAsync(
                $"owner-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Owner"
            );

        var manager =
            await _helper.CreateUserAsync(
                $"manager-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Manager"
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

        var managerToken =
            await _helper.GenerateTokenAsync(
                manager.Id
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
            await CreateProjectAsync(
                ownerToken,
                "Issue Labels Integration",
                $"P{Guid.NewGuid():N}"[..10]
            );

        Assert.Equal(
            HttpStatusCode.Created,
            projectResponse.StatusCode
        );

        var project =
            await projectResponse.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        var managerMemberResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/members",
                new AddProjectMemberRequest(
                    manager.Id,
                    "Manager"
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            managerMemberResponse.StatusCode
        );

        var viewerMemberResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/members",
                new AddProjectMemberRequest(
                    viewer.Id,
                    "Viewer"
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            viewerMemberResponse.StatusCode
        );

        var issueResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/issues",
                new CreateIssueRequest(
                    "Issue for label integration",
                    "Issue created for API integration tests",
                    "High"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            issueResponse.StatusCode
        );

        var issue =
            await issueResponse.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(issue);

        var labelResponse =
            await CreateLabelAsync(
                ownerToken,
                project.Id
            );

        Assert.Equal(
            HttpStatusCode.Created,
            labelResponse.StatusCode
        );

        var label =
            await labelResponse.Content
                .ReadFromJsonAsync<LabelDto>();

        Assert.NotNull(label);

        return new Scenario(
            project.Id,
            issue.Id,
            label.Id,
            label.Name,
            ownerToken,
            managerToken,
            viewerToken,
            outsiderToken
        );
    }

    private async Task<HttpResponseMessage>
        CreateProjectAsync(
            string token,
            string name,
            string key)
    {
        using var client =
            CreateAuthenticatedClient(
                token
            );

        return await client.PostAsJsonAsync(
            "/api/projects",
            new CreateProjectRequest(
                name,
                key,
                "Proyecto para pruebas HTTP"
            )
        );
    }

    private async Task<HttpResponseMessage>
        CreateLabelAsync(
            string token,
            Guid projectId)
    {
        using var client =
            CreateAuthenticatedClient(
                token
            );

        return await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/labels",
            new CreateLabelRequest(
                $"Label-{Guid.NewGuid():N}"[..20],
                "#123456"
            )
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

    private sealed record Scenario(
        Guid ProjectId,
        Guid IssueId,
        Guid LabelId,
        string LabelName,
        string OwnerToken,
        string ManagerToken,
        string ViewerToken,
        string OutsiderToken
    );
}