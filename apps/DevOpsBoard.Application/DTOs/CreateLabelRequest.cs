namespace DevOpsBoard.Application.DTOs;

public record CreateLabelRequest(
    string Name,
    string Color
);