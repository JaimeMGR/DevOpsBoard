namespace DevOpsBoard.Application.DTOs;

public record TeamMemberReadModel(
    string UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime JoinedAt
);