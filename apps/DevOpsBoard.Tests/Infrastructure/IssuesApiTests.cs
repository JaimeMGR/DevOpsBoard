using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class IssuesApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public IssuesApiTests(ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Owner_ShouldCreateIssue()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues",
                new CreateIssueRequest(
                    "Nueva incidencia",
                    "Descripción de prueba",
                    "High"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode
        );

        var issue =
            await response.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(issue);
        Assert.Equal(
            scenario.ProjectId,
            issue.ProjectId
        );
        Assert.Equal(
            "Nueva incidencia",
            issue.Title
        );
        Assert.Equal(
            "Descripción de prueba",
            issue.Description
        );
        Assert.Equal(
            "Todo",
            issue.Status
        );
        Assert.Equal(
            "High",
            issue.Priority
        );
        Assert.Equal(
            scenario.OwnerId,
            issue.ReporterId
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotCreateIssue()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues",
                new CreateIssueRequest(
                    "Unauthorized issue",
                    null,
                    "Medium"
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Viewer_ShouldReadIssues()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?page=1&pageSize=20"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResult<IssueDto>
                >();

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Outsider_ShouldNotReadIssues()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?page=1&pageSize=20"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldGetIssueById()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var issue =
            await response.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(issue);
        Assert.Equal(
            scenario.IssueId,
            issue.Id
        );
    }

    [Fact]
    public async Task Viewer_ShouldGetIssueById()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var issue =
            await response.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(issue);
        Assert.Equal(
            scenario.IssueId,
            issue.Id
        );
    }

    [Fact]
    public async Task Owner_ShouldUpdateIssue()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}",
                new UpdateIssueRequest(
                    "Título actualizado",
                    "Descripción actualizada",
                    "InProgress",
                    "Critical",
                    null
                )
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var issue =
            await response.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(issue);
        Assert.Equal(
            "Título actualizado",
            issue.Title
        );
        Assert.Equal(
            "Descripción actualizada",
            issue.Description
        );
        Assert.Equal(
            "InProgress",
            issue.Status
        );
        Assert.Equal(
            "Critical",
            issue.Priority
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotUpdateIssue()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}",
                new UpdateIssueRequest(
                    "No autorizado",
                    null,
                    "Done",
                    "Low",
                    null
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldDeleteIssue()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var deleteResponse =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}"
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        var getResponse =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}"
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotDeleteIssue()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Issues_ShouldSupportPagination()
    {
        var scenario = await CreateScenarioAsync(
            createThirdIssue: true
        );

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?page=1&pageSize=2"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResult<IssueDto>
                >();

        Assert.NotNull(result);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Issues_ShouldFilterByPriority()
    {
        var scenario = await CreateScenarioAsync(
            createThirdIssue: true
        );

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?priority=High&page=1&pageSize=20"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResult<IssueDto>
                >();

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        Assert.Equal(
            "High",
            result.Items[0].Priority
        );
    }

    [Fact]
    public async Task Issues_ShouldFilterBySearch()
    {
        var scenario = await CreateScenarioAsync(
            createThirdIssue: true
        );

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?search=JWT&page=1&pageSize=20"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResult<IssueDto>
                >();

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);

        Assert.Contains(
            "JWT",
            result.Items[0].Title,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public async Task Issues_ShouldSortByTitleAscending()
    {
        var scenario = await CreateScenarioAsync(
            createThirdIssue: true
        );

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?sortBy=title&sortDirection=asc&page=1&pageSize=20"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    PagedResult<IssueDto>
                >();

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);

        var titles =
            result.Items
                .Select(issue => issue.Title)
                .ToList();

        Assert.Equal(
            titles.OrderBy(
                title => title,
                StringComparer.OrdinalIgnoreCase
            ),
            titles
        );
    }

    [Fact]
    public async Task Issues_ShouldRejectInvalidPage()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?page=0"
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Issues_ShouldRejectInvalidSortField()
    {
        var scenario = await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues?sortBy=assigneeId"
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    private async Task<Scenario> CreateScenarioAsync(
        bool createThirdIssue = false)
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
                    "Issues Integration Project",
                    $"I{Guid.NewGuid():N}"[..10],
                    "Proyecto para pruebas HTTP de Issues"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            projectResponse.StatusCode
        );

        var project =
            await projectResponse.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

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

        var firstIssueResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/issues",
                new CreateIssueRequest(
                    "Implementar autenticacion JWT",
                    "Issue principal de integracion",
                    "High"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            firstIssueResponse.StatusCode
        );

        var firstIssue =
            await firstIssueResponse.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(firstIssue);

        var secondIssueResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/issues",
                new CreateIssueRequest(
                    "Prueba autorizacion Viewer",
                    "Issue para comprobar permisos",
                    "Medium"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            secondIssueResponse.StatusCode
        );

        if (createThirdIssue)
        {
            var thirdIssueResponse =
                await ownerClient.PostAsJsonAsync(
                    $"/api/projects/{project.Id}/issues",
                    new CreateIssueRequest(
                        "Corregir frontend",
                        "Issue de prioridad baja",
                        "Low"
                    )
                );

            Assert.Equal(
                HttpStatusCode.Created,
                thirdIssueResponse.StatusCode
            );
        }

        return new Scenario(
            project.Id,
            firstIssue.Id,
            owner.Id,
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

    private sealed record Scenario(
        Guid ProjectId,
        Guid IssueId,
        string OwnerId,
        string OwnerToken,
        string ViewerToken,
        string OutsiderToken
    );
}