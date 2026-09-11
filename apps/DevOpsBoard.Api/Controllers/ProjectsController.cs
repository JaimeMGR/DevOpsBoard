using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(
        IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAllAsync(
            cancellationToken
        );

        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (project is null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }

        var project = await _projectService.CreateAsync(
            request,
            ownerId,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = project.Id },
            project
        );
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> Update(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        var project = await _projectService.UpdateAsync(
            id,
            request,
            actingUserId,
            cancellationToken
        );

        return Ok(project);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        await _projectService.DeleteAsync(
            id,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}