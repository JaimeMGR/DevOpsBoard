using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;
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
            .ThenBy(
                issue => issue.Id
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Issue>>
        GetPagedByProjectIdAsync(
            Guid projectId,
            IssueQueryParameters query,
            CancellationToken cancellationToken = default)
    {
        IQueryable<Issue> issues =
            _dbContext.Issues
                .AsNoTracking()
                .Where(
                    issue =>
                        issue.ProjectId == projectId
                );

        if (!string.IsNullOrWhiteSpace(
                query.Status))
        {
            if (Enum.TryParse<IssueStatus>(
                    query.Status,
                    true,
                    out var status))
            {
                issues = issues.Where(
                    issue =>
                        issue.Status == status
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(
                query.Priority))
        {
            if (Enum.TryParse<IssuePriority>(
                    query.Priority,
                    true,
                    out var priority))
            {
                issues = issues.Where(
                    issue =>
                        issue.Priority == priority
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(
                query.AssigneeId))
        {
            issues = issues.Where(
                issue =>
                    issue.AssigneeId ==
                    query.AssigneeId
            );
        }

        if (!string.IsNullOrWhiteSpace(
                query.Search))
        {
            var search =
                query.Search.Trim();

            var pattern =
                $"%{search}%";

            issues = issues.Where(
                issue =>
                    EF.Functions.ILike(
                        issue.Title,
                        pattern
                    )
                    ||
                    (
                        issue.Description != null &&
                        EF.Functions.ILike(
                            issue.Description,
                            pattern
                        )
                    )
            );
        }

        issues = ApplySorting(
            issues,
            query.SortBy,
            query.SortDirection
        );

        var totalCount =
            await issues.CountAsync(
                cancellationToken
            );

        var skip =
            (query.Page - 1) *
            query.PageSize;

        var items =
            await issues
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync(
                    cancellationToken
                );

        return new PagedResult<Issue>(
            items,
            query.Page,
            query.PageSize,
            totalCount
        );
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

    public void Remove(
        Issue issue)
    {
        _dbContext.Issues.Remove(
            issue
        );
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken
        );
    }

    private static IQueryable<Issue> ApplySorting(
        IQueryable<Issue> query,
        string sortBy,
        string sortDirection)
    {
        var descending =
            string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

        return sortBy.ToLowerInvariant() switch
        {
            "title" =>
                descending
                    ? query
                        .OrderByDescending(
                            issue => issue.Title
                        )
                        .ThenBy(
                            issue => issue.Id
                        )
                    : query
                        .OrderBy(
                            issue => issue.Title
                        )
                        .ThenBy(
                            issue => issue.Id
                        ),

            "status" =>
                descending
                    ? query
                        .OrderByDescending(
                            issue => issue.Status
                        )
                        .ThenBy(
                            issue => issue.Id
                        )
                    : query
                        .OrderBy(
                            issue => issue.Status
                        )
                        .ThenBy(
                            issue => issue.Id
                        ),

            "priority" =>
                descending
                    ? query
                        .OrderByDescending(
                            issue => issue.Priority
                        )
                        .ThenBy(
                            issue => issue.Id
                        )
                    : query
                        .OrderBy(
                            issue => issue.Priority
                        )
                        .ThenBy(
                            issue => issue.Id
                        ),

            "updatedat" =>
                descending
                    ? query
                        .OrderByDescending(
                            issue => issue.UpdatedAt
                        )
                        .ThenBy(
                            issue => issue.Id
                        )
                    : query
                        .OrderBy(
                            issue => issue.UpdatedAt
                        )
                        .ThenBy(
                            issue => issue.Id
                        ),

            _ =>
                descending
                    ? query
                        .OrderByDescending(
                            issue => issue.CreatedAt
                        )
                        .ThenBy(
                            issue => issue.Id
                        )
                    : query
                        .OrderBy(
                            issue => issue.CreatedAt
                        )
                        .ThenBy(
                            issue => issue.Id
                        )
        };
    }
}