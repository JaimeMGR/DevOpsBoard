namespace DevOpsBoard.Application.Abstractions;

public interface IProjectMemberAuthorizationService
{
    Task<bool> CanManageMembersAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );
}