namespace DevOpsBoard.Application.DTOs;

public record LabelDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string Color
);