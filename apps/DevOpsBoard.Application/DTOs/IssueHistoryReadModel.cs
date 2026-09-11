namespace DevOpsBoard.Application.DTOs;

public record IssueHistoryReadModel(
    Guid Id,
    Guid IssueId,
    string ActorId,
    string ActorDisplayName,
    string ActorEmail,
    string Action,
    string? OldValue,
    string? NewValue,
    Guid CorrelationId,
    DateTime CreatedAt
);