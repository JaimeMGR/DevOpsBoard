using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IIssueService
{
    Task<IssueDto> CreateAsync(
        Guid projectId,
        CreateIssueRequest request,
        string reporterId,
        CancellationToken cancellationToken = default
    );

    Task<IssueDto?> GetByIdAsync(
        Guid projectId,
        Guid issueId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<IssueDto>> GetByProjectIdAsync(
        Guid projectId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<IssueDto> UpdateAsync(
        Guid projectId,
        Guid issueId,
        UpdateIssueRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(
        Guid projectId,
        Guid issueId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}