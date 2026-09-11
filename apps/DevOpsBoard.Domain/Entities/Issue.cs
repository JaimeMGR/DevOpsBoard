using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Domain.Entities;

public class Issue
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public IssueStatus Status { get; private set; }

    public IssuePriority Priority { get; private set; }

    public string ReporterId { get; private set; } = string.Empty;

    public string? AssigneeId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Issue(
        Guid projectId,
        string title,
        string reporterId,
        string? description = null,
        IssuePriority priority = IssuePriority.Medium)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "El proyecto es obligatorio.",
                nameof(projectId)
            );
        }

        var normalizedTitle = title?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ArgumentException(
                "El título de la incidencia es obligatorio.",
                nameof(title)
            );
        }

        if (normalizedTitle.Length > 200)
        {
            throw new ArgumentException(
                "El título de la incidencia no puede superar los 200 caracteres.",
                nameof(title)
            );
        }

        var normalizedReporterId = reporterId?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedReporterId))
        {
            throw new ArgumentException(
                "El creador de la incidencia es obligatorio.",
                nameof(reporterId)
            );
        }

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = normalizedTitle;
        Description = description?.Trim();
        Status = IssueStatus.Todo;
        Priority = priority;
        ReporterId = normalizedReporterId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Update(
        string title,
        string? description)
    {
        var normalizedTitle = title?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            throw new ArgumentException(
                "El título de la incidencia es obligatorio.",
                nameof(title)
            );
        }

        if (normalizedTitle.Length > 200)
        {
            throw new ArgumentException(
                "El título de la incidencia no puede superar los 200 caracteres.",
                nameof(title)
            );
        }

        Title = normalizedTitle;
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(
        IssueStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePriority(
        IssuePriority priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(
        string userId)
    {
        var normalizedUserId = userId?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedUserId))
        {
            throw new ArgumentException(
                "El usuario asignado es obligatorio.",
                nameof(userId)
            );
        }

        AssigneeId = normalizedUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unassign()
    {
        AssigneeId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private Issue()
    {
        // Constructor requerido por Entity Framework Core.
    }
}