using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route(
    "api/projects/{projectId:guid}/issues/{issueId:guid}/history"
)]
[Authorize]
public class IssueHistoryController : ControllerBase
{
    private readonly IIssueHistoryService _historyService;

    public IssueHistoryController(
        IIssueHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<IssueHistoryDto>>
    > GetAll(
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

        var history =
            await _historyService.GetByIssueIdAsync(
                projectId,
                issueId,
                actingUserId,
                cancellationToken
            );

        return Ok(history);
    }
}