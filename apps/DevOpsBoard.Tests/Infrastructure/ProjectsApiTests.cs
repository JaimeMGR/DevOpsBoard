using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class ProjectsApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public ProjectsApiTests(
        ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Owner_ShouldCreateProject()
    {
        var owner =
            await _helper.CreateUserAsync(
                $"owner-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Owner"
            );

        var ownerToken =
            await _helper.GenerateTokenAsync(
                owner.Id
            );

        using var client =
            CreateAuthenticatedClient(
                ownerToken
            );

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Projects API Test",
                    key = $"P{Guid.NewGuid():N}"[..10],
                    description = "Proyecto de integración"
                }
            );

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode
        );

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        Assert.NotEqual(
            Guid.Empty,
            project.Id
        );

        Assert.Equal(
            "Projects API Test",
            project.Name
        );

        Assert.Equal(
            "P" + project.Key[1..],
            project.Key
        );

        Assert.Equal(
            "Proyecto de integración",
            project.Description
        );

        Assert.Equal(
            owner.Id,
            project.OwnerId
        );

        Assert.NotEqual(
            default,
            project.CreatedAt
        );
    }

    [Fact]
    public async Task Unauthenticated_ShouldNotCreateProject()
    {
        using var client =
            _factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Unauthorized Project",
                    key = $"U{Guid.NewGuid():N}"[..10],
                    description = "No debería crearse"
                }
            );

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldGetProjectById()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        Assert.Equal(
            scenario.ProjectId,
            project.Id
        );

        Assert.Equal(
            "Integration Project",
            project.Name
        );

        Assert.Equal(
            scenario.OwnerId,
            project.OwnerId
        );
    }

    [Fact]
    public async Task AuthenticatedUser_ShouldGetAllProjects()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                "/api/projects"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var projects =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<ProjectDto>
                >();

        Assert.NotNull(projects);

        Assert.Contains(
            projects,
            project =>
                project.Id == scenario.ProjectId
        );
    }

    [Fact]
    public async Task GetProjectById_ShouldReturnNotFoundForUnknownProject()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                $"/api/projects/{Guid.NewGuid()}"
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldUpdateProject()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}",
                new
                {
                    name = "Updated Project",
                    description = "Updated description"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        Assert.Equal(
            scenario.ProjectId,
            project.Id
        );

        Assert.Equal(
            "Updated Project",
            project.Name
        );

        Assert.Equal(
            "Updated description",
            project.Description
        );

        Assert.Equal(
            scenario.OwnerId,
            project.OwnerId
        );

        Assert.Equal(
            scenario.ProjectKey,
            project.Key
        );
    }

    [Fact]
    public async Task Manager_ShouldNotUpdateProject()
    {
        var scenario =
            await CreateScenarioAsync(
                includeManager: true
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken!
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}",
                new
                {
                    name = "Manager Must Not Update",
                    description = "No autorizado"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotUpdateProject()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{scenario.ProjectId}",
                new
                {
                    name = "Outsider Update",
                    description = "No autorizado"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldDeleteProject()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var deleteResponse =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}"
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        var getResponse =
            await client.GetAsync(
                $"/api/projects/{scenario.ProjectId}"
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode
        );
    }

    [Fact]
    public async Task Manager_ShouldNotDeleteProject()
    {
        var scenario =
            await CreateScenarioAsync(
                includeManager: true
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken!
            );

        var response =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotDeleteProject()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.DeleteAsync(
                $"/api/projects/{scenario.ProjectId}"
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task DuplicateProjectKey_ShouldReturnConflict()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Duplicate Key Project",
                    key = scenario.ProjectKey,
                    description = "Clave duplicada"
                }
            );

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode
        );
    }

    [Fact]
    public async Task EmptyProjectName_ShouldReturnBadRequest()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "",
                    key = $"E{Guid.NewGuid():N}"[..10],
                    description = "Nombre vacío"
                }
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task EmptyProjectKey_ShouldReturnBadRequest()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Invalid Key Project",
                    key = "",
                    description = "Clave vacía"
                }
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task ProjectKeyLongerThanTenCharacters_ShouldReturnBadRequest()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Long Key Project",
                    key = "ABCDEFGHIJK",
                    description = "Clave demasiado larga"
                }
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldNormalizeProjectKey()
    {
        var owner =
            await _helper.CreateUserAsync(
                $"owner-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Owner"
            );

        var ownerToken =
            await _helper.GenerateTokenAsync(
                owner.Id
            );

        using var client =
            CreateAuthenticatedClient(
                ownerToken
            );

        var rawKey =
            $"p{Guid.NewGuid():N}"[..10];

        var response =
            await client.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Normalized Key Project",
                    key = $" {rawKey.ToLowerInvariant()} ",
                    description = "Normalización"
                }
            );

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode
        );

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        Assert.Equal(
            rawKey.ToUpperInvariant(),
            project.Key
        );
    }

    [Fact]
    public async Task UpdateUnknownProject_ShouldReturnNotFound()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                $"/api/projects/{Guid.NewGuid()}",
                new
                {
                    name = "Unknown Project",
                    description = "Does not exist"
                }
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task DeleteUnknownProject_ShouldReturnNotFound()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.DeleteAsync(
                $"/api/projects/{Guid.NewGuid()}"
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    private async Task<Scenario>
        CreateScenarioAsync(
            bool includeManager = false)
    {
        var owner =
            await _helper.CreateUserAsync(
                $"owner-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Owner"
            );

        var outsider =
            await _helper.CreateUserAsync(
                $"outsider-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Outsider"
            );

        string? managerId = null;
        string? managerToken = null;

        if (includeManager)
        {
            var manager =
                await _helper.CreateUserAsync(
                    $"manager-{Guid.NewGuid():N}@devopsboard.local",
                    "Integration Manager"
                );

            managerId = manager.Id;

            managerToken =
                await _helper.GenerateTokenAsync(
                    manager.Id
                );
        }

        var ownerToken =
            await _helper.GenerateTokenAsync(
                owner.Id
            );

        var outsiderToken =
            await _helper.GenerateTokenAsync(
                outsider.Id
            );

        using var ownerClient =
            CreateAuthenticatedClient(
                ownerToken
            );

        var projectKey =
            $"P{Guid.NewGuid():N}"[..10];

        var response =
            await ownerClient.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Integration Project",
                    key = projectKey,
                    description = "Proyecto de integración"
                }
            );

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode
        );

        var project =
            await response.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        if (managerId is not null)
        {
            var memberResponse =
                await ownerClient.PostAsJsonAsync(
                    $"/api/projects/{project.Id}/members",
                    new
                    {
                        userId = managerId,
                        role = "Manager"
                    }
                );

            Assert.Equal(
                HttpStatusCode.OK,
                memberResponse.StatusCode
            );
        }

        return new Scenario(
            project.Id,
            project.Key,
            owner.Id,
            ownerToken,
            managerToken,
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
        string ProjectKey,
        string OwnerId,
        string OwnerToken,
        string? ManagerToken,
        string OutsiderToken
    );
}