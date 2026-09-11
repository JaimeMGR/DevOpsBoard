namespace DevOpsBoard.Application.DTOs;

public record ProjectMemberReadModel(
    string UserId,
    string DisplayName,
    string Email,
    string Role,
    DateTime JoinedAt
);