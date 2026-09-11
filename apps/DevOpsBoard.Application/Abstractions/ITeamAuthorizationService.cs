namespace DevOpsBoard.Application.Abstractions;

public interface ITeamAuthorizationService
{
    Task<bool> CanManageMembersAsync(
        Guid teamId,
        string userId,
        CancellationToken cancellationToken = default
    );
}