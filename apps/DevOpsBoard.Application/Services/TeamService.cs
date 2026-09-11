using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Application.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;

    public TeamService(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<TeamDto> CreateAsync(
        CreateTeamRequest request,
        string createdByUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException(
                "El nombre del equipo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            throw new ValidationException(
                "El usuario creador es obligatorio."
            );
        }

        var normalizedName = request.Name.Trim();

        var exists = await _teamRepository.ExistsByNameAsync(
            normalizedName,
            cancellationToken
        );

        if (exists)
        {
            throw new ConflictException(
                $"Ya existe un equipo con el nombre '{normalizedName}'."
            );
        }

        var team = new Team(
            normalizedName,
            request.Description,
            createdByUserId
        );

        await _teamRepository.AddAsync(
            team,
            cancellationToken
        );

        await _teamRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(team);
    }

    public async Task<IReadOnlyList<TeamDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var teams = await _teamRepository.GetAllAsync(
            cancellationToken
        );

        return teams
            .Select(MapToDto)
            .ToList();
    }

    public async Task<TeamDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(
            id,
            cancellationToken
        );

        return team is null
            ? null
            : MapToDto(team);
    }

    private static TeamDto MapToDto(Team team)
    {
        return new TeamDto(
            team.Id,
            team.Name,
            team.Description,
            team.CreatedAt
        );
    }
}