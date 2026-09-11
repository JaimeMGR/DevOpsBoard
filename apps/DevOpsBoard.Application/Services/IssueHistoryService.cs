using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;

namespace DevOpsBoard.Application.Services;

public class IssueHistoryService
    : IIssueHistoryService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IIssueHistoryRepository _historyRepository;
    private readonly IIssueAuthorizationService
        _issueAuthorizationService;

    public IssueHistoryService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        IIssueHistoryRepository historyRepository,
        IIssueAuthorizationService issueAuthorizationService)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _historyRepository = historyRepository;
        _issueAuthorizationService =
            issueAuthorizationService;
    }

    public async Task<IReadOnlyList<IssueHistoryDto>>
        GetByIssueIdAsync(
            Guid projectId,
            Guid issueId,
            string actingUserId,
            CancellationToken cancellationToken = default)
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

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
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

        var canView =
            await _issueAuthorizationService.CanViewAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canView)
        {
            throw new ForbiddenException(
                "No tienes permisos para consultar el historial de esta incidencia."
            );
        }

        var issue = await _issueRepository.GetByIdAsync(
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

        var history =
            await _historyRepository.GetByIssueIdAsync(
                issueId,
                cancellationToken
            );

        return history
            .Select(entry =>
                new IssueHistoryDto(
                    entry.Id,
                    entry.ActorId,
                    entry.ActorDisplayName,
                    entry.ActorEmail,
                    entry.Action,
                    entry.OldValue,
                    entry.NewValue,
                    entry.CorrelationId,
                    entry.CreatedAt
                )
            )
            .ToList();
    }
}