using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route(
    "api/projects/{projectId:guid}/issues/{issueId:guid}/comments"
)]
[Authorize]
public class IssueCommentsController : ControllerBase
{
    private readonly IIssueCommentService _commentService;

    public IssueCommentsController(
        IIssueCommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<IssueCommentDto>>
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

        var comments =
            await _commentService.GetByIssueIdAsync(
                projectId,
                issueId,
                actingUserId,
                cancellationToken
            );

        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<IssueCommentDto>> Create(
        Guid projectId,
        Guid issueId,
        CreateIssueCommentRequest request,
        CancellationToken cancellationToken)
    {
        var authorId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(authorId))
        {
            return Unauthorized();
        }

        var comment =
            await _commentService.CreateAsync(
                projectId,
                issueId,
                request,
                authorId,
                cancellationToken
            );

        return Ok(comment);
    }

    [HttpPatch("{commentId:guid}")]
    public async Task<ActionResult<IssueCommentDto>> Update(
        Guid projectId,
        Guid issueId,
        Guid commentId,
        UpdateIssueCommentRequest request,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        var comment =
            await _commentService.UpdateAsync(
                projectId,
                issueId,
                commentId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(comment);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid issueId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        if (string.IsNullOrWhiteSpace(actingUserId))
        {
            return Unauthorized();
        }

        await _commentService.DeleteAsync(
            projectId,
            issueId,
            commentId,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}