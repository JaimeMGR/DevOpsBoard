namespace DevOpsBoard.Application.DTOs;

public record CreateProjectRequest(
    string Name,
    string Key,
    string? Description
);