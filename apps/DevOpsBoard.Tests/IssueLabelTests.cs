using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Tests;

public class IssueLabelTests
{
    private static readonly Guid IssueId =
        Guid.NewGuid();

    private static readonly Guid LabelId =
        Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldCreateIssueLabel()
    {
        var issueLabel = new IssueLabel(
            IssueId,
            LabelId
        );

        Assert.Equal(
            IssueId,
            issueLabel.IssueId
        );

        Assert.Equal(
            LabelId,
            issueLabel.LabelId
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyIssue()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new IssueLabel(
                    Guid.Empty,
                    LabelId
                )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyLabel()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new IssueLabel(
                    IssueId,
                    Guid.Empty
                )
        );
    }
}