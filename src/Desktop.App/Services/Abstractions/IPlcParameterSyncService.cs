using Desktop.App.Models.Runtime;

namespace Desktop.App.Services.Abstractions;

public interface IPlcParameterSyncService : IDisposable, IAsyncDisposable
{
    event EventHandler<PlcParameterSyncChangedEventArgs>? SyncStatesChanged;

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    bool IsTagSynced(string tagName);

    Task SetDesiredValueAsync(string tagName, object typedValue, CancellationToken cancellationToken = default);
}
