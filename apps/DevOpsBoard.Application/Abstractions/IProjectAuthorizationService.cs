namespace DevOpsBoard.Application.Abstractions;

public interface IProjectAuthorizationService
{
    Task<bool> CanManageAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );
}