using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface ILabelRepository
{
    Task<Label?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Label>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByNameAsync(
        Guid projectId,
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Label label,
        CancellationToken cancellationToken = default
    );

    void Remove(Label label);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}