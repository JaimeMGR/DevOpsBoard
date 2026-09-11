namespace DevOpsBoard.Domain.Entities;

public class Project
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Key { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public Project(
        string name,
        string key,
        string ownerId,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del proyecto es obligatorio.",
                nameof(name)
            );
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "La clave del proyecto es obligatoria.",
                nameof(key)
            );
        }

        if (key.Length > 10)
        {
            throw new ArgumentException(
                "La clave del proyecto no puede superar los 10 caracteres.",
                nameof(key)
            );
        }

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException(
                "El propietario del proyecto es obligatorio.",
                nameof(ownerId)
            );
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Key = key.Trim().ToUpperInvariant();
        Description = description?.Trim();
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
    }

    private Project()
    {
        // Constructor requerido por Entity Framework Core.
    }
}