using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public ProjectRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project => project.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(project => project.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AnyAsync(
                project => project.Key == key,
                cancellationToken
            );
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(
            project,
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

    public async Task<Project?> GetByIdForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .FirstOrDefaultAsync(
                project => project.Id == id,
                cancellationToken
            );
    }

    public void Remove(Project project)
    {
        _dbContext.Projects.Remove(project);
    }
}