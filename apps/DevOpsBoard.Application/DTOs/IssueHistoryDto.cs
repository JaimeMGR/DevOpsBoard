namespace DevOpsBoard.Application.DTOs;

public record IssueHistoryDto(
    Guid Id,
    string ActorId,
    string ActorDisplayName,
    string ActorEmail,
    string Action,
    string? OldValue,
    string? NewValue,
    Guid CorrelationId,
    DateTime CreatedAt
);