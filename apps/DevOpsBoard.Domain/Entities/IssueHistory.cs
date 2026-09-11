using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Domain.Entities;

public class IssueHistory
{
    public Guid Id { get; private set; }

    public Guid IssueId { get; private set; }

    public string ActorId { get; private set; } = string.Empty;

    public Guid CorrelationId { get; private set; }

    public IssueHistoryAction Action { get; private set; }

    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IssueHistory(
        Guid issueId,
        string actorId,
        Guid correlationId,
        IssueHistoryAction action,
        string? oldValue = null,
        string? newValue = null)
    {
        if (issueId == Guid.Empty)
        {
            throw new ArgumentException(
                "La incidencia es obligatoria.",
                nameof(issueId)
            );
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            throw new ArgumentException(
                "El usuario que realiza la acción es obligatorio.",
                nameof(actorId)
            );
        }

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de correlación es obligatorio.",
                nameof(correlationId)
            );
        }

        Id = Guid.NewGuid();
        IssueId = issueId;
        ActorId = actorId;
        CorrelationId = correlationId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        CreatedAt = DateTime.UtcNow;
    }

    private IssueHistory()
    {
        // Constructor requerido por Entity Framework Core.
    }
}