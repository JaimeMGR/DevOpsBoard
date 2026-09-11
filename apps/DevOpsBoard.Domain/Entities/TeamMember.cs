using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Domain.Entities;

public class TeamMember
{
    public Guid TeamId { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public TeamRole Role { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public TeamMember(
        Guid teamId,
        string userId,
        TeamRole role = TeamRole.Member)
    {
        if (teamId == Guid.Empty)
        {
            throw new ArgumentException(
                "El equipo es obligatorio.",
                nameof(teamId)
            );
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "El usuario es obligatorio.",
                nameof(userId)
            );
        }

        TeamId = teamId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }

    public void ChangeRole(TeamRole role)
    {
        Role = role;
    }

    private TeamMember()
    {
        // Constructor requerido por Entity Framework Core.
    }
}