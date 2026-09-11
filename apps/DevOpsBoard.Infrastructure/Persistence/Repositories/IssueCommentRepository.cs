using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class IssueCommentRepository
    : IIssueCommentRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public IssueCommentRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IssueComment?> GetByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IssueComments
            .FirstOrDefaultAsync(
                comment => comment.Id == commentId,
                cancellationToken
            );
    }

    public async Task<
        IReadOnlyList<IssueCommentReadModel>
    > GetByIssueIdAsync(
        Guid issueId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from comment in _dbContext.IssueComments
            join user in _dbContext.Users
                on comment.AuthorId equals user.Id
            where comment.IssueId == issueId
            orderby comment.CreatedAt
            select new IssueCommentReadModel(
                comment.Id,
                comment.IssueId,
                comment.AuthorId,
                user.DisplayName,
                user.Email ?? string.Empty,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt
            )
        )
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        IssueComment comment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.IssueComments.AddAsync(
            comment,
            cancellationToken
        );
    }

    public void Remove(
        IssueComment comment)
    {
        _dbContext.IssueComments.Remove(
            comment
        );
    }
}