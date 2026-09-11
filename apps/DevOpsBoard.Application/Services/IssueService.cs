using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Application.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IIssueAuthorizationService
        _issueAuthorizationService;
    private readonly IIssueHistoryRepository
        _issueHistoryRepository;

    public IssueService(
    IIssueRepository issueRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IIssueAuthorizationService issueAuthorizationService,
    IIssueHistoryRepository issueHistoryRepository)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _issueAuthorizationService =
            issueAuthorizationService;
        _issueHistoryRepository =
            issueHistoryRepository;
    }

    private static void ValidateQuery(
        IssueQueryParameters query)
    {
        if (query.Page < 1)
        {
            throw new ValidationException(
                "La página debe ser mayor o igual que 1."
            );
        }

        if (query.PageSize < 1 ||
            query.PageSize > 100)
        {
            throw new ValidationException(
                "El tamaño de página debe estar entre 1 y 100."
            );
        }

        if (!string.IsNullOrWhiteSpace(
                query.Status) &&
            !Enum.TryParse<IssueStatus>(
                query.Status,
                true,
                out _))
        {
            throw new ValidationException(
                "El estado indicado no es válido."
            );
        }

        if (!string.IsNullOrWhiteSpace(
                query.Priority) &&
            !Enum.TryParse<IssuePriority>(
                query.Priority,
                true,
                out _))
        {
            throw new ValidationException(
                "La prioridad indicada no es válida."
            );
        }

        var validSortFields =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
            "CreatedAt",
            "UpdatedAt",
            "Title",
            "Status",
            "Priority"
            };

        if (!validSortFields.Contains(
                query.SortBy))
        {
            throw new ValidationException(
                "El campo de ordenación no es válido."
            );
        }

        if (!string.Equals(
                query.SortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase)
            &&
            !string.Equals(
                query.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                "La dirección de ordenación debe ser 'asc' o 'desc'."
            );
        }
    }
    public async Task<IssueDto> CreateAsync(
        Guid projectId,
        CreateIssueRequest request,
        string reporterId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(reporterId))
        {
            throw new ValidationException(
                "El creador de la incidencia es obligatorio."
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

        var canCreate =
            await _issueAuthorizationService.CanCreateAsync(
                projectId,
                reporterId,
                cancellationToken
            );

        if (!canCreate)
        {
            throw new ForbiddenException(
                "No tienes permisos para crear incidencias en este proyecto."
            );
        }

        var reporter =
            await _userRepository.GetSummaryByIdAsync(
                reporterId,
                cancellationToken
            );

        if (reporter is null)
        {
            throw new NotFoundException(
                "El usuario no existe."
            );
        }

        if (!Enum.TryParse<IssuePriority>(
                request.Priority,
                true,
                out var priority))
        {
            throw new ValidationException(
                "La prioridad debe ser 'Low', 'Medium', 'High' o 'Critical'."
            );
        }

        var issue = new Issue(
            projectId,
            request.Title,
            reporterId,
            request.Description,
            priority
        );

        var correlationId = Guid.NewGuid();

        var history = new IssueHistory(
            issue.Id,
            reporterId,
            correlationId,
            IssueHistoryAction.Created
        );

        await _issueRepository.AddAsync(
            issue,
            cancellationToken
        );

        await _issueHistoryRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(issue);
    }

    public async Task<IssueDto?> GetByIdAsync(
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
                "No tienes permisos para consultar este proyecto."
            );
        }

        var issue = await _issueRepository.GetByIdAsync(
            issueId,
            cancellationToken
        );

        if (issue is null ||
            issue.ProjectId != projectId)
        {
            return null;
        }

        return MapToDto(issue);
    }

    public async Task<IReadOnlyList<IssueDto>>
        GetByProjectIdAsync(
            Guid projectId,
            string actingUserId,
            CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
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
                "No tienes permisos para consultar este proyecto."
            );
        }

        var issues =
            await _issueRepository.GetByProjectIdAsync(
                projectId,
                cancellationToken
            );

        return issues
            .Select(MapToDto)
            .ToList();
    }

    public async Task<PagedResult<IssueDto>>
    GetPagedByProjectIdAsync(
        Guid projectId,
        IssueQueryParameters query,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        ValidateQuery(query);

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

        var canView =
            await _issueAuthorizationService.CanViewAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canView)
        {
            throw new ForbiddenException(
                "No tienes permisos para consultar este proyecto."
            );
        }

        var result =
            await _issueRepository.GetPagedByProjectIdAsync(
                projectId,
                query,
                cancellationToken
            );

        return new PagedResult<IssueDto>(
            result.Items
                .Select(MapToDto)
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount
        );
    }

    public async Task<IssueDto> UpdateAsync(
        Guid projectId,
        Guid issueId,
        UpdateIssueRequest request,
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

        var issue =
            await _issueRepository.GetByIdForUpdateAsync(
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
                "No tienes permisos para modificar esta incidencia."
            );
        }

        var normalizedTitle =
            request.Title.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ValidationException(
                "El título de la incidencia es obligatorio."
            );
        }

        var normalizedDescription =
            request.Description?.Trim();

        if (!Enum.TryParse<IssueStatus>(
                request.Status,
                true,
                out var status))
        {
            throw new ValidationException(
                "El estado debe ser 'Todo', 'InProgress', 'InReview' o 'Done'."
            );
        }

        if (!Enum.TryParse<IssuePriority>(
                request.Priority,
                true,
                out var priority))
        {
            throw new ValidationException(
                "La prioridad debe ser 'Low', 'Medium', 'High' o 'Critical'."
            );
        }

        var normalizedAssigneeId =
            string.IsNullOrWhiteSpace(
                request.AssigneeId)
                ? null
                : request.AssigneeId.Trim();

        if (normalizedAssigneeId is not null)
        {
            var assignee =
                await _userRepository.GetSummaryByIdAsync(
                    normalizedAssigneeId,
                    cancellationToken
                );

            if (assignee is null)
            {
                throw new NotFoundException(
                    "El usuario asignado no existe."
                );
            }
        }

        var correlationId = Guid.NewGuid();

        if (issue.Title != normalizedTitle)
        {
            var oldValue = issue.Title;

            issue.Update(
                normalizedTitle,
                issue.Description
            );

            await AddHistoryAsync(
                issue.Id,
                actingUserId,
                correlationId,
                IssueHistoryAction.TitleChanged,
                oldValue,
                normalizedTitle,
                cancellationToken
            );
        }

        if (issue.Description != normalizedDescription)
        {
            var oldValue = issue.Description;

            issue.Update(
                issue.Title,
                normalizedDescription
            );

            await AddHistoryAsync(
                issue.Id,
                actingUserId,
                correlationId,
                IssueHistoryAction.DescriptionChanged,
                oldValue,
                normalizedDescription,
                cancellationToken
            );
        }

        if (issue.Status != status)
        {
            var oldValue =
                issue.Status.ToString();

            issue.ChangeStatus(status);

            await AddHistoryAsync(
                issue.Id,
                actingUserId,
                correlationId,
                IssueHistoryAction.StatusChanged,
                oldValue,
                status.ToString(),
                cancellationToken
            );
        }

        if (issue.Priority != priority)
        {
            var oldValue =
                issue.Priority.ToString();

            issue.ChangePriority(priority);

            await AddHistoryAsync(
                issue.Id,
                actingUserId,
                correlationId,
                IssueHistoryAction.PriorityChanged,
                oldValue,
                priority.ToString(),
                cancellationToken
            );
        }

        if (issue.AssigneeId != normalizedAssigneeId)
        {
            var oldValue =
                issue.AssigneeId;

            if (normalizedAssigneeId is null)
            {
                issue.Unassign();

                await AddHistoryAsync(
                    issue.Id,
                    actingUserId,
                    correlationId,
                    IssueHistoryAction.Unassigned,
                    oldValue,
                    null,
                    cancellationToken
                );
            }
            else
            {
                issue.AssignTo(
                    normalizedAssigneeId
                );

                await AddHistoryAsync(
                    issue.Id,
                    actingUserId,
                    correlationId,
                    IssueHistoryAction.Assigned,
                    oldValue,
                    normalizedAssigneeId,
                    cancellationToken
                );
            }
        }

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(issue);
    }

    public async Task DeleteAsync(
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

        var issue =
            await _issueRepository.GetByIdForUpdateAsync(
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

        var canDelete =
            await _issueAuthorizationService.CanDeleteAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canDelete)
        {
            throw new ForbiddenException(
                "No tienes permisos para eliminar esta incidencia."
            );
        }

        var correlationId = Guid.NewGuid();

        issue.Delete();

        var history = new IssueHistory(
            issue.Id,
            actingUserId,
            correlationId,
            IssueHistoryAction.Deleted
        );

        await _issueHistoryRepository.AddAsync(
            history,
            cancellationToken
        );

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private async Task AddHistoryAsync(
        Guid issueId,
        string actorId,
        Guid correlationId,
        IssueHistoryAction action,
        string? oldValue,
        string? newValue,
        CancellationToken cancellationToken)
    {
        var history = new IssueHistory(
            issueId,
            actorId,
            correlationId,
            action,
            oldValue,
            newValue
        );

        await _issueHistoryRepository.AddAsync(
            history,
            cancellationToken
        );
    }

    private static IssueDto MapToDto(
        Issue issue)
    {
        return new IssueDto(
            issue.Id,
            issue.ProjectId,
            issue.Title,
            issue.Description,
            issue.Status.ToString(),
            issue.Priority.ToString(),
            issue.ReporterId,
            issue.AssigneeId,
            issue.CreatedAt,
            issue.UpdatedAt
        );
    }
}