using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/issues")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _issueService;

    public IssuesController(
        IIssueService issueService)
    {
        _issueService = issueService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<IssueDto>>
    > GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var issues =
            await _issueService.GetByProjectIdAsync(
                projectId,
                cancellationToken
            );

        return Ok(issues);
    }

    [HttpGet("{issueId:guid}")]
    public async Task<ActionResult<IssueDto>> GetById(
        Guid projectId,
        Guid issueId,
        CancellationToken cancellationToken)
    {
        var issue =
            await _issueService.GetByIdAsync(
                projectId,
                issueId,
                cancellationToken
            );

        if (issue is null)
        {
            return NotFound();
        }

        return Ok(issue);
    }

    [HttpPost]
    public async Task<ActionResult<IssueDto>> Create(
        Guid projectId,
        CreateIssueRequest request,
        CancellationToken cancellationToken)
    {
        var reporterId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(reporterId))
        {
            return Unauthorized();
        }

        var issue =
            await _issueService.CreateAsync(
                projectId,
                request,
                reporterId,
                cancellationToken
            );

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                projectId,
                issueId = issue.Id
            },
            issue
        );
    }
    [HttpPatch("{issueId:guid}")]
    public async Task<ActionResult<IssueDto>> Update(
    Guid projectId,
    Guid issueId,
    UpdateIssueRequest request,
    CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        var issue =
            await _issueService.UpdateAsync(
                projectId,
                issueId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(issue);
    }

    [HttpDelete("{issueId:guid}")]
    public async Task<IActionResult> Delete(
    Guid projectId,
    Guid issueId,
    CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        await _issueService.DeleteAsync(
            projectId,
            issueId,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}