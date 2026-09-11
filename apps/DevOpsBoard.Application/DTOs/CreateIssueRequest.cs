namespace DevOpsBoard.Application.DTOs;

public record CreateIssueRequest(
    string Title,
    string? Description,
    string Priority
);