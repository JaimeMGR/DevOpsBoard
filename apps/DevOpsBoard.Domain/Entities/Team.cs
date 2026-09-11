namespace DevOpsBoard.Domain.Entities;

public class Team
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Team(
        string name,
        string? description,
        string createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del equipo es obligatorio.",
                nameof(name)
            );
        }

        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            throw new ArgumentException(
                "El usuario creador es obligatorio.",
                nameof(createdByUserId)
            );
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim();
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    private Team()
    {
        // Constructor requerido por Entity Framework Core.
    }
}