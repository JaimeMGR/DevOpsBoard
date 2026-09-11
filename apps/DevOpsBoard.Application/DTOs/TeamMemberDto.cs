namespace DevOpsBoard.Application.DTOs;

public record TeamMemberDto(
    string UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime JoinedAt
);