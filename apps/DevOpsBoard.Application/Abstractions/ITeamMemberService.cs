using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface ITeamMemberService
{
    Task<TeamMemberDto> AddAsync(
        Guid teamId,
        AddTeamMemberRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TeamMemberDto>> GetByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default
    );

    Task<TeamMemberDto> UpdateRoleAsync(
        Guid teamId,
        string userId,
        UpdateTeamMemberRoleRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        Guid teamId,
        string userId,
        string actingUserId,
        CancellationToken cancellationToken = default
    );
}