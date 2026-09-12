using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueLabelService
{
    Task<IReadOnlyList<LabelDto>> GetByIssueIdAsync(
        Guid projectId,
        Guid issueId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<LabelDto> AddAsync(
        Guid projectId,
        Guid issueId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        Guid projectId,
        Guid issueId,
        Guid labelId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}