using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class IssueHistoryTests
{
    private static readonly Guid IssueId =
        Guid.NewGuid();

    private static readonly Guid CorrelationId =
        Guid.NewGuid();

    private const string ActorId =
        "actor-user-id";

    [Fact]
    public void Constructor_ShouldCreateHistoryEntry()
    {
        var history = new IssueHistory(
            IssueId,
            ActorId,
            CorrelationId,
            IssueHistoryAction.StatusChanged,
            "Todo",
            "InProgress"
        );

        Assert.NotEqual(
            Guid.Empty,
            history.Id
        );

        Assert.Equal(
            IssueId,
            history.IssueId
        );

        Assert.Equal(
            ActorId,
            history.ActorId
        );

        Assert.Equal(
            CorrelationId,
            history.CorrelationId
        );

        Assert.Equal(
            IssueHistoryAction.StatusChanged,
            history.Action
        );

        Assert.Equal(
            "Todo",
            history.OldValue
        );

        Assert.Equal(
            "InProgress",
            history.NewValue
        );
    }

    [Fact]
    public void Constructor_ShouldAllowNullValues()
    {
        var history = new IssueHistory(
            IssueId,
            ActorId,
            CorrelationId,
            IssueHistoryAction.Created
        );

        Assert.Null(
            history.OldValue
        );

        Assert.Null(
            history.NewValue
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyIssueId()
    {
        Assert.Throws<ArgumentException>(
            () => new IssueHistory(
                Guid.Empty,
                ActorId,
                CorrelationId,
                IssueHistoryAction.Created
            )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyActorId()
    {
        Assert.Throws<ArgumentException>(
            () => new IssueHistory(
                IssueId,
                "",
                CorrelationId,
                IssueHistoryAction.Created
            )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyCorrelationId()
    {
        Assert.Throws<ArgumentException>(
            () => new IssueHistory(
                IssueId,
                ActorId,
                Guid.Empty,
                IssueHistoryAction.Created
            )
        );
    }
}