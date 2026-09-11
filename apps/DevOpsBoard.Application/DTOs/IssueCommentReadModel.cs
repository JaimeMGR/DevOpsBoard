namespace DevOpsBoard.Application.DTOs;

public record IssueCommentReadModel(
    Guid Id,
    Guid IssueId,
    string AuthorId,
    string AuthorDisplayName,
    string AuthorEmail,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);