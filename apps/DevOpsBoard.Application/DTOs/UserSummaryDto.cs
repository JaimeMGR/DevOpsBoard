namespace DevOpsBoard.Application.DTOs;

public record UserSummaryDto(
    string Id,
    string DisplayName,
    string Email
);