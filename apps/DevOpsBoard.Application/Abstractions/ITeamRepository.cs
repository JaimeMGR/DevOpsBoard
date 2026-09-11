using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Team>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Team team,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}