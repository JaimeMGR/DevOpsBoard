namespace DevOpsBoard.Application.DTOs;

public record CurrentUserDto(
    string Id,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles
);