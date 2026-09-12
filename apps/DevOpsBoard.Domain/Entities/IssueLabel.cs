namespace DevOpsBoard.Domain.Entities;

public class IssueLabel
{
    public Guid IssueId { get; private set; }

    public Guid LabelId { get; private set; }

    public IssueLabel(
        Guid issueId,
        Guid labelId)
    {
        if (issueId == Guid.Empty)
        {
            throw new ArgumentException(
                "La incidencia es obligatoria.",
                nameof(issueId)
            );
        }

        if (labelId == Guid.Empty)
        {
            throw new ArgumentException(
                "La etiqueta es obligatoria.",
                nameof(labelId)
            );
        }

        IssueId = issueId;
        LabelId = labelId;
    }

    private IssueLabel()
    {
        // Constructor requerido por Entity Framework Core.
    }
}