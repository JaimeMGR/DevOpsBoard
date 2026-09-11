using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.Security;
using DevOpsBoard.Domain.Enums;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Authorization;

public class IssueAuthorizationService
    : IIssueAuthorizationService
{
    private readonly DevOpsBoardDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public IssueAuthorizationService(
        DevOpsBoardDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<bool> CanCreateAsync(
    Guid projectId,
    string userId,
    CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(
            userId
        );

        if (user is null)
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(
                user,
                RoleNames.Admin))
        {
            return true;
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project => project.Id == projectId,
                cancellationToken
            );

        if (project is null)
        {
            return false;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId &&
                    (
                        member.Role == ProjectRole.Manager ||
                        member.Role == ProjectRole.Developer
                    ),
                cancellationToken
            );
    }

    public async Task<bool> CanViewAsync(
    Guid projectId,
    string userId,
    CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(
            userId
        );

        if (user is null)
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(
                user,
                RoleNames.Admin))
        {
            return true;
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project => project.Id == projectId,
                cancellationToken
            );

        if (project is null)
        {
            return false;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId,
                cancellationToken
            );
    }

    public async Task<bool> CanModifyAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(
                user,
                RoleNames.Admin))
        {
            return true;
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project => project.Id == projectId,
                cancellationToken
            );

        if (project is null)
        {
            return false;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId &&
                    (
                        member.Role == ProjectRole.Manager ||
                        member.Role == ProjectRole.Developer
                    ),
                cancellationToken
            );
    }

    public async Task<bool> CanDeleteAsync(
        Guid projectId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(
                user,
                RoleNames.Admin))
        {
            return true;
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(
                project => project.Id == projectId,
                cancellationToken
            );

        if (project is null)
        {
            return false;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .AnyAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId &&
                    member.Role == ProjectRole.Manager,
                cancellationToken
            );
    }
}