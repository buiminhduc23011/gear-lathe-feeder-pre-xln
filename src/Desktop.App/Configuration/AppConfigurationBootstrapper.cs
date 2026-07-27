using Desktop.App.Data.Sqlite;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.Configuration;

public static class AppConfigurationBootstrapper
{
    public static async Task<AppOptions> InitializeAsync(
        ISettingsService settingsService,
        string? databasePath = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        await Migrations.ApplyAsync(databasePath, cancellationToken);

        var options = await settingsService.LoadAsync(cancellationToken);
        AppSettings.Initialize(options);

        return AppSettings.Current;
    }
}
