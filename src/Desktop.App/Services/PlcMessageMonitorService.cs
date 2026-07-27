using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Desktop.App.Configuration.Plc;
using Desktop.App.Services.Abstractions;
using Desktop.App.Models.Runtime;

namespace Desktop.App.Services;

public sealed class PlcMessageMonitorService : IPlcMessageMonitorService, IDisposable
{
    private readonly IPlcService _plcService;
    private readonly INotificationDialogService _dialogService;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeMessages = new(StringComparer.OrdinalIgnoreCase);
    private bool _initialized;
    private bool _disposed;

    private static readonly PlcTagDefinition[] MonitoredTags = [];

    public PlcMessageMonitorService(IPlcService plcService, INotificationDialogService dialogService)
    {
        _plcService = plcService ?? throw new ArgumentNullException(nameof(plcService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return Task.CompletedTask;
        }

        _plcService.DataUpdated += OnPlcDataUpdated;
        _initialized = true;

        return Task.CompletedTask;
    }

    private void OnPlcDataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_disposed)
        {
            return;
        }

        foreach (var tag in MonitoredTags)
        {
            if (!e.Snapshot.TryGetValue(tag.Name, out var signalObj) || signalObj is not bool isSignalHigh)
            {
                continue;
            }

            if (isSignalHigh)
            {
                if (!_activeMessages.ContainsKey(tag.Name))
                {
                    _ = HandleMessageActivationAsync(tag);
                }
            }
            else
            {
                if (_activeMessages.TryRemove(tag.Name, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
            }
        }
    }

    private async Task HandleMessageActivationAsync(PlcTagDefinition tag)
    {
        var cts = new CancellationTokenSource();
        if (!_activeMessages.TryAdd(tag.Name, cts))
        {
            cts.Dispose();
            return;
        }

        try
        {
            var confirmed = await _dialogService.ShowConfirmAsync("Thông báo từ máy", tag.Description, cts.Token);
            if (confirmed)
            {
                try
                {
                    await _plcService.WriteAsync(tag.Name, false);
                }
                catch
                {
                    // Ignore write errors to avoid crashing background task.
                }
            }
        }
        catch (TaskCanceledException)
        {
            // The message bit was cleared from PLC side
        }
        finally
        {
            // Need to remove from dictionary in case it was closed manually or threw unexpectedly.
            if (_activeMessages.TryRemove(tag.Name, out var actualCts))
            {
                actualCts.Dispose();
            }
            else
            {
                // Already removed by cancellation branch
                cts.Dispose();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _plcService.DataUpdated -= OnPlcDataUpdated;

        foreach (var cts in _activeMessages.Values)
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch
            {
                // Ignored
            }
        }

        _activeMessages.Clear();
    }
}
