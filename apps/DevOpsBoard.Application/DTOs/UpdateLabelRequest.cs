namespace DevOpsBoard.Application.DTOs;

public record UpdateLabelRequest(
    string Name,
    string Color
);