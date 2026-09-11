using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class ProjectMemberRepository
    : IProjectMemberRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public ProjectMemberRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectMember?> GetAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId,
                cancellationToken
            );
    }

    public async Task<
        IReadOnlyList<ProjectMemberReadModel>
    > GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from member in _dbContext.ProjectMembers
            join user in _dbContext.Users
                on member.UserId equals user.Id
            where member.ProjectId == projectId
            orderby member.JoinedAt
            select new ProjectMemberReadModel(
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
        ProjectMember member,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ProjectMembers.AddAsync(
            member,
            cancellationToken
        );
    }

    public void Remove(ProjectMember member)
    {
        _dbContext.ProjectMembers.Remove(member);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}