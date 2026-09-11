namespace DevOpsBoard.Application.Abstractions;

public interface ICommentAuthorizationService
{
    Task<bool> CanCreateAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanModifyAsync(
        Guid projectId,
        string userId,
        string authorId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanDeleteAsync(
        Guid projectId,
        string userId,
        string authorId,
        CancellationToken cancellationToken = default
    );
}