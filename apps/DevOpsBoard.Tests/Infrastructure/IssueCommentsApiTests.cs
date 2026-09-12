using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Tests.Infrastructure;

namespace DevOpsBoard.Tests;

public class IssueCommentsApiTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly ApiTestHelper _helper;

    public IssueCommentsApiTests(
        ApiFactory factory)
    {
        _factory = factory;
        _helper = new ApiTestHelper(factory);
    }

    [Fact]
    public async Task Owner_ShouldCreateComment()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                CommentsUrl(scenario),
                new
                {
                    content = "Comentario creado por Owner"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var comment =
            await response.Content
                .ReadFromJsonAsync<IssueCommentDto>();

        Assert.NotNull(comment);

        Assert.NotEqual(
            Guid.Empty,
            comment.Id
        );

        Assert.Equal(
            scenario.OwnerId,
            comment.AuthorId
        );

        Assert.Equal(
            "Comentario creado por Owner",
            comment.Content
        );
    }

    [Fact]
    public async Task Viewer_ShouldReadComments()
    {
        var scenario =
            await CreateScenarioAsync();

        using var ownerClient =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var createResponse =
            await ownerClient.PostAsJsonAsync(
                CommentsUrl(scenario),
                new
                {
                    content = "Comentario visible"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            createResponse.StatusCode
        );

        using var viewerClient =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await viewerClient.GetAsync(
                CommentsUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var comments =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueCommentDto>
                >();

        Assert.NotNull(comments);
        Assert.Single(comments);

        Assert.Equal(
            "Comentario visible",
            comments[0].Content
        );

        Assert.Equal(
            scenario.OwnerId,
            comments[0].AuthorId
        );
    }

    [Fact]
    public async Task Viewer_ShouldNotCreateComment()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.ViewerToken
            );

        var response =
            await client.PostAsJsonAsync(
                CommentsUrl(scenario),
                new
                {
                    content = "No debería crearse"
                }
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task Developer_ShouldCreateComment()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.DeveloperToken
            );

        var response =
            await client.PostAsJsonAsync(
                CommentsUrl(scenario),
                new
                {
                    content = "Comentario del Developer"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var comment =
            await response.Content
                .ReadFromJsonAsync<IssueCommentDto>();

        Assert.NotNull(comment);

        Assert.Equal(
            scenario.DeveloperId,
            comment.AuthorId
        );

        Assert.Equal(
            "Comentario del Developer",
            comment.Content
        );
    }

    [Fact]
    public async Task Developer_ShouldUpdateOwnComment()
    {
        var scenario =
            await CreateScenarioAsync();

        var comment =
            await CreateCommentAsync(
                scenario.DeveloperToken,
                scenario,
                "Contenido original"
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.DeveloperToken
            );

        var response =
            await client.PatchAsJsonAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                ),
                new
                {
                    content = "Contenido actualizado"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var updated =
            await response.Content
                .ReadFromJsonAsync<IssueCommentDto>();

        Assert.NotNull(updated);

        Assert.Equal(
            comment.Id,
            updated.Id
        );

        Assert.Equal(
            scenario.DeveloperId,
            updated.AuthorId
        );

        Assert.Equal(
            "Contenido actualizado",
            updated.Content
        );
    }

    [Fact]
    public async Task Developer_ShouldNotUpdateAnotherDeveloperComment()
    {
        var scenario =
            await CreateScenarioAsync(
                includeSecondDeveloper: true
            );

        var comment =
            await CreateCommentAsync(
                scenario.SecondDeveloperToken!,
                scenario,
                "Comentario ajeno"
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.DeveloperToken
            );

        var response =
            await client.PatchAsJsonAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                ),
                new
                {
                    content = "Intento no autorizado"
                }
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
                CommentsUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var comments =
            await verifyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueCommentDto>
                >();

        Assert.NotNull(comments);
        Assert.Single(comments);

        Assert.Equal(
            "Comentario ajeno",
            comments[0].Content
        );
    }

    [Fact]
    public async Task Manager_ShouldUpdateAnotherUsersComment()
    {
        var scenario =
            await CreateScenarioAsync();

        var comment =
            await CreateCommentAsync(
                scenario.DeveloperToken,
                scenario,
                "Comentario de Developer"
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var response =
            await client.PatchAsJsonAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                ),
                new
                {
                    content = "Editado por Manager"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var updated =
            await response.Content
                .ReadFromJsonAsync<IssueCommentDto>();

        Assert.NotNull(updated);

        Assert.Equal(
            scenario.DeveloperId,
            updated.AuthorId
        );

        Assert.Equal(
            "Editado por Manager",
            updated.Content
        );
    }

    [Fact]
    public async Task Developer_ShouldDeleteOwnComment()
    {
        var scenario =
            await CreateScenarioAsync();

        var comment =
            await CreateCommentAsync(
                scenario.DeveloperToken,
                scenario,
                "Comentario para eliminar"
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.DeveloperToken
            );

        var response =
            await client.DeleteAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode
        );

        var verifyResponse =
            await client.GetAsync(
                CommentsUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var comments =
            await verifyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueCommentDto>
                >();

        Assert.NotNull(comments);
        Assert.Empty(comments);
    }

    [Fact]
    public async Task Developer_ShouldNotDeleteAnotherUsersComment()
    {
        var scenario =
            await CreateScenarioAsync(
                includeSecondDeveloper: true
            );

        var comment =
            await CreateCommentAsync(
                scenario.SecondDeveloperToken!,
                scenario,
                "Comentario ajeno"
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.DeveloperToken
            );

        var response =
            await client.DeleteAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );

        var verifyResponse =
            await client.GetAsync(
                CommentsUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var comments =
            await verifyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueCommentDto>
                >();

        Assert.NotNull(comments);
        Assert.Single(comments);

        Assert.Equal(
            "Comentario ajeno",
            comments[0].Content
        );
    }

    [Fact]
    public async Task Manager_ShouldDeleteAnotherUsersComment()
    {
        var scenario =
            await CreateScenarioAsync();

        var comment =
            await CreateCommentAsync(
                scenario.DeveloperToken,
                scenario,
                "Comentario de Developer"
            );

        using var client =
            CreateAuthenticatedClient(
                scenario.ManagerToken
            );

        var response =
            await client.DeleteAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode
        );

        var verifyResponse =
            await client.GetAsync(
                CommentsUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode
        );

        var comments =
            await verifyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueCommentDto>
                >();

        Assert.NotNull(comments);
        Assert.Empty(comments);
    }

    [Fact]
    public async Task Outsider_ShouldNotReadComments()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OutsiderToken
            );

        var response =
            await client.GetAsync(
                CommentsUrl(scenario)
            );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }

    [Fact]
    public async Task EmptyComment_ShouldReturnBadRequest()
    {
        var scenario =
            await CreateScenarioAsync();

        using var client =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var response =
            await client.PostAsJsonAsync(
                CommentsUrl(scenario),
                new
                {
                    content = ""
                }
            );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    [Fact]
    public async Task CommentHistory_ShouldContainCreateEditAndDelete()
    {
        var scenario =
            await CreateScenarioAsync();

        var comment =
            await CreateCommentAsync(
                scenario.DeveloperToken,
                scenario,
                "Comentario original"
            );

        using var developerClient =
            CreateAuthenticatedClient(
                scenario.DeveloperToken
            );

        var updateResponse =
            await developerClient.PatchAsJsonAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                ),
                new
                {
                    content = "Comentario editado"
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode
        );

        var deleteResponse =
            await developerClient.DeleteAsync(
                CommentUrl(
                    scenario,
                    comment.Id
                )
            );

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode
        );

        using var ownerClient =
            CreateAuthenticatedClient(
                scenario.OwnerToken
            );

        var historyResponse =
            await ownerClient.GetAsync(
                $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/history"
            );

        Assert.Equal(
            HttpStatusCode.OK,
            historyResponse.StatusCode
        );

        var history =
            await historyResponse.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<IssueHistoryDto>
                >();

        Assert.NotNull(history);

        Assert.Contains(
            history,
            entry =>
                entry.Action == "Created"
        );

        Assert.Contains(
            history,
            entry =>
                entry.Action == "CommentAdded" &&
                entry.NewValue == "Comentario original"
        );

        Assert.Contains(
            history,
            entry =>
                entry.Action == "CommentEdited" &&
                entry.OldValue == "Comentario original" &&
                entry.NewValue == "Comentario editado"
        );

        Assert.Contains(
            history,
            entry =>
                entry.Action == "CommentDeleted" &&
                entry.OldValue == "Comentario editado"
        );
    }

    private async Task<IssueCommentDto>
        CreateCommentAsync(
            string token,
            Scenario scenario,
            string content)
    {
        using var client =
            CreateAuthenticatedClient(
                token
            );

        var response =
            await client.PostAsJsonAsync(
                CommentsUrl(scenario),
                new
                {
                    content
                }
            );

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var comment =
            await response.Content
                .ReadFromJsonAsync<IssueCommentDto>();

        Assert.NotNull(comment);

        return comment;
    }

    private async Task<Scenario>
        CreateScenarioAsync(
            bool includeSecondDeveloper = false)
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

        var secondDeveloper =
            includeSecondDeveloper
                ? await _helper.CreateUserAsync(
                    $"developer2-{Guid.NewGuid():N}@devopsboard.local",
                    "Second Integration Developer"
                )
                : null;

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

        string? secondDeveloperToken = null;

        if (secondDeveloper is not null)
        {
            secondDeveloperToken =
                await _helper.GenerateTokenAsync(
                    secondDeveloper.Id
                );
        }

        using var ownerClient =
            CreateAuthenticatedClient(
                ownerToken
            );

        var projectResponse =
            await ownerClient.PostAsJsonAsync(
                "/api/projects",
                new
                {
                    name = "Issue Comments Integration",
                    key = $"C{Guid.NewGuid():N}"[..10],
                    description =
                        "Proyecto para pruebas HTTP de comentarios"
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

        if (secondDeveloper is not null)
        {
            await AddMemberAsync(
                ownerClient,
                project.Id,
                secondDeveloper.Id,
                "Developer"
            );
        }

        var issueResponse =
            await ownerClient.PostAsJsonAsync(
                $"/api/projects/{project.Id}/issues",
                new
                {
                    title = "Issue de comentarios",
                    description =
                        "Issue para pruebas de comentarios",
                    priority = "Medium"
                }
            );

        Assert.Equal(
            HttpStatusCode.Created,
            issueResponse.StatusCode
        );

        var issue =
            await issueResponse.Content
                .ReadFromJsonAsync<IssueDto>();

        Assert.NotNull(issue);

        return new Scenario(
            project.Id,
            issue.Id,
            owner.Id,
            manager.Id,
            developer.Id,
            viewer.Id,
            outsider.Id,
            ownerToken,
            managerToken,
            developerToken,
            viewerToken,
            outsiderToken,
            secondDeveloper?.Id,
            secondDeveloperToken
        );
    }

    private async Task AddMemberAsync(
        HttpClient ownerClient,
        Guid projectId,
        string userId,
        string role)
    {
        var response =
            await ownerClient.PostAsJsonAsync(
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

    private static string CommentsUrl(
        Scenario scenario)
    {
        return
            $"/api/projects/{scenario.ProjectId}/issues/{scenario.IssueId}/comments";
    }

    private static string CommentUrl(
        Scenario scenario,
        Guid commentId)
    {
        return
            $"{CommentsUrl(scenario)}/{commentId}";
    }

    private sealed record Scenario(
        Guid ProjectId,
        Guid IssueId,
        string OwnerId,
        string ManagerId,
        string DeveloperId,
        string ViewerId,
        string OutsiderId,
        string OwnerToken,
        string ManagerToken,
        string DeveloperToken,
        string ViewerToken,
        string OutsiderToken,
        string? SecondDeveloperId,
        string? SecondDeveloperToken
    );
}