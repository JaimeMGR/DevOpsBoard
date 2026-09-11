using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Abstractions;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ProjectMemberReadModel>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        ProjectMember member,
        CancellationToken cancellationToken = default
    );

    void Remove(ProjectMember member);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}