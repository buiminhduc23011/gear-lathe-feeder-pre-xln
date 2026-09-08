namespace Server.Api.Infrastructure;

public static class ShelfDeclarationModes
{
    public const string Agv = "Agv";
    public const string ManualLoad = "ManualLoad";
}

public static class ShelfDeclarationStatuses
{
    public const string Created = "Created";
    public const string AgvTaken = "AgvTaken";
    public const string Loaded = "Loaded";
    public const string InProduction = "InProduction";
    public const string Completed = "Completed";
    public const string Cleared = "Cleared";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlySet<string> TerminalStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Completed,
        Cleared,
        Cancelled
    };
}

public static class ShelfDeclarationEventTypes
{
    public const string Created = "Created";
    public const string LoadRequested = "LoadRequested";
    public const string AgvPicked = "AgvPicked";
    public const string Loaded = "Loaded";
    public const string InProduction = "InProduction";
    public const string OrderCompleted = "OrderCompleted";
    public const string Completed = "Completed";
    public const string Cleared = "Cleared";
    public const string Cancelled = "Cancelled";
}

public static class ShelfDeclarationActorTypes
{
    public const string User = "User";
    public const string Agv = "AGV";
    public const string Desktop = "Desktop";
    public const string System = "System";
}
