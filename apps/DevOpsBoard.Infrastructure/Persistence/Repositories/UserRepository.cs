using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DevOpsBoardDbContext _dbContext;

    public UserRepository(
        DevOpsBoardDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(
                user => user.Id == userId,
                cancellationToken
            );
    }

    public async Task<UserSummaryDto?> GetSummaryByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new UserSummaryDto(
                user.Id,
                user.DisplayName,
                user.Email ?? string.Empty
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}