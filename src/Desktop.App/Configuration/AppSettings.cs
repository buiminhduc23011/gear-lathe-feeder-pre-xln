namespace Desktop.App.Configuration;

public static class AppSettings
{
    public static AppOptions Current { get; private set; } = new();

    public static event EventHandler<AppOptions>? CurrentChanged;

    public static void Initialize(AppOptions? options = null)
    {
        SetCurrent(options ?? new AppOptions());
    }

    public static void Update(AppOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        SetCurrent(options);
    }

    private static void SetCurrent(AppOptions options)
    {
        Current = options.Clone();
        CurrentChanged?.Invoke(null, Current);
    }
}
