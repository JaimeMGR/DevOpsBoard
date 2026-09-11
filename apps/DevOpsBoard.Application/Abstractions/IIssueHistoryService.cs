using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueHistoryService
{
    Task<IReadOnlyList<IssueHistoryDto>> GetByIssueIdAsync(
        Guid projectId,
        Guid issueId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}