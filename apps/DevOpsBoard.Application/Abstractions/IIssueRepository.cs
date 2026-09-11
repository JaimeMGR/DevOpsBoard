using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueRepository
{
    Task<Issue?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Issue?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Issue>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Issue issue,
        CancellationToken cancellationToken = default
    );

    void Remove(Issue issue);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}