using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class IssueHistoryRepository
    : IIssueHistoryRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public IssueHistoryRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        IssueHistory history,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IssueHistories.AddAsync(
            history,
            cancellationToken
        );
    }

    public async Task<
        IReadOnlyList<IssueHistoryReadModel>
    > GetByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from history in _dbContext.IssueHistories
            join user in _dbContext.Users
                on history.ActorId equals user.Id
            where history.IssueId == issueId
            orderby history.CreatedAt
            select new IssueHistoryReadModel(
                history.Id,
                history.IssueId,
                history.ActorId,
                user.DisplayName,
                user.Email ?? string.Empty,
                history.Action.ToString(),
                history.OldValue,
                history.NewValue,
                history.CorrelationId,
                history.CreatedAt
            )
        )
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }
}