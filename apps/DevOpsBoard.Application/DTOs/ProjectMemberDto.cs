namespace DevOpsBoard.Application.DTOs;

public record ProjectMemberDto(
    string UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime JoinedAt
);