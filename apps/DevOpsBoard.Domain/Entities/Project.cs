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
        var normalizedName = name?.Trim();
        var normalizedKey = key?.Trim().ToUpperInvariant();
        var normalizedOwnerId = ownerId?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException(
                "El nombre del proyecto es obligatorio.",
                nameof(name)
            );
        }

        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            throw new ArgumentException(
                "La clave del proyecto es obligatoria.",
                nameof(key)
            );
        }

        if (normalizedKey.Length > 10)
        {
            throw new ArgumentException(
                "La clave del proyecto no puede superar los 10 caracteres.",
                nameof(key)
            );
        }

        if (string.IsNullOrWhiteSpace(normalizedOwnerId))
        {
            throw new ArgumentException(
                "El propietario del proyecto es obligatorio.",
                nameof(ownerId)
            );
        }

        Id = Guid.NewGuid();
        Name = normalizedName;
        Key = normalizedKey;
        Description = description?.Trim();
        OwnerId = normalizedOwnerId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? description)
    {
        var normalizedName = name?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException(
                "El nombre del proyecto es obligatorio.",
                nameof(name)
            );
        }

        Name = normalizedName;
        Description = description?.Trim();
    }

    public void ChangeKey(string key)
    {
        var normalizedKey = key?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            throw new ArgumentException(
                "La clave del proyecto es obligatoria.",
                nameof(key)
            );
        }

        if (normalizedKey.Length > 10)
        {
            throw new ArgumentException(
                "La clave del proyecto no puede superar los 10 caracteres.",
                nameof(key)
            );
        }

        Key = normalizedKey;
    }

    private Project()
    {
        // Constructor requerido por Entity Framework Core.
    }
}