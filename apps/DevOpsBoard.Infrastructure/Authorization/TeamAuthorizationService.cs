using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.Security;
using DevOpsBoard.Infrastructure.Identity;
using DevOpsBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevOpsBoard.Infrastructure.Authorization;

public class TeamAuthorizationService : ITeamAuthorizationService
{
    private readonly DevOpsBoardDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeamAuthorizationService(
        DevOpsBoardDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<bool> CanManageMembersAsync(
        Guid teamId,
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

        var membership = await _dbContext.TeamMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                member =>
                    member.TeamId == teamId &&
                    member.UserId == userId,
                cancellationToken
            );

        return membership?.Role == Domain.Enums.TeamRole.Lead;
    }
}