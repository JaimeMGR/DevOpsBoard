using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueHistoryRepository
{
    Task AddAsync(
        IssueHistory history,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<IssueHistoryReadModel>> GetByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken = default
    );
}