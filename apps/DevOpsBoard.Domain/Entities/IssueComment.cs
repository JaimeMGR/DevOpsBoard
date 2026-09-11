namespace DevOpsBoard.Domain.Entities;

public class IssueComment
{
    public Guid Id { get; private set; }

    public Guid IssueId { get; private set; }

    public string AuthorId { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IssueComment(
        Guid issueId,
        string authorId,
        string content)
    {
        if (issueId == Guid.Empty)
        {
            throw new ArgumentException(
                "La incidencia es obligatoria.",
                nameof(issueId)
            );
        }

        if (string.IsNullOrWhiteSpace(authorId))
        {
            throw new ArgumentException(
                "El autor es obligatorio.",
                nameof(authorId)
            );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "El contenido del comentario es obligatorio.",
                nameof(content)
            );
        }

        Id = Guid.NewGuid();
        IssueId = issueId;
        AuthorId = authorId;
        Content = content.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void UpdateContent(
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "El contenido del comentario es obligatorio.",
                nameof(content)
            );
        }

        Content = content.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private IssueComment()
    {
        // Constructor requerido por Entity Framework Core.
    }
}