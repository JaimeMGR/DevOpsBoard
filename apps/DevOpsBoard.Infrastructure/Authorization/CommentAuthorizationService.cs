using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.Security;
using DevOpsBoard.Domain.Enums;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Authorization;

public class CommentAuthorizationService
    : ICommentAuthorizationService
{
    private readonly DevOpsBoardDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentAuthorizationService(
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
                        member.Role == ProjectRole.Developer ||
                        member.Role == ProjectRole.Manager
                    ),
                cancellationToken
            );
    }

    public async Task<bool> CanModifyAsync(
        Guid projectId,
        string userId,
        string authorId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(authorId))
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

        var member = await _dbContext.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId,
                cancellationToken
            );

        if (member is null)
        {
            return false;
        }

        if (member.Role == ProjectRole.Manager)
        {
            return true;
        }

        return member.Role == ProjectRole.Developer &&
               userId == authorId;
    }

    public async Task<bool> CanDeleteAsync(
        Guid projectId,
        string userId,
        string authorId,
        CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(authorId))
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

        var member = await _dbContext.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                member =>
                    member.ProjectId == projectId &&
                    member.UserId == userId,
                cancellationToken
            );

        if (member is null)
        {
            return false;
        }

        if (member.Role == ProjectRole.Manager)
        {
            return true;
        }

        return member.Role == ProjectRole.Developer &&
               userId == authorId;
    }
}