using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<Project?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByKeyAsync(
        string key,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default
    );

    void Remove(Project project);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}