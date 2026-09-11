using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IProjectMemberService
{
    Task<ProjectMemberDto> AddAsync(
        Guid projectId,
        AddProjectMemberRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ProjectMemberDto>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default
    );

    Task<ProjectMemberDto> UpdateRoleAsync(
        Guid projectId,
        string userId,
        UpdateProjectMemberRoleRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        Guid projectId,
        string userId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}