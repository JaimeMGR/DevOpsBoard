namespace DevOpsBoard.Application.DTOs;

public record AddTeamMemberRequest(
    string UserId,
    string Role
);