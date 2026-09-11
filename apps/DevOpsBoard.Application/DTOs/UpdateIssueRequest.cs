namespace DevOpsBoard.Application.DTOs;

public record UpdateIssueRequest(
    string Title,
    string? Description,
    string Status,
    string Priority,
    string? AssigneeId
);