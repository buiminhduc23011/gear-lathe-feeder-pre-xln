using Desktop.App.Configuration;
using Desktop.App.Data.Repositories;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly SettingsRepository _settingsRepository;

    public SettingsService(SettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public Task<AppOptions> LoadAsync(CancellationToken cancellationToken = default)
    {
        return _settingsRepository.GetLatestAsync(cancellationToken);
    }

    public Task SaveAsync(AppOptions options, CancellationToken cancellationToken = default)
    {
        return _settingsRepository.SaveAsync(options, cancellationToken);
    }
}
