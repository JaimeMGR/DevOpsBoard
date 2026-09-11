namespace DevOpsBoard.Application.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string Key,
    string? Description,
    DateTime CreatedAt,
    string OwnerId
);