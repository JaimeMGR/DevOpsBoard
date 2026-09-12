using System.Security.Claims;
using DevOpsBoard.Application.Abstractions;
using DevOpsBoard.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsBoard.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/labels")]
[Authorize]
public class LabelsController : ControllerBase
{
    private readonly ILabelService _labelService;

    public LabelsController(
        ILabelService labelService)
    {
        _labelService = labelService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<LabelDto>>
    > GetAll(
        Guid projectId,
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
            await _labelService.GetByProjectIdAsync(
                projectId,
                actingUserId,
                cancellationToken
            );

        return Ok(labels);
    }

    [HttpGet("{labelId:guid}")]
    public async Task<ActionResult<LabelDto>> GetById(
        Guid projectId,
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

        var label =
            await _labelService.GetByIdAsync(
                projectId,
                labelId,
                actingUserId,
                cancellationToken
            );

        if (label is null)
        {
            return NotFound();
        }

        return Ok(label);
    }

    [HttpPost]
    public async Task<ActionResult<LabelDto>> Create(
        Guid projectId,
        CreateLabelRequest request,
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
            await _labelService.CreateAsync(
                projectId,
                request,
                actingUserId,
                cancellationToken
            );

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                projectId,
                labelId = label.Id
            },
            label
        );
    }

    [HttpPatch("{labelId:guid}")]
    public async Task<ActionResult<LabelDto>> Update(
        Guid projectId,
        Guid labelId,
        UpdateLabelRequest request,
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
            await _labelService.UpdateAsync(
                projectId,
                labelId,
                request,
                actingUserId,
                cancellationToken
            );

        return Ok(label);
    }

    [HttpDelete("{labelId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
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

        await _labelService.DeleteAsync(
            projectId,
            labelId,
            actingUserId,
            cancellationToken
        );

        return NoContent();
    }
}