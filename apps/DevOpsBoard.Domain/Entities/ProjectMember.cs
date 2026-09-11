using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Domain.Entities;

public class ProjectMember
{
    public Guid ProjectId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public ProjectRole Role { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public ProjectMember(
        Guid projectId,
        string userId,
        ProjectRole role = ProjectRole.Developer)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException(
                "El proyecto es obligatorio.",
                nameof(projectId)
            );
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "El usuario es obligatorio.",
                nameof(userId)
            );
        }

        ProjectId = projectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    public void ChangeRole(ProjectRole role)
    {
        Role = role;
    }

    private ProjectMember()
    {
        // Constructor requerido por Entity Framework Core.
    }
}