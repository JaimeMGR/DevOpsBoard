namespace DevOpsBoard.Application.DTOs;

public record IssueCommentDto(
    Guid Id,
    string AuthorId,
    string AuthorDisplayName,
    string AuthorEmail,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);