using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/teams/{teamId:guid}/members")]
[Authorize]
public class TeamMembersController : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService;

    public TeamMembersController(
        ITeamMemberService teamMemberService)
    {
        _teamMemberService = teamMemberService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeamMemberDto>>> GetAll(
        Guid teamId,
        CancellationToken cancellationToken)
    {
        var members =
            await _teamMemberService.GetByTeamIdAsync(
                teamId,
                cancellationToken
            );

        return Ok(members);
    }

    [HttpPost]
    public async Task<ActionResult<TeamMemberDto>> Add(
        Guid teamId,
        AddTeamMemberRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        var member =
            await _teamMemberService.AddAsync(
                teamId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(member);
    }

    [HttpPatch("{userId}")]
    public async Task<ActionResult<TeamMemberDto>> UpdateRole(
        Guid teamId,
        string userId,
        UpdateTeamMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        var member =
            await _teamMemberService.UpdateRoleAsync(
                teamId,
                userId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(member);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Remove(
        Guid teamId,
        string userId,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        await _teamMemberService.RemoveAsync(
            teamId,
            userId,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}