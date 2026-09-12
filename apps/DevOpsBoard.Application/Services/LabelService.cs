using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Services;

public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAuthorizationService
        _projectAuthorizationService;

    public LabelService(
        ILabelRepository labelRepository,
        IProjectRepository projectRepository,
        IProjectAuthorizationService projectAuthorizationService)
    {
        _labelRepository = labelRepository;
        _projectRepository = projectRepository;
        _projectAuthorizationService =
            projectAuthorizationService;
    }

    public async Task<IReadOnlyList<LabelDto>> GetByProjectIdAsync(
        Guid projectId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndUser(
            projectId,
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

        var canView =
            await _projectAuthorizationService.CanViewAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canView)
        {
            throw new ForbiddenException(
                "No tienes permisos para consultar las etiquetas de este proyecto."
            );
        }

        var labels =
            await _labelRepository.GetByProjectIdAsync(
                projectId,
                cancellationToken
            );

        return labels
            .Select(MapToDto)
            .ToList();
    }

    public async Task<LabelDto?> GetByIdAsync(
        Guid projectId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndUser(
            projectId,
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

        var canView =
            await _projectAuthorizationService.CanViewAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canView)
        {
            throw new ForbiddenException(
                "No tienes permisos para consultar las etiquetas de este proyecto."
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
            return null;
        }

        return MapToDto(label);
    }

    public async Task<LabelDto> CreateAsync(
        Guid projectId,
        CreateLabelRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndUser(
            projectId,
            actingUserId
        );

        ValidateLabelRequest(
            request.Name,
            request.Color
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

        var canManage =
            await _projectAuthorizationService
                .CanManageSettingsAsync(
                    projectId,
                    actingUserId,
                    cancellationToken
                );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar las etiquetas de este proyecto."
            );
        }

        var normalizedName =
            request.Name.Trim();

        var exists =
            await _labelRepository.ExistsByNameAsync(
                projectId,
                normalizedName,
                cancellationToken: cancellationToken
            );

        if (exists)
        {
            throw new ConflictException(
                $"Ya existe una etiqueta con el nombre '{normalizedName}'."
            );
        }

        var label = new Label(
            projectId,
            normalizedName,
            request.Color
        );

        await _labelRepository.AddAsync(
            label,
            cancellationToken
        );

        await _labelRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(label);
    }

    public async Task<LabelDto> UpdateAsync(
        Guid projectId,
        Guid labelId,
        UpdateLabelRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndUser(
            projectId,
            actingUserId
        );

        if (labelId == Guid.Empty)
        {
            throw new ValidationException(
                "La etiqueta es obligatoria."
            );
        }

        ValidateLabelRequest(
            request.Name,
            request.Color
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

        var canManage =
            await _projectAuthorizationService
                .CanManageSettingsAsync(
                    projectId,
                    actingUserId,
                    cancellationToken
                );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar las etiquetas de este proyecto."
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

        var normalizedName =
            request.Name.Trim();

        var exists =
            await _labelRepository.ExistsByNameAsync(
                projectId,
                normalizedName,
                labelId,
                cancellationToken
            );

        if (exists)
        {
            throw new ConflictException(
                $"Ya existe una etiqueta con el nombre '{normalizedName}'."
            );
        }

        label.Rename(
            normalizedName
        );

        label.ChangeColor(
            request.Color
        );

        await _labelRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(label);
    }

    public async Task DeleteAsync(
        Guid projectId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateProjectAndUser(
            projectId,
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

        var canManage =
            await _projectAuthorizationService
                .CanManageSettingsAsync(
                    projectId,
                    actingUserId,
                    cancellationToken
                );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar las etiquetas de este proyecto."
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

        _labelRepository.Remove(
            label
        );

        await _labelRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private static void ValidateProjectAndUser(
        Guid projectId,
        string actingUserId)
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
    }

    private static void ValidateLabelRequest(
        string name,
        string color)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "El nombre de la etiqueta es obligatorio."
            );
        }

        if (name.Trim().Length > 50)
        {
            throw new ValidationException(
                "El nombre de la etiqueta no puede superar los 50 caracteres."
            );
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ValidationException(
                "El color de la etiqueta es obligatorio."
            );
        }

        if (color.Trim().Length > 20)
        {
            throw new ValidationException(
                "El color de la etiqueta no puede superar los 20 caracteres."
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