namespace DevOpsBoard.Application.DTOs;

public record TeamDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt
);