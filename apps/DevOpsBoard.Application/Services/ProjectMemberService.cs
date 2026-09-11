using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using DevOpsBoard.Application.Exceptions;
using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Application.Services;

public class ProjectMemberService
    : IProjectMemberService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectMemberAuthorizationService
        _authorizationService;

    public ProjectMemberService(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IUserRepository userRepository,
        IProjectMemberAuthorizationService authorizationService)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
        _authorizationService = authorizationService;
    }

    public async Task<ProjectMemberDto> AddAsync(
        Guid projectId,
        AddProjectMemberRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
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

        var project = await _projectRepository.GetByIdAsync(
            projectId,
            cancellationToken
        );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var canManage =
            await _authorizationService.CanManageMembersAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar los miembros de este proyecto."
            );
        }

        if (!Enum.TryParse<ProjectRole>(
                request.Role,
                true,
                out var role))
        {
            throw new ValidationException(
                "El rol debe ser 'Viewer', 'Developer' o 'Manager'."
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
            await _projectMemberRepository.GetAsync(
                projectId,
                request.UserId,
                cancellationToken
            );

        if (existingMember is not null)
        {
            throw new ConflictException(
                "El usuario ya pertenece al proyecto."
            );
        }

        var member = new ProjectMember(
            projectId,
            request.UserId,
            role
        );

        await _projectMemberRepository.AddAsync(
            member,
            cancellationToken
        );

        await _projectMemberRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(member, user);
    }

    public async Task<IReadOnlyList<ProjectMemberDto>>
        GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(
            projectId,
            cancellationToken
        );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var members =
            await _projectMemberRepository.GetByProjectIdAsync(
                projectId,
                cancellationToken
            );

        return members
            .Select(member =>
                new ProjectMemberDto(
                    member.UserId,
                    member.DisplayName,
                    member.Email,
                    member.Role,
                    member.JoinedAt
                )
            )
            .ToList();
    }

    public async Task<ProjectMemberDto> UpdateRoleAsync(
        Guid projectId,
        string userId,
        UpdateProjectMemberRoleRequest request,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario que realiza la operación es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ValidationException(
                "El rol es obligatorio."
            );
        }

        var project =
            await _projectRepository.GetByIdAsync(
                projectId,
                cancellationToken
            );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var canManage =
            await _authorizationService.CanManageMembersAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar los miembros de este proyecto."
            );
        }

        if (!Enum.TryParse<ProjectRole>(
                request.Role,
                true,
                out var role))
        {
            throw new ValidationException(
                "El rol debe ser 'Viewer', 'Developer' o 'Manager'."
            );
        }

        var member =
            await _projectMemberRepository.GetAsync(
                projectId,
                userId,
                cancellationToken
            );

        if (member is null)
        {
            throw new NotFoundException(
                "El usuario no pertenece al proyecto."
            );
        }

        var user =
            await _userRepository.GetSummaryByIdAsync(
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

        await _projectMemberRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToDto(member, user);
    }

    public async Task RemoveAsync(
        Guid projectId,
        string userId,
        string actingUserId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty)
        {
            throw new ValidationException(
                "El proyecto es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException(
                "El usuario es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            throw new ValidationException(
                "El usuario que realiza la operación es obligatorio."
            );
        }

        var project =
            await _projectRepository.GetByIdAsync(
                projectId,
                cancellationToken
            );

        if (project is null)
        {
            throw new NotFoundException(
                "El proyecto no existe."
            );
        }

        var canManage =
            await _authorizationService.CanManageMembersAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        if (!canManage)
        {
            throw new ForbiddenException(
                "No tienes permisos para gestionar los miembros de este proyecto."
            );
        }

        var member =
            await _projectMemberRepository.GetAsync(
                projectId,
                userId,
                cancellationToken
            );

        if (member is null)
        {
            throw new NotFoundException(
                "El usuario no pertenece al proyecto."
            );
        }

        _projectMemberRepository.Remove(member);

        await _projectMemberRepository.SaveChangesAsync(
            cancellationToken
        );
    }

    private static ProjectMemberDto MapToDto(
        ProjectMember member,
        UserSummaryDto user)
    {
        return new ProjectMemberDto(
            member.UserId,
            user.DisplayName,
            user.Email,
            member.Role.ToString(),
            member.JoinedAt
        );
    }
}