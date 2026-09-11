namespace DevOpsBoard.Application.DTOs;

public record UpdateProjectRequest(
    string Name,
    string? Description
);