using DevOpsBoard.Domain.Entities;
using DevOpsBoard.Domain.Enums;

namespace DevOpsBoard.Tests;

public class IssueTests
{
    private static readonly Guid ProjectId =
        Guid.NewGuid();

    [Fact]
    public void Delete_ShouldMarkIssueAsDeleted()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.Delete();

        Assert.True(issue.IsDeleted);
        Assert.NotNull(issue.DeletedAt);
    }

    [Fact]
    public void Delete_ShouldBeIdempotent()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.Delete();

        var deletedAt = issue.DeletedAt;

        issue.Delete();

        Assert.True(issue.IsDeleted);
        Assert.Equal(
            deletedAt,
            issue.DeletedAt
        );
    }

    private const string ReporterId =
        "reporter-user-id";

    [Fact]
    public void Constructor_ShouldCreateIssueWithDefaultValues()
    {
        var issue = new Issue(
            ProjectId,
            "Implement login",
            ReporterId
        );

        Assert.NotEqual(
            Guid.Empty,
            issue.Id
        );

        Assert.Equal(
            ProjectId,
            issue.ProjectId
        );

        Assert.Equal(
            "Implement login",
            issue.Title
        );

        Assert.Equal(
            ReporterId,
            issue.ReporterId
        );

        Assert.Null(
            issue.AssigneeId
        );

        Assert.Equal(
            IssueStatus.Todo,
            issue.Status
        );

        Assert.Equal(
            IssuePriority.Medium,
            issue.Priority
        );

        Assert.Equal(
            issue.CreatedAt,
            issue.UpdatedAt
        );
    }

    [Fact]
    public void Constructor_ShouldNormalizeTitleAndDescription()
    {
        var issue = new Issue(
            ProjectId,
            "  Login issue  ",
            ReporterId,
            "  Description  "
        );

        Assert.Equal(
            "Login issue",
            issue.Title
        );

        Assert.Equal(
            "Description",
            issue.Description
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyProjectId()
    {
        Assert.Throws<ArgumentException>(
            () => new Issue(
                Guid.Empty,
                "Issue",
                ReporterId
            )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyTitle()
    {
        Assert.Throws<ArgumentException>(
            () => new Issue(
                ProjectId,
                "",
                ReporterId
            )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectTitleOver200Characters()
    {
        var title = new string(
            'A',
            201
        );

        Assert.Throws<ArgumentException>(
            () => new Issue(
                ProjectId,
                title,
                ReporterId
            )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyReporter()
    {
        Assert.Throws<ArgumentException>(
            () => new Issue(
                ProjectId,
                "Issue",
                ""
            )
        );
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatus()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.ChangeStatus(
            IssueStatus.InProgress
        );

        Assert.Equal(
            IssueStatus.InProgress,
            issue.Status
        );
    }

    [Fact]
    public void ChangePriority_ShouldUpdatePriority()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.ChangePriority(
            IssuePriority.Critical
        );

        Assert.Equal(
            IssuePriority.Critical,
            issue.Priority
        );
    }

    [Fact]
    public void AssignTo_ShouldAssignUser()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.AssignTo(
            "assignee-user-id"
        );

        Assert.Equal(
            "assignee-user-id",
            issue.AssigneeId
        );
    }

    [Fact]
    public void AssignTo_ShouldRejectEmptyUser()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        Assert.Throws<ArgumentException>(
            () => issue.AssignTo("")
        );
    }

    [Fact]
    public void Unassign_ShouldRemoveAssignee()
    {
        var issue = new Issue(
            ProjectId,
            "Issue",
            ReporterId
        );

        issue.AssignTo(
            "assignee-user-id"
        );

        issue.Unassign();

        Assert.Null(
            issue.AssigneeId
        );
    }

    [Fact]
    public void Update_ShouldChangeTitleAndDescription()
    {
        var issue = new Issue(
            ProjectId,
            "Old title",
            ReporterId,
            "Old description"
        );

        issue.Update(
            "New title",
            "New description"
        );

        Assert.Equal(
            "New title",
            issue.Title
        );

        Assert.Equal(
            "New description",
            issue.Description
        );
    }
}