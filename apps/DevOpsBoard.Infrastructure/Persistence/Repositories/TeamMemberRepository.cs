using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class TeamMemberRepository : ITeamMemberRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public TeamMemberRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TeamMember?> GetAsync(
        Guid teamId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TeamMembers
            .FirstOrDefaultAsync(
                member =>
                    member.TeamId == teamId &&
                    member.UserId == userId,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<TeamMemberReadModel>> GetByTeamIdAsync(
    Guid teamId,
    CancellationToken cancellationToken = default)
    {
        return await (
            from member in _dbContext.TeamMembers
            join user in _dbContext.Users
                on member.UserId equals user.Id
            where member.TeamId == teamId
            orderby member.JoinedAt
            select new TeamMemberReadModel(
                member.UserId,
                user.DisplayName,
                user.Email ?? string.Empty,
                member.Role.ToString(),
                member.JoinedAt
            )
        )
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TeamMember member,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TeamMembers.AddAsync(
            member,
            cancellationToken
        );
    }

    public void Remove(TeamMember member)
    {
        _dbContext.TeamMembers.Remove(member);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}