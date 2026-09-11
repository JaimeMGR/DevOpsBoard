namespace DevOpsBoard.Application.DTOs;

public record IssueDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string ReporterId,
    string? AssigneeId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);