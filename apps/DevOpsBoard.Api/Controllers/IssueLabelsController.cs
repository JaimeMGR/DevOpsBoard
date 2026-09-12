using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route(
    "api/projects/{projectId:guid}/issues/{issueId:guid}/labels"
)]
[Authorize]
public class IssueLabelsController : ControllerBase
{
    private readonly IIssueLabelService _issueLabelService;

    public IssueLabelsController(
        IIssueLabelService issueLabelService)
    {
        _issueLabelService = issueLabelService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<LabelDto>>
    > GetAll(
        Guid projectId,
        Guid issueId,
        CancellationToken cancellationToken)
    {
        var actingUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (string.IsNullOrWhiteSpace(
                actingUserId))
        {
            return Unauthorized();
        }

        var labels =
            await _issueLabelService.GetByIssueIdAsync(
                projectId,
                issueId,
                actingUserId,
                cancellationToken
            );

        return Ok(labels);
    }

    [HttpPost]
    public async Task<ActionResult<LabelDto>> Add(
        Guid projectId,
        Guid issueId,
        AssignIssueLabelRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (string.IsNullOrWhiteSpace(
                actingUserId))
        {
            return Unauthorized();
        }

        var label =
            await _issueLabelService.AddAsync(
                projectId,
                issueId,
                request.LabelId,
                actingUserId,
                cancellationToken
            );

        return Ok(label);
    }

    [HttpDelete("{labelId:guid}")]
    public async Task<IActionResult> Remove(
        Guid projectId,
        Guid issueId,
        Guid labelId,
        CancellationToken cancellationToken)
    {
        var actingUserId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (string.IsNullOrWhiteSpace(
                actingUserId))
        {
            return Unauthorized();
        }

        await _issueLabelService.RemoveAsync(
            projectId,
            issueId,
            labelId,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}