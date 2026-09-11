using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Application.Services;

public class IssueCommentService
    : IIssueCommentService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IIssueCommentRepository _commentRepository;
    private readonly IIssueHistoryRepository _historyRepository;
    private readonly IIssueAuthorizationService _issueAuthorizationService;
    private readonly ICommentAuthorizationService _commentAuthorizationService;

    public IssueCommentService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IIssueCommentRepository commentRepository,
        IIssueHistoryRepository historyRepository,
        IIssueAuthorizationService issueAuthorizationService,
        ICommentAuthorizationService commentAuthorizationService)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _commentRepository = commentRepository;
        _historyRepository = historyRepository;
        _issueAuthorizationService =
            issueAuthorizationService;
        _commentAuthorizationService =
            commentAuthorizationService;
    }

    public async Task<IssueCommentDto> CreateAsync(
        Guid projectId,
        Guid issueId,
        CreateIssueCommentRequest request,
        string authorId,
        CancellationToken cancellationToken = default)
    {
        var issue =
            await GetIssueAsync(
                projectId,
                issueId,
                cancellationToken
            );

        var canCreate =
            await _commentAuthorizationService.CanCreateAsync(
                projectId,
                authorId,
                cancellationToken
            );

        if (!canCreate)
        {
            throw new ForbiddenException(
                "No tienes permisos para crear comentarios."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ValidationException(
                "El contenido del comentario es obligatorio."
            );
        }

        var comment = new IssueComment(
            issue.Id,
            authorId,
            request.Content
        );

        var correlationId = Guid.NewGuid();

        var history = new IssueHistory(
            issue.Id,
            authorId,
            correlationId,
            IssueHistoryAction.CommentAdded,
            null,
            comment.Content
        );

        await _commentRepository.AddAsync(
            comment,
            cancellationToken
        );

        await _historyRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );

        return await MapToDtoAsync(
            comment,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<IssueCommentDto>>
        GetByIssueIdAsync(
            Guid projectId,
            Guid issueId,
            string actingUserId,
            CancellationToken cancellationToken = default)
    {
        await GetIssueAsync(
            projectId,
            issueId,
            cancellationToken
        );

        var canView =
            await _issueAuthorizationService.CanViewAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canView)
        {
            throw new ForbiddenException(
                "No tienes permisos para consultar los comentarios."
            );
        }

        var comments =
            await _commentRepository.GetByIssueIdAsync(
                issueId,
                cancellationToken
            );

        return comments
            .Select(
                comment =>
                    new IssueCommentDto(
                        comment.Id,
                        comment.AuthorId,
                        comment.AuthorDisplayName,
                        comment.AuthorEmail,
                        comment.Content,
                        comment.CreatedAt,
                        comment.UpdatedAt
                    )
            )
            .ToList();
    }

    public async Task<IssueCommentDto> UpdateAsync(
        Guid projectId,
        Guid issueId,
        Guid commentId,
        UpdateIssueCommentRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        var issue =
            await GetIssueAsync(
                projectId,
                issueId,
                cancellationToken
            );

        var comment =
            await _commentRepository.GetByIdAsync(
                commentId,
                cancellationToken
            );

        if (comment is null ||
            comment.IssueId != issue.Id)
        {
            throw new NotFoundException(
                "El comentario no existe."
            );
        }

        var canModify =
            await _commentAuthorizationService.CanModifyAsync(
                projectId,
                actingUserId,
                comment.AuthorId,
                cancellationToken
            );

        if (!canModify)
        {
            throw new ForbiddenException(
                "No tienes permisos para modificar este comentario."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ValidationException(
                "El contenido del comentario es obligatorio."
            );
        }

        var oldContent = comment.Content;

        comment.UpdateContent(
            request.Content
        );

        var correlationId = Guid.NewGuid();

        var history = new IssueHistory(
            issue.Id,
            actingUserId,
            correlationId,
            IssueHistoryAction.CommentEdited,
            oldContent,
            comment.Content
        );

        await _historyRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );

        return await MapToDtoAsync(
            comment,
            cancellationToken
        );
    }

    public async Task DeleteAsync(
        Guid projectId,
        Guid issueId,
        Guid commentId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        var issue =
            await GetIssueAsync(
                projectId,
                issueId,
                cancellationToken
            );

        var comment =
            await _commentRepository.GetByIdAsync(
                commentId,
                cancellationToken
            );

        if (comment is null ||
            comment.IssueId != issue.Id)
        {
            throw new NotFoundException(
                "El comentario no existe."
            );
        }

        var canDelete =
            await _commentAuthorizationService.CanDeleteAsync(
                projectId,
                actingUserId,
                comment.AuthorId,
                cancellationToken
            );

        if (!canDelete)
        {
            throw new ForbiddenException(
                "No tienes permisos para eliminar este comentario."
            );
        }

        var correlationId = Guid.NewGuid();

        var history = new IssueHistory(
            issue.Id,
            actingUserId,
            correlationId,
            IssueHistoryAction.CommentDeleted,
            comment.Content,
            null
        );

        _commentRepository.Remove(
            comment
        );

        await _historyRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private async Task<Issue> GetIssueAsync(
        Guid projectId,
        Guid issueId,
        CancellationToken cancellationToken)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
            );
        }

        if (issueId == Guid.Empty)
        {
            throw new ValidationException(
                "La incidencia es obligatoria."
            );
        }

        var project = await _projectRepository.GetByIdAsync(
            projectId,
            cancellationToken
        );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var issue =
            await _issueRepository.GetByIdAsync(
                issueId,
                cancellationToken
            );

        if (issue is null ||
            issue.ProjectId != projectId)
        {
            throw new NotFoundException(
                "La incidencia no existe."
            );
        }

        return issue;
    }

    private async Task<IssueCommentDto> MapToDtoAsync(
        IssueComment comment,
        CancellationToken cancellationToken)
    {
        var comments =
            await _commentRepository.GetByIssueIdAsync(
                comment.IssueId,
                cancellationToken
            );

        var result = comments.FirstOrDefault(
            item => item.Id == comment.Id
        );

        if (result is null)
        {
            throw new InvalidOperationException(
                "No se pudo recuperar el comentario creado o actualizado."
            );
        }

        return new IssueCommentDto(
            result.Id,
            result.AuthorId,
            result.AuthorDisplayName,
            result.AuthorEmail,
            result.Content,
            result.CreatedAt,
            result.UpdatedAt
        );
    }
}