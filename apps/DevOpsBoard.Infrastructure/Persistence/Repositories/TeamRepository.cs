using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public TeamRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Team?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(
                team => team.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Team>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .AsNoTracking()
            .OrderBy(team => team.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Teams
            .AnyAsync(
                team => team.Name == name,
                cancellationToken
            );
    }

    public async Task AddAsync(
        Team team,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Teams.AddAsync(
            team,
            cancellationToken
        );
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}