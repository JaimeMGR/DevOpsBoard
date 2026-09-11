using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueCommentRepository
{
    Task<IssueComment?> GetByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<IssueCommentReadModel>> GetByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        IssueComment comment,
        CancellationToken cancellationToken = default
    );

    void Remove(
        IssueComment comment
    );
}