using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAuthorizationService
        _projectAuthorizationService;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectAuthorizationService projectAuthorizationService)
    {
        _projectRepository = projectRepository;
        _projectAuthorizationService =
            projectAuthorizationService;
    }

    public async Task<ProjectDto> CreateAsync(
        CreateProjectRequest request,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
                "El nombre del proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Key))
        {
            throw new ValidationException(
                "La clave del proyecto es obligatoria."
            );
        }

        var normalizedKey =
            request.Key.Trim().ToUpperInvariant();

        if (normalizedKey.Length > 10)
        {
            throw new ValidationException(
                "La clave del proyecto no puede superar los 10 caracteres."
            );
        }

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ValidationException(
                "El propietario del proyecto es obligatorio."
            );
        }

        var exists = await _projectRepository.ExistsByKeyAsync(
            normalizedKey,
            cancellationToken
        );

        if (exists)
        {
            throw new ConflictException(
                $"Ya existe un proyecto con la clave '{normalizedKey}'."
            );
        }

        var project = new Project(
            request.Name,
            normalizedKey,
            ownerId,
            request.Description
        );

        await _projectRepository.AddAsync(
            project,
            cancellationToken
        );

        await _projectRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(project);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(
            cancellationToken
        );

        return projects
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        return project is null
            ? null
            : MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(
        Guid id,
        UpdateProjectRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
                "El nombre del proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        var project =
            await _projectRepository.GetByIdForUpdateAsync(
                id,
                cancellationToken
            );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var canManage =
            await _projectAuthorizationService.CanManageAsync(
                id,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para modificar este proyecto."
            );
        }

        project.Update(
            request.Name,
            request.Description
        );

        await _projectRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(project);
    }

    public async Task DeleteAsync(
        Guid id,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
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

        var project =
            await _projectRepository.GetByIdForUpdateAsync(
                id,
                cancellationToken
            );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var canManage =
            await _projectAuthorizationService.CanManageAsync(
                id,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para eliminar este proyecto."
            );
        }

        _projectRepository.Remove(project);

        await _projectRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private static ProjectDto MapToDto(
        Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Key,
            project.Description,
            project.CreatedAt,
            project.OwnerId
        );
    }
}