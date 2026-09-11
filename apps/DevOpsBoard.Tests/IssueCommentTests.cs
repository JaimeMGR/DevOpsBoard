using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Tests;

public class IssueCommentTests
{
    private static readonly Guid IssueId =
        Guid.NewGuid();

    private const string AuthorId =
        "author-user-id";

    [Fact]
    public void Constructor_ShouldCreateComment()
    {
        var comment = new IssueComment(
            IssueId,
            AuthorId,
            "  Comentario de prueba  "
        );

        Assert.NotEqual(
            Guid.Empty,
            comment.Id
        );

        Assert.Equal(
            IssueId,
            comment.IssueId
        );

        Assert.Equal(
            AuthorId,
            comment.AuthorId
        );

        Assert.Equal(
            "Comentario de prueba",
            comment.Content
        );

        Assert.Equal(
            comment.CreatedAt,
            comment.UpdatedAt
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyContent()
    {
        Assert.Throws<ArgumentException>(
            () => new IssueComment(
                IssueId,
                AuthorId,
                ""
            )
        );
    }

    [Fact]
    public void UpdateContent_ShouldChangeContent()
    {
        var comment = new IssueComment(
            IssueId,
            AuthorId,
            "Original"
        );

        var originalUpdatedAt =
            comment.UpdatedAt;

        comment.UpdateContent(
            "Actualizado"
        );

        Assert.Equal(
            "Actualizado",
            comment.Content
        );

        Assert.True(
            comment.UpdatedAt >= originalUpdatedAt
        );
    }
}