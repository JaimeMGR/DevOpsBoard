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

    public IssueService(
    IIssueRepository issueRepository,
    IProjectRepository projectRepository,
    IUserRepository userRepository,
    IIssueAuthorizationService issueAuthorizationService)
    {
        _issueRepository = issueRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _issueAuthorizationService =
            issueAuthorizationService;
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

        var reporter = await _userRepository.GetSummaryByIdAsync(
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

        await _issueRepository.AddAsync(
            issue,
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
        CancellationToken cancellationToken = default)
    {
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

        var issue = await _issueRepository.GetByIdAsync(
            issueId,
            cancellationToken
        );

        if (issue is null)
        {
            return null;
        }

        if (issue.ProjectId != projectId)
        {
            return null;
        }

        return MapToDto(issue);
    }

    public async Task<IReadOnlyList<IssueDto>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
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

        var issues =
            await _issueRepository.GetByProjectIdAsync(
                projectId,
                cancellationToken
            );

        return issues
            .Select(MapToDto)
            .ToList();
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

    private readonly IIssueAuthorizationService
    _issueAuthorizationService;

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

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException(
                "El título de la incidencia es obligatorio."
            );
        }

        if (!string.IsNullOrWhiteSpace(request.AssigneeId))
        {
            var assignee =
                await _userRepository.GetSummaryByIdAsync(
                    request.AssigneeId,
                    cancellationToken
                );

            if (assignee is null)
            {
                throw new NotFoundException(
                    "El usuario asignado no existe."
                );
            }
        }

        issue.Update(
            request.Title,
            request.Description
        );

        issue.ChangeStatus(status);
        issue.ChangePriority(priority);

        if (string.IsNullOrWhiteSpace(
                request.AssigneeId))
        {
            issue.Unassign();
        }
        else
        {
            issue.AssignTo(
                request.AssigneeId
            );
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

        _issueRepository.Remove(issue);

        await _issueRepository.SaveChangesAsync(
            cancellationToken
        );
    }
}