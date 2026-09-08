using Desktop.App.Configuration;

namespace Desktop.App.Services.Abstractions;

public interface ISettingsService
{
    Task<AppOptions> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppOptions options, CancellationToken cancellationToken = default);
}
