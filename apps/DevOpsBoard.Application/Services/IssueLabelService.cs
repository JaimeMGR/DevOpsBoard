using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Application.Services;

public class IssueLabelService : IIssueLabelService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILabelRepository _labelRepository;
    private readonly IIssueLabelRepository _issueLabelRepository;
    private readonly IIssueAuthorizationService
        _issueAuthorizationService;
    private readonly IIssueHistoryRepository
        _issueHistoryRepository;

    public IssueLabelService(
        IIssueRepository issueRepository,
        IProjectRepository projectRepository,
        ILabelRepository labelRepository,
        IIssueLabelRepository issueLabelRepository,
        IIssueAuthorizationService issueAuthorizationService,
        IIssueHistoryRepository issueHistoryRepository)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _labelRepository = labelRepository;
        _issueLabelRepository = issueLabelRepository;
        _issueAuthorizationService =
            issueAuthorizationService;
        _issueHistoryRepository =
            issueHistoryRepository;
    }

    public async Task<IReadOnlyList<LabelDto>>
        GetByIssueIdAsync(
            Guid projectId,
            Guid issueId,
            string actingUserId,
            CancellationToken cancellationToken = default)
    {
        ValidateIds(
            projectId,
            issueId,
            actingUserId
        );

        var project =
            await _projectRepository.GetByIdAsync(
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

        var canView =
            await _issueAuthorizationService.CanViewAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canView)
        {
            throw new ForbiddenException(
                "No tienes permisos para consultar las etiquetas de esta incidencia."
            );
        }

        var labels =
            await _issueLabelRepository.GetLabelsByIssueIdAsync(
                issueId,
                cancellationToken
            );

        return labels
            .Select(MapToDto)
            .ToList();
    }

    public async Task<LabelDto> AddAsync(
        Guid projectId,
        Guid issueId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateIds(
            projectId,
            issueId,
            actingUserId
        );

        if (labelId == Guid.Empty)
        {
            throw new ValidationException(
                "La etiqueta es obligatoria."
            );
        }

        var project =
            await _projectRepository.GetByIdAsync(
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

        var canModify =
            await _issueAuthorizationService.CanModifyAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canModify)
        {
            throw new ForbiddenException(
                "No tienes permisos para modificar las etiquetas de esta incidencia."
            );
        }

        var label =
            await _labelRepository.GetByIdAsync(
                labelId,
                cancellationToken
            );

        if (label is null ||
            label.ProjectId != projectId)
        {
            throw new NotFoundException(
                "La etiqueta no existe."
            );
        }

        var alreadyAssigned =
            await _issueLabelRepository.ExistsAsync(
                issueId,
                labelId,
                cancellationToken
            );

        if (alreadyAssigned)
        {
            throw new ConflictException(
                "La etiqueta ya está asignada a esta incidencia."
            );
        }

        var issueLabel =
            new IssueLabel(
                issueId,
                labelId
            );

        await _issueLabelRepository.AddAsync(
            issueLabel,
            cancellationToken
        );

        var correlationId =
            Guid.NewGuid();

        var history =
            new IssueHistory(
                issueId,
                actingUserId,
                correlationId,
                IssueHistoryAction.LabelAdded,
                null,
                label.Name
            );

        await _issueHistoryRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueLabelRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(label);
    }

    public async Task RemoveAsync(
        Guid projectId,
        Guid issueId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateIds(
            projectId,
            issueId,
            actingUserId
        );

        if (labelId == Guid.Empty)
        {
            throw new ValidationException(
                "La etiqueta es obligatoria."
            );
        }

        var project =
            await _projectRepository.GetByIdAsync(
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

        var canModify =
            await _issueAuthorizationService.CanModifyAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canModify)
        {
            throw new ForbiddenException(
                "No tienes permisos para modificar las etiquetas de esta incidencia."
            );
        }

        var label =
            await _labelRepository.GetByIdAsync(
                labelId,
                cancellationToken
            );

        if (label is null ||
            label.ProjectId != projectId)
        {
            throw new NotFoundException(
                "La etiqueta no existe."
            );
        }

        var issueLabel =
            await _issueLabelRepository.GetAsync(
                issueId,
                labelId,
                cancellationToken
            );

        if (issueLabel is null)
        {
            throw new NotFoundException(
                "La etiqueta no está asignada a esta incidencia."
            );
        }

        _issueLabelRepository.Remove(
            issueLabel
        );

        var correlationId =
            Guid.NewGuid();

        var history =
            new IssueHistory(
                issueId,
                actingUserId,
                correlationId,
                IssueHistoryAction.LabelRemoved,
                label.Name,
                null
            );

        await _issueHistoryRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueLabelRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private static void ValidateIds(
        Guid projectId,
        Guid issueId,
        string actingUserId)
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
    }

    private static LabelDto MapToDto(
        Label label)
    {
        return new LabelDto(
            label.Id,
            label.ProjectId,
            label.Name,
            label.Color
        );
    }
}