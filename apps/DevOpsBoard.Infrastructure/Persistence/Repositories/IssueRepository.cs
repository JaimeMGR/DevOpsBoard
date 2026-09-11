using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public IssueRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Issue?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Issues
            .AsNoTracking()
            .FirstOrDefaultAsync(
                issue => issue.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Issue>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Issues
            .AsNoTracking()
            .Where(
                issue => issue.ProjectId == projectId
            )
            .OrderByDescending(
                issue => issue.CreatedAt
            )
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Issue issue,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Issues.AddAsync(
            issue,
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

    public async Task<Issue?> GetByIdForUpdateAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Issues
            .FirstOrDefaultAsync(
                issue => issue.Id == id,
                cancellationToken
            );
    }

    public void Remove(Issue issue)
    {
        _dbContext.Issues.Remove(issue);
    }
}