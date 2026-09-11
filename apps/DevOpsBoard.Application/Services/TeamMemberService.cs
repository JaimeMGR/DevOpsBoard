using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Application.Services;

public class TeamMemberService : ITeamMemberService
{
    private readonly ITeamAuthorizationService _teamAuthorizationService;
    private readonly ITeamRepository _teamRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;
    private readonly IUserRepository _userRepository;

    public TeamMemberService(
        ITeamRepository teamRepository,
        ITeamMemberRepository teamMemberRepository,
        IUserRepository userRepository,
        ITeamAuthorizationService teamAuthorizationService)
    {
        _teamRepository = teamRepository;
        _teamMemberRepository = teamMemberRepository;
        _userRepository = userRepository;
        _teamAuthorizationService = teamAuthorizationService;
    }

    public async Task<TeamMemberDto> AddAsync(
        Guid teamId,
        AddTeamMemberRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (teamId == Guid.Empty)
        {
            throw new ValidationException(
                "El equipo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario que realiza la operación es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ValidationException(
                "El rol es obligatorio."
            );
        }

        var team = await _teamRepository.GetByIdAsync(
            teamId,
            cancellationToken
        );

        if (team is null)
        {
            throw new NotFoundException(
                "El equipo no existe."
            );
        }

        var canManage =
            await _teamAuthorizationService.CanManageMembersAsync(
                teamId,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar los miembros de este equipo."
            );
        }

        if (!Enum.TryParse<TeamRole>(
                request.Role,
                true,
                out var role))
        {
            throw new ValidationException(
                "El rol debe ser 'Member' o 'Lead'."
            );
        }

        var user = await _userRepository.GetSummaryByIdAsync(
            request.UserId,
            cancellationToken
        );

        if (user is null)
        {
            throw new NotFoundException(
                "El usuario no existe."
            );
        }

        var existingMember =
            await _teamMemberRepository.GetAsync(
                teamId,
                request.UserId,
                cancellationToken
            );

        if (existingMember is not null)
        {
            throw new ConflictException(
                "El usuario ya pertenece al equipo."
            );
        }

        var member = new TeamMember(
            teamId,
            request.UserId,
            role
        );

        await _teamMemberRepository.AddAsync(
            member,
            cancellationToken
        );

        await _teamMemberRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(member, user);
    }

    public async Task<IReadOnlyList<TeamMemberDto>> GetByTeamIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        var team = await _teamRepository.GetByIdAsync(
            teamId,
            cancellationToken
        );

        if (team is null)
        {
            throw new NotFoundException(
                "El equipo no existe."
            );
        }

        var members =
            await _teamMemberRepository.GetByTeamIdAsync(
                teamId,
                cancellationToken
            );

        return members
            .Select(member => new TeamMemberDto(
                member.UserId,
                member.DisplayName,
                member.Email,
                member.Role,
                member.JoinedAt
            ))
            .ToList();
    }

    public async Task<TeamMemberDto> UpdateRoleAsync(
        Guid teamId,
        string userId,
        UpdateTeamMemberRoleRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (teamId == Guid.Empty)
        {
            throw new ValidationException(
                "El equipo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario que realiza la operación es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ValidationException(
                "El rol es obligatorio."
            );
        }

        var team = await _teamRepository.GetByIdAsync(
            teamId,
            cancellationToken
        );

        if (team is null)
        {
            throw new NotFoundException(
                "El equipo no existe."
            );
        }

        var canManage =
            await _teamAuthorizationService.CanManageMembersAsync(
                teamId,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar los miembros de este equipo."
            );
        }

        if (!Enum.TryParse<TeamRole>(
                request.Role,
                true,
                out var role))
        {
            throw new ValidationException(
                "El rol debe ser 'Member' o 'Lead'."
            );
        }

        var member = await _teamMemberRepository.GetAsync(
            teamId,
            userId,
            cancellationToken
        );

        if (member is null)
        {
            throw new NotFoundException(
                "El usuario no pertenece al equipo."
            );
        }

        var user = await _userRepository.GetSummaryByIdAsync(
            userId,
            cancellationToken
        );

        if (user is null)
        {
            throw new NotFoundException(
                "El usuario no existe."
            );
        }

        member.ChangeRole(role);

        await _teamMemberRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(member, user);
    }

    public async Task RemoveAsync(
        Guid teamId,
        string userId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (teamId == Guid.Empty)
        {
            throw new ValidationException(
                "El equipo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario que realiza la operación es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        var team = await _teamRepository.GetByIdAsync(
            teamId,
            cancellationToken
        );

        if (team is null)
        {
            throw new NotFoundException(
                "El equipo no existe."
            );
        }

        var canManage =
            await _teamAuthorizationService.CanManageMembersAsync(
                teamId,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar los miembros de este equipo."
            );
        }

        var member =
            await _teamMemberRepository.GetAsync(
                teamId,
                userId,
                cancellationToken
            );

        if (member is null)
        {
            throw new NotFoundException(
                "El usuario no pertenece al equipo."
            );
        }

        _teamMemberRepository.Remove(member);

        await _teamMemberRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private static TeamMemberDto MapToDto(
        TeamMember member,
        UserSummaryDto user)
    {
        return new TeamMemberDto(
            member.UserId,
            user.DisplayName,
            user.Email,
            member.Role.ToString(),
            member.JoinedAt
        );
    }
}