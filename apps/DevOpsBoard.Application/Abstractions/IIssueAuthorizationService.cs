namespace DevOpsBoard.Application.Abstractions;

public interface IIssueAuthorizationService
{
    Task<bool> CanViewAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanCreateAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanModifyAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanDeleteAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );
}