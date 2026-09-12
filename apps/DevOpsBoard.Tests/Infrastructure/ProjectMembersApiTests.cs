using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class ProjectMembersApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public ProjectMembersApiTests(
        ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Owner_ShouldReadProjectMembers()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.GetAsync(
                MembersUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var members =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<ProjectMemberDto>
                >();

        Assert.NotNull(members);
        Assert.Equal(3, members.Count);

        Assert.Contains(
            members,
            member =>
                member.UserId == scenario.ManagerId &&
                member.Role == "Manager"
        );

        Assert.Contains(
            members,
            member =>
                member.UserId == scenario.DeveloperId &&
                member.Role == "Developer"
        );

        Assert.Contains(
            members,
            member =>
                member.UserId == scenario.ViewerId &&
                member.Role == "Viewer"
        );
    }

    [Fact]
    public async Task Owner_ShouldAddMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = scenario.OutsiderId,
                    role = "Developer"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var member =
            await response.Content
                .ReadFromJsonAsync<ProjectMemberDto>();

        Assert.NotNull(member);

        Assert.Equal(
            scenario.OutsiderId,
            member.UserId
        );

        Assert.Equal(
            "Developer",
            member.Role
        );
    }

    [Fact]
    public async Task Manager_ShouldAddMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = scenario.OutsiderId,
                    role = "Viewer"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var member =
            await response.Content
                .ReadFromJsonAsync<ProjectMemberDto>();

        Assert.NotNull(member);

        Assert.Equal(
            scenario.OutsiderId,
            member.UserId
        );

        Assert.Equal(
            "Viewer",
            member.Role
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotAddMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = scenario.OutsiderId,
                    role = "Developer"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotAddMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = scenario.OutsiderId,
                    role = "Developer"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task AddingExistingMember_ShouldReturnConflict()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = scenario.ViewerId,
                    role = "Developer"
                }
            );

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode
        );
    }

    [Fact]
    public async Task AddingMemberWithInvalidRole_ShouldReturnBadRequest()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = scenario.OutsiderId,
                    role = "Administrator"
                }
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task AddingUnknownUser_ShouldReturnNotFound()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                MembersUrl(scenario),
                new
                {
                    userId = Guid.NewGuid().ToString(),
                    role = "Viewer"
                }
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldUpdateMemberRole()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                MemberUrl(
                    scenario,
                    scenario.DeveloperId
                ),
                new
                {
                    role = "Manager"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var member =
            await response.Content
                .ReadFromJsonAsync<ProjectMemberDto>();

        Assert.NotNull(member);

        Assert.Equal(
            scenario.DeveloperId,
            member.UserId
        );

        Assert.Equal(
            "Manager",
            member.Role
        );
    }

    [Fact]
    public async Task Manager_ShouldUpdateMemberRole()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                MemberUrl(
                    scenario,
                    scenario.DeveloperId
                ),
                new
                {
                    role = "Viewer"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var member =
            await response.Content
                .ReadFromJsonAsync<ProjectMemberDto>();

        Assert.NotNull(member);

        Assert.Equal(
            "Viewer",
            member.Role
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotUpdateMemberRole()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                MemberUrl(
                    scenario,
                    scenario.DeveloperId
                ),
                new
                {
                    role = "Manager"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotUpdateMemberRole()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.PatchAsJsonAsync(
                MemberUrl(
                    scenario,
                    scenario.DeveloperId
                ),
                new
                {
                    role = "Manager"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Owner_ShouldRemoveMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.DeleteAsync(
                MemberUrl(
                    scenario,
                    scenario.ViewerId
                )
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode
        );

        var membersResponse =
            await client.GetAsync(
                MembersUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            membersResponse.StatusCode
        );

        var members =
            await membersResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<ProjectMemberDto>
                >();

        Assert.NotNull(members);

        Assert.DoesNotContain(
            members,
            member =>
                member.UserId == scenario.ViewerId
        );
    }

    [Fact]
    public async Task Manager_ShouldRemoveMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var response =
            await client.DeleteAsync(
                MemberUrl(
                    scenario,
                    scenario.ViewerId
                )
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotRemoveMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.DeleteAsync(
                MemberUrl(
                    scenario,
                    scenario.DeveloperId
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Outsider_ShouldNotRemoveMember()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.DeleteAsync(
                MemberUrl(
                    scenario,
                    scenario.DeveloperId
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task RemovingUnknownMember_ShouldReturnNotFound()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.DeleteAsync(
                MemberUrl(
                    scenario,
                    Guid.NewGuid().ToString()
                )
            );

        Assert.Equal(
            HttpStatusCode.NotFound,
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

        var manager =
            await _helper.CreateUserAsync(
                $"manager-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Manager"
            );

        var developer =
            await _helper.CreateUserAsync(
                $"developer-{Guid.NewGuid():N}@devopsboard.local",
                "Integration Developer"
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

        var developerToken =
            await _helper.GenerateTokenAsync(
                developer.Id
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
                new
                {
                    name = "Project Members Integration",
                    key = $"M{Guid.NewGuid():N}"[..10],
                    description =
                        "Proyecto para pruebas HTTP de miembros"
                }
            );

        Assert.Equal(
            HttpStatusCode.Created,
            projectResponse.StatusCode
        );

        var project =
            await projectResponse.Content
                .ReadFromJsonAsync<ProjectDto>();

        Assert.NotNull(project);

        await AddMemberAsync(
            ownerClient,
            project.Id,
            manager.Id,
            "Manager"
        );

        await AddMemberAsync(
            ownerClient,
            project.Id,
            developer.Id,
            "Developer"
        );

        await AddMemberAsync(
            ownerClient,
            project.Id,
            viewer.Id,
            "Viewer"
        );

        return new Scenario(
            project.Id,
            owner.Id,
            manager.Id,
            developer.Id,
            viewer.Id,
            outsider.Id,
            ownerToken,
            managerToken,
            developerToken,
            viewerToken,
            outsiderToken
        );
    }

    private static async Task AddMemberAsync(
        HttpClient client,
        Guid projectId,
        string userId,
        string role)
    {
        var response =
            await client.PostAsJsonAsync(
                $"/api/projects/{projectId}/members",
                new
                {
                    userId,
                    role
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
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

    private static string MembersUrl(
        Scenario scenario)
    {
        return
            $"/api/projects/{scenario.ProjectId}/members";
    }

    private static string MemberUrl(
        Scenario scenario,
        string userId)
    {
        return
            $"{MembersUrl(scenario)}/{userId}";
    }

    private sealed record Scenario(
        Guid ProjectId,
        string OwnerId,
        string ManagerId,
        string DeveloperId,
        string ViewerId,
        string OutsiderId,
        string OwnerToken,
        string ManagerToken,
        string DeveloperToken,
        string ViewerToken,
        string OutsiderToken
    );
}