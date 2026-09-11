namespace DevOpsBoard.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt
);