using System.Security.Claims;
using DevOpsBoard.Application.Security;
using Microsoft.AspNetCore.Authorization;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TeamDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var teams = await _teamService.GetAllAsync(
            cancellationToken
        );

        return Ok(teams);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var team = await _teamService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (team is null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Manager}")]
    public async Task<ActionResult<TeamDto>> Create(
    CreateTeamRequest request,
    CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var team = await _teamService.CreateAsync(
            request,
            userId,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = team.Id },
            team
        );
    }
}