using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/members")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;

    public ProjectMembersController(
        IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<ProjectMemberDto>>
    > GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var members =
            await _projectMemberService.GetByProjectIdAsync(
                projectId,
                cancellationToken
            );

        return Ok(members);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectMemberDto>> Add(
        Guid projectId,
        AddProjectMemberRequest request,
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
            await _projectMemberService.AddAsync(
                projectId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(member);
    }

    [HttpPatch("{userId}")]
    public async Task<ActionResult<ProjectMemberDto>> UpdateRole(
        Guid projectId,
        string userId,
        UpdateProjectMemberRoleRequest request,
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
            await _projectMemberService.UpdateRoleAsync(
                projectId,
                userId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(member);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Remove(
        Guid projectId,
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

        await _projectMemberService.RemoveAsync(
            projectId,
            userId,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}