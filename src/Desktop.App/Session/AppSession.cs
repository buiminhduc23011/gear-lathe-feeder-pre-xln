using System.Windows;
using System.Windows.Threading;
using Desktop.App.Models.Ui;

namespace Desktop.App.Session;

public static class AppSession
{
    private static readonly TimeSpan PrivilegedSessionTimeout = TimeSpan.FromMinutes(20);
    private static Timer? _autoLogoutTimer;

    public static string? CurrentUserName { get; private set; }
    public static string? FullName { get; private set; }
    public static string? Role { get; private set; }
    public static string? AccessToken { get; private set; }
    public static DateTimeOffset? ExpiresAtUtc { get; private set; }
    public static DateTime? LoginTimeUtc { get; private set; }

    public static bool IsAuthenticated => !string.IsNullOrWhiteSpace(CurrentUserName);

    public static bool CanSaveModel =>
        IsAuthenticated &&
        (string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(Role, "Technician", StringComparison.OrdinalIgnoreCase));

    public static bool IsAdmin =>
        IsAuthenticated &&
        string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    public static bool CanEditSettings =>
        IsAuthenticated &&
        (string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(Role, "Technician", StringComparison.OrdinalIgnoreCase));

    public static event EventHandler? SessionChanged;

    public static void SetSession(LoginResult result)
    {
        CancelAutoLogoutTimer();

        CurrentUserName = result.Username;
        FullName = result.FullName;
        Role = result.Role;
        AccessToken = result.AccessToken;
        ExpiresAtUtc = result.ExpiresAtUtc;
        LoginTimeUtc = DateTime.UtcNow;

        if (IsPrivilegedRole(result.Role))
        {
            _autoLogoutTimer = new Timer(
                OnAutoLogoutTimerElapsed,
                state: null,
                dueTime: PrivilegedSessionTimeout,
                period: Timeout.InfiniteTimeSpan);
        }

        SessionChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void Logout()
    {
        CancelAutoLogoutTimer();

        CurrentUserName = null;
        FullName = null;
        Role = null;
        AccessToken = null;
        ExpiresAtUtc = null;
        LoginTimeUtc = null;

        SessionChanged?.Invoke(null, EventArgs.Empty);
    }

    private static bool IsPrivilegedRole(string? role)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "Technician", StringComparison.OrdinalIgnoreCase);
    }

    private static void CancelAutoLogoutTimer()
    {
        _autoLogoutTimer?.Dispose();
        _autoLogoutTimer = null;
    }

    private static void OnAutoLogoutTimerElapsed(object? state)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            Logout();
            return;
        }

        dispatcher.BeginInvoke(Logout, DispatcherPriority.Normal);
    }
}
