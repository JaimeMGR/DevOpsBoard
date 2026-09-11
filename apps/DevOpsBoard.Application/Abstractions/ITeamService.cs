using DevOpsBoard.Application.DTOs;

namespace DevOpsBoard.Application.Abstractions;

public interface ITeamService
{
    Task<TeamDto> CreateAsync(
    CreateTeamRequest request,
    string createdByUserId,
    CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TeamDto>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<TeamDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}