namespace Desktop.App.Services;

/// <summary>
/// Singleton lightweight state holder for the virtual keyboard on/off toggle.
/// Updated immediately when the user toggles the setting — no app restart required.
/// </summary>
public sealed class VirtualKeyboardStateService
{
    private static readonly VirtualKeyboardStateService _instance = new();

    private VirtualKeyboardStateService() { }

    public static VirtualKeyboardStateService Instance => _instance;

    /// <summary>Whether the on-screen virtual keyboard is currently enabled.</summary>
    public bool IsEnabled { get; set; } = true;
}
