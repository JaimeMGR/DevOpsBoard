using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IProjectService
{
    Task<ProjectDto> CreateAsync(
        CreateProjectRequest request,
        string ownerId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ProjectDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<ProjectDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<ProjectDto> UpdateAsync(
        Guid id,
        UpdateProjectRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid id,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}