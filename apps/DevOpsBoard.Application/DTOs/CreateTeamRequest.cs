namespace DevOpsBoard.Application.DTOs;

public record CreateTeamRequest(
    string Name,
    string? Description
);