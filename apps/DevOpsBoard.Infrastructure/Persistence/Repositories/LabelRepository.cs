using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class LabelRepository : ILabelRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public LabelRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Label?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Labels
            .FirstOrDefaultAsync(
                label => label.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Label>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Labels
            .AsNoTracking()
            .Where(
                label => label.ProjectId == projectId
            )
            .OrderBy(
                label => label.Name
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        Guid projectId,
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();

        var query = _dbContext.Labels
            .AsNoTracking()
            .Where(
                label =>
                    label.ProjectId == projectId &&
                    label.Name == normalizedName
            );

        if (excludingId.HasValue)
        {
            query = query.Where(
                label => label.Id != excludingId.Value
            );
        }

        return await query.AnyAsync(
            cancellationToken
        );
    }

    public async Task AddAsync(
        Label label,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Labels.AddAsync(
            label,
            cancellationToken
        );
    }

    public void Remove(
        Label label)
    {
        _dbContext.Labels.Remove(label);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}