namespace DevOpsBoard.Application.Security;

public static class RoleNames
{
    public const string Admin = "ADMIN";
    public const string Manager = "MANAGER";
    public const string Developer = "DEVELOPER";
    public const string Viewer = "VIEWER";

    public static readonly string[] All =
    [
        Admin,
        Manager,
        Developer,
        Viewer
    ];
}