using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> ExistsAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<UserSummaryDto?> GetSummaryByIdAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
}