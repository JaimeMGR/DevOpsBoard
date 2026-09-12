namespace DevOpsBoard.Domain.Entities;

public class Label
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Color { get; private set; } = string.Empty;

    public Label(
        Guid projectId,
        string name,
        string color)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "El proyecto es obligatorio.",
                nameof(projectId)
            );
        }

        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException(
                "El nombre de la etiqueta es obligatorio.",
                nameof(name)
            );
        }

        if (normalizedName.Length > 50)
        {
            throw new ArgumentException(
                "El nombre de la etiqueta no puede superar los 50 caracteres.",
                nameof(name)
            );
        }

        var normalizedColor = color.Trim();

        if (string.IsNullOrWhiteSpace(normalizedColor))
        {
            throw new ArgumentException(
                "El color de la etiqueta es obligatorio.",
                nameof(color)
            );
        }

        if (normalizedColor.Length > 20)
        {
            throw new ArgumentException(
                "El color de la etiqueta no puede superar los 20 caracteres.",
                nameof(color)
            );
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Name = normalizedName;
        Color = normalizedColor;
    }

    public void Rename(string name)
    {
        var normalizedName = name.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            throw new ArgumentException(
                "El nombre de la etiqueta es obligatorio.",
                nameof(name)
            );
        }

        if (normalizedName.Length > 50)
        {
            throw new ArgumentException(
                "El nombre de la etiqueta no puede superar los 50 caracteres.",
                nameof(name)
            );
        }

        Name = normalizedName;
    }

    public void ChangeColor(string color)
    {
        var normalizedColor = color.Trim();

        if (string.IsNullOrWhiteSpace(normalizedColor))
        {
            throw new ArgumentException(
                "El color de la etiqueta es obligatorio.",
                nameof(color)
            );
        }

        if (normalizedColor.Length > 20)
        {
            throw new ArgumentException(
                "El color de la etiqueta no puede superar los 20 caracteres.",
                nameof(color)
            );
        }

        Color = normalizedColor;
    }

    private Label()
    {
        // Constructor requerido por Entity Framework Core.
    }
}