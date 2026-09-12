using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class LabelsApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public LabelsApiTests(
        ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Viewer_ShouldReadProjectLabels()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/labels"
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
    public async Task Viewer_ShouldReadLabelById()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/labels/{scenario.LabelId}"
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

        Assert.Equal(
            "#123456",
            label.Color
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotCreateLabel()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/labels",
                new CreateLabelRequest(
                    "Viewer Created",
                    "#654321"
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotUpdateLabel()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}/labels/{scenario.LabelId}",
                new UpdateLabelRequest(
                    "Unauthorized Update",
                    "#654321"
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );

        using var ownerClient =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var verifyResponse =
            await ownerClient.GetAsync(
                $"/api/projects/{scenario.ProjectId}/labels/{scenario.LabelId}"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var label =
            await verifyResponse.Content
                .ReadFromJsonAsync<LabelDto>();

        Assert.NotNull(label);

        Assert.Equal(
            scenario.LabelName,
            label.Name
        );

        Assert.Equal(
            "#123456",
            label.Color
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotDeleteLabel()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}/labels/{scenario.LabelId}"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );

        using var ownerClient =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var verifyResponse =
            await ownerClient.GetAsync(
                $"/api/projects/{scenario.ProjectId}/labels/{scenario.LabelId}"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var label =
            await verifyResponse.Content
                .ReadFromJsonAsync<LabelDto>();

        Assert.NotNull(label);

        Assert.Equal(
            scenario.LabelId,
            label.Id
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotReadProjectLabels()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}/labels"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    private async Task<Scenario> CreateScenarioAsync()
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

        var tokenParts =
        new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(ownerToken);

        Assert.Equal(
            "DevOpsBoard.Tests",
            tokenParts.Issuer
        );

        Assert.Contains(
            tokenParts.Claims,
            claim =>
                claim.Type ==
                System.Security.Claims.ClaimTypes.NameIdentifier &&
                claim.Value == owner.Id
        );

        using var ownerClient =
            CreateAuthenticatedClient(
                ownerToken
            );

        var projectResponse =
            await ownerClient.PostAsJsonAsync(
                "/api/projects",
                new CreateProjectRequest(
                    "Integration Labels Project",
                    $"P{Guid.NewGuid():N}"[..10],
                    "Proyecto para pruebas HTTP de Labels"
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

        var labelName =
            $"Label-{Guid.NewGuid():N}"[..20];

        var labelResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/labels",
                new CreateLabelRequest(
                    labelName,
                    "#123456"
                )
            );

        Assert.Equal(
            HttpStatusCode.Created,
            labelResponse.StatusCode
        );

        var label =
            await labelResponse.Content
                .ReadFromJsonAsync<LabelDto>();

        Assert.NotNull(label);

        var viewerToken =
            await _helper.GenerateTokenAsync(
                viewer.Id
            );

        var outsiderToken =
            await _helper.GenerateTokenAsync(
                outsider.Id
            );

        return new Scenario(
            project.Id,
            label.Id,
            label.Name,
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
        Guid LabelId,
        string LabelName,
        string OwnerToken,
        string ViewerToken,
        string OutsiderToken
    );
}