using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueCommentService
{
    Task<IssueCommentDto> CreateAsync(
        Guid projectId,
        Guid issueId,
        CreateIssueCommentRequest request,
        string authorId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<IssueCommentDto>> GetByIssueIdAsync(
        Guid projectId,
        Guid issueId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<IssueCommentDto> UpdateAsync(
        Guid projectId,
        Guid issueId,
        Guid commentId,
        UpdateIssueCommentRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid projectId,
        Guid issueId,
        Guid commentId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}