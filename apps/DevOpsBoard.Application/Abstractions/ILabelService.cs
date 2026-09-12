using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface ILabelService
{
    Task<IReadOnlyList<LabelDto>> GetByProjectIdAsync(
        Guid projectId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<LabelDto?> GetByIdAsync(
        Guid projectId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<LabelDto> CreateAsync(
        Guid projectId,
        CreateLabelRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<LabelDto> UpdateAsync(
        Guid projectId,
        Guid labelId,
        UpdateLabelRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid projectId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}