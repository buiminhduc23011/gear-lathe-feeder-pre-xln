namespace Server.Api.Infrastructure;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Technician = "Technician";
    public const string Operator = "Operator";
    public const string Viewer = "Viewer";
    public const string ShelfDeclarationUsers = Admin + "," + Technician + "," + Operator;

    private static readonly HashSet<string> SupportedRoles =
    [
        Admin,
        Technician,
        Operator,
        Viewer
    ];

    public static bool IsSupported(string? role)
    {
        return !string.IsNullOrWhiteSpace(role)
            && SupportedRoles.Contains(role.Trim(), StringComparer.Ordinal);
    }
}
