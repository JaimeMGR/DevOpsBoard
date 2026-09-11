namespace DevOpsBoard.Domain.Enums;

public enum IssueHistoryAction
{
    Created,
    TitleChanged,
    DescriptionChanged,
    StatusChanged,
    PriorityChanged,
    Assigned,
    Unassigned,
    Deleted
}