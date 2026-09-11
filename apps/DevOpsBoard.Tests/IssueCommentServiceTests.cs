using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Application.Services;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class IssueCommentServiceTests
{
    private static readonly Guid ProjectId =
        Guid.NewGuid();

    private static readonly Guid IssueId =
        Guid.NewGuid();

    private const string OwnerId =
        "owner-user-id";

    private const string DeveloperId =
        "developer-user-id";

    private const string OtherDeveloperId =
        "other-developer-user-id";

    [Fact]
    public async Task CreateAsync_ShouldCreateCommentAndHistory_WhenAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: true,
                canModify: true,
                canDelete: true
            );

        var issue = CreateIssue();

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        var request =
            new CreateIssueCommentRequest(
                "Comentario de prueba"
            );

        var result =
            await service.CreateAsync(
                ProjectId,
                issue.Id,
                request,
                DeveloperId
            );

        Assert.NotEqual(
            Guid.Empty,
            result.Id
        );

        Assert.Equal(
            DeveloperId,
            result.AuthorId
        );

        Assert.Equal(
            "Comentario de prueba",
            result.Content
        );

        Assert.Single(
            commentRepository.Comments
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            IssueHistoryAction.CommentAdded,
            history.Action
        );

        Assert.Null(
            history.OldValue
        );

        Assert.Equal(
            "Comentario de prueba",
            history.NewValue
        );

        Assert.Equal(
            DeveloperId,
            history.ActorId
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForbidden_WhenUnauthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: false,
                canModify: false,
                canDelete: false
            );

        var issue = CreateIssue();

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        var request =
            new CreateIssueCommentRequest(
                "No debería crearse"
            );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.CreateAsync(
                ProjectId,
                issue.Id,
                request,
                DeveloperId
            )
        );

        Assert.Empty(
            commentRepository.Comments
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldReturnComments_WhenAuthorized()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService();

        var issue = CreateIssue();

        await issueRepository.AddAsync(
            issue
        );

        commentRepository.ReadModels.Add(
            new IssueCommentReadModel(
                Guid.NewGuid(),
                issue.Id,
                DeveloperId,
                "Developer",
                "developer@devopsboard.local",
                "Comentario visible",
                DateTime.UtcNow,
                DateTime.UtcNow
            )
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        var result =
            await service.GetByIssueIdAsync(
                ProjectId,
                issue.Id,
                DeveloperId
            );

        Assert.Single(
            result
        );

        Assert.Equal(
            DeveloperId,
            result[0].AuthorId
        );

        Assert.Equal(
            "Developer",
            result[0].AuthorDisplayName
        );

        Assert.Equal(
            "developer@devopsboard.local",
            result[0].AuthorEmail
        );
    }

    [Fact]
    public async Task GetByIssueIdAsync_ShouldThrowForbidden_WhenUserCannotView()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService(
                canView: false
            );

        var commentAuthorizationService =
            new FakeCommentAuthorizationService();

        var issue = CreateIssue();

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.GetByIssueIdAsync(
                ProjectId,
                issue.Id,
                DeveloperId
            )
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOwnComment_WhenDeveloperIsAuthor()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: true,
                canModify: true,
                canDelete: true
            );

        var issue = CreateIssue();

        var comment = new IssueComment(
            issue.Id,
            DeveloperId,
            "Contenido original"
        );

        commentRepository.Entities.Add(
            comment
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        var result =
            await service.UpdateAsync(
                ProjectId,
                issue.Id,
                comment.Id,
                new UpdateIssueCommentRequest(
                    "Contenido actualizado"
                ),
                DeveloperId
            );

        Assert.Equal(
            "Contenido actualizado",
            result.Content
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            IssueHistoryAction.CommentEdited,
            history.Action
        );

        Assert.Equal(
            "Contenido original",
            history.OldValue
        );

        Assert.Equal(
            "Contenido actualizado",
            history.NewValue
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbidden_WhenDeveloperIsNotAuthor()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: true,
                canModify: false,
                canDelete: false
            );

        var issue = CreateIssue();

        var comment = new IssueComment(
            issue.Id,
            OtherDeveloperId,
            "Comentario ajeno"
        );

        commentRepository.Entities.Add(
            comment
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.UpdateAsync(
                ProjectId,
                issue.Id,
                comment.Id,
                new UpdateIssueCommentRequest(
                    "Intento de edición"
                ),
                DeveloperId
            )
        );

        Assert.Equal(
            "Comentario ajeno",
            comment.Content
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteOwnCommentAndCreateHistory()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: true,
                canModify: true,
                canDelete: true
            );

        var issue = CreateIssue();

        var comment = new IssueComment(
            issue.Id,
            DeveloperId,
            "Comentario eliminado"
        );

        commentRepository.Entities.Add(
            comment
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        await service.DeleteAsync(
            ProjectId,
            issue.Id,
            comment.Id,
            DeveloperId
        );

        Assert.Empty(
            commentRepository.Entities
        );

        Assert.Single(
            historyRepository.Histories
        );

        var history =
            historyRepository.Histories[0];

        Assert.Equal(
            IssueHistoryAction.CommentDeleted,
            history.Action
        );

        Assert.Equal(
            "Comentario eliminado",
            history.OldValue
        );

        Assert.Null(
            history.NewValue
        );
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbidden_WhenDeveloperIsNotAuthor()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: true,
                canModify: false,
                canDelete: false
            );

        var issue = CreateIssue();

        var comment = new IssueComment(
            issue.Id,
            OtherDeveloperId,
            "Comentario ajeno"
        );

        commentRepository.Entities.Add(
            comment
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.DeleteAsync(
                ProjectId,
                issue.Id,
                comment.Id,
                DeveloperId
            )
        );

        Assert.Single(
            commentRepository.Entities
        );

        Assert.Empty(
            historyRepository.Histories
        );
    }

    [Fact]
    public async Task UpdateAsync_ShouldAllowManagerToEditAnyComment()
    {
        var issueRepository =
            new FakeIssueRepository();

        var projectRepository =
            new FakeProjectRepository();

        var commentRepository =
            new FakeIssueCommentRepository();

        var historyRepository =
            new FakeIssueHistoryRepository();

        var issueAuthorizationService =
            new FakeIssueAuthorizationService();

        var commentAuthorizationService =
            new FakeCommentAuthorizationService(
                canCreate: true,
                canModify: true,
                canDelete: true
            );

        var issue = CreateIssue();

        var comment = new IssueComment(
            issue.Id,
            OtherDeveloperId,
            "Comentario original"
        );

        commentRepository.Entities.Add(
            comment
        );

        await issueRepository.AddAsync(
            issue
        );

        var service = CreateService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
        );

        var result =
            await service.UpdateAsync(
                ProjectId,
                issue.Id,
                comment.Id,
                new UpdateIssueCommentRequest(
                    "Editado por Manager"
                ),
                OwnerId
            );

        Assert.Equal(
            "Editado por Manager",
            result.Content
        );

        Assert.Single(
            historyRepository.Histories
        );

        Assert.Equal(
            IssueHistoryAction.CommentEdited,
            historyRepository.Histories[0].Action
        );
    }

    private static Issue CreateIssue()
    {
        return new Issue(
            ProjectId,
            "Test issue",
            OwnerId
        );
    }

    private static IssueCommentService CreateService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IIssueCommentRepository commentRepository,
        IIssueHistoryRepository historyRepository,
        IIssueAuthorizationService issueAuthorizationService,
        ICommentAuthorizationService commentAuthorizationService)
    {
        return new IssueCommentService(
            issueRepository,
            projectRepository,
            commentRepository,
            historyRepository,
            issueAuthorizationService,
            commentAuthorizationService
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
            return Task.FromResult<
                IReadOnlyList<Issue>
            >(
                Issues
                    .Where(
                        issue => issue.ProjectId == projectId
                    )
                    .ToList()
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
                "Test Project",
                "TEST",
                OwnerId
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

    private sealed class FakeIssueCommentRepository
        : IIssueCommentRepository
    {
        public List<IssueComment> Entities { get; } = [];

        public List<IssueCommentReadModel> ReadModels { get; } = [];

        public List<IssueComment> Comments =>
            Entities;

        public Task<IssueComment?> GetByIdAsync(
            Guid commentId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Entities.FirstOrDefault(
                    comment => comment.Id == commentId
                )
            );
        }

        public Task<
            IReadOnlyList<IssueCommentReadModel>
        > GetByIssueIdAsync(
            Guid issueId,
            CancellationToken cancellationToken = default)
        {
            var result =
                ReadModels
                    .Where(
                        comment =>
                            comment.IssueId == issueId
                    )
                    .ToList();

            if (result.Count == 0)
            {
                result = Entities
                    .Where(
                        comment =>
                            comment.IssueId == issueId
                    )
                    .Select(
                        comment =>
                            new IssueCommentReadModel(
                                comment.Id,
                                comment.IssueId,
                                comment.AuthorId,
                                "Developer",
                                "developer@devopsboard.local",
                                comment.Content,
                                comment.CreatedAt,
                                comment.UpdatedAt
                            )
                    )
                    .ToList();
            }

            return Task.FromResult<
                IReadOnlyList<IssueCommentReadModel>
            >(result);
        }

        public Task AddAsync(
            IssueComment comment,
            CancellationToken cancellationToken = default)
        {
            Entities.Add(comment);

            return Task.CompletedTask;
        }

        public void Remove(
            IssueComment comment)
        {
            Entities.Remove(comment);
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
            var result =
                Histories
                    .Where(
                        history =>
                            history.IssueId == issueId
                    )
                    .Select(
                        history =>
                            new IssueHistoryReadModel(
                                history.Id,
                                history.IssueId,
                                history.ActorId,
                                "Developer",
                                "developer@devopsboard.local",
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
        private readonly bool _canView;

        public FakeIssueAuthorizationService(
            bool canView = true)
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
                true
            );
        }

        public Task<bool> CanModifyAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                true
            );
        }

        public Task<bool> CanDeleteAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                true
            );
        }
    }

    private sealed class FakeCommentAuthorizationService
        : ICommentAuthorizationService
    {
        private readonly bool _canCreate;
        private readonly bool _canModify;
        private readonly bool _canDelete;

        public FakeCommentAuthorizationService(
            bool canCreate = true,
            bool canModify = true,
            bool canDelete = true)
        {
            _canCreate = canCreate;
            _canModify = canModify;
            _canDelete = canDelete;
        }

        public Task<bool> CanCreateAsync(
            Guid projectId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canCreate
            );
        }

        public Task<bool> CanModifyAsync(
            Guid projectId,
            string userId,
            string authorId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canModify
            );
        }

        public Task<bool> CanDeleteAsync(
            Guid projectId,
            string userId,
            string authorId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _canDelete
            );
        }
    }
}