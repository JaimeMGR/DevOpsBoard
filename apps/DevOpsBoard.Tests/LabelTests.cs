using DevOpsBoard.Domain.Entities;

namespace DevOpsBoard.Tests;

public class LabelTests
{
    private static readonly Guid ProjectId =
        Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldCreateLabel()
    {
        var label = new Label(
            ProjectId,
            " backend ",
            "#FF0000"
        );

        Assert.NotEqual(
            Guid.Empty,
            label.Id
        );

        Assert.Equal(
            ProjectId,
            label.ProjectId
        );

        Assert.Equal(
            "backend",
            label.Name
        );

        Assert.Equal(
            "#FF0000",
            label.Color
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyProject()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Label(
                    Guid.Empty,
                    "backend",
                    "#FF0000"
                )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyName()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Label(
                    ProjectId,
                    "   ",
                    "#FF0000"
                )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectNameLongerThan50Characters()
    {
        var name = new string(
            'a',
            51
        );

        Assert.Throws<ArgumentException>(
            () =>
                new Label(
                    ProjectId,
                    name,
                    "#FF0000"
                )
        );
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyColor()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new Label(
                    ProjectId,
                    "backend",
                    "   "
                )
        );
    }

    [Fact]
    public void Rename_ShouldTrimAndChangeName()
    {
        var label = new Label(
            ProjectId,
            "backend",
            "#FF0000"
        );

        label.Rename(
            " security "
        );

        Assert.Equal(
            "security",
            label.Name
        );
    }

    [Fact]
    public void Rename_ShouldRejectEmptyName()
    {
        var label = new Label(
            ProjectId,
            "backend",
            "#FF0000"
        );

        Assert.Throws<ArgumentException>(
            () =>
                label.Rename("   ")
        );
    }

    [Fact]
    public void ChangeColor_ShouldTrimAndChangeColor()
    {
        var label = new Label(
            ProjectId,
            "backend",
            "#FF0000"
        );

        label.ChangeColor(
            " #00FF00 "
        );

        Assert.Equal(
            "#00FF00",
            label.Color
        );
    }
}