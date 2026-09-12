using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueLabelRepository
{
    Task<IReadOnlyList<Label>> GetLabelsByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsAsync(
        Guid issueId,
        Guid labelId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        IssueLabel issueLabel,
        CancellationToken cancellationToken = default
    );

    Task<IssueLabel?> GetAsync(
        Guid issueId,
        Guid labelId,
        CancellationToken cancellationToken = default
    );

    void Remove(IssueLabel issueLabel);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}