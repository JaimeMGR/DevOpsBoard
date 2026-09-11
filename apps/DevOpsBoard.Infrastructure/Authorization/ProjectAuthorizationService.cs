using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.Security;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Authorization;

public class ProjectAuthorizationService
    : IProjectAuthorizationService
{
    private readonly DevOpsBoardDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectAuthorizationService(
        DevOpsBoardDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<bool> CanManageAsync(
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

        return await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(
                project =>
                    project.Id == projectId &&
                    project.OwnerId == userId,
                cancellationToken
            );
    }
}