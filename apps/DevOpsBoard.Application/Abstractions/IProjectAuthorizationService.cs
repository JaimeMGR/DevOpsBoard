namespace DevOpsBoard.Application.Abstractions;

public interface IProjectAuthorizationService
{
    Task<bool> CanViewAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanManageSettingsAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> CanManageAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );
}