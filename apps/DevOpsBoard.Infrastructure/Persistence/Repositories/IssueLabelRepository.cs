using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class IssueLabelRepository : IIssueLabelRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public IssueLabelRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Label>> GetLabelsByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IssueLabels
            .AsNoTracking()
            .Where(
                issueLabel =>
                    issueLabel.IssueId == issueId
            )
            .Join(
                _dbContext.Labels.AsNoTracking(),
                issueLabel => issueLabel.LabelId,
                label => label.Id,
                (_, label) => label
            )
            .OrderBy(
                label => label.Name
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid issueId,
        Guid labelId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IssueLabels
            .AsNoTracking()
            .AnyAsync(
                issueLabel =>
                    issueLabel.IssueId == issueId &&
                    issueLabel.LabelId == labelId,
                cancellationToken
            );
    }

    public async Task AddAsync(
        IssueLabel issueLabel,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IssueLabels.AddAsync(
            issueLabel,
            cancellationToken
        );
    }

    public async Task<IssueLabel?> GetAsync(
        Guid issueId,
        Guid labelId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IssueLabels
            .FirstOrDefaultAsync(
                issueLabel =>
                    issueLabel.IssueId == issueId &&
                    issueLabel.LabelId == labelId,
                cancellationToken
            );
    }

    public void Remove(
        IssueLabel issueLabel)
    {
        _dbContext.IssueLabels.Remove(
            issueLabel
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