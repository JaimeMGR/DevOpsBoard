namespace DevOpsBoard.Application.DTOs;

public record AddProjectMemberRequest(
    string UserId,
    string Role
);