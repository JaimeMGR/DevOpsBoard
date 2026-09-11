using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface ITeamMemberRepository
{
    Task<TeamMember?> GetAsync(
        Guid teamId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TeamMemberReadModel>> GetByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        TeamMember member,
        CancellationToken cancellationToken = default
    );

    void Remove(TeamMember member);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}