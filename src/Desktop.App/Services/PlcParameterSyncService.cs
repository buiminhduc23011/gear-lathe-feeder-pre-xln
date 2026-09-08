using System.Collections.Concurrent;
using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Runtime;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Plc;

namespace Desktop.App.Services;

public sealed class PlcParameterSyncService : IPlcParameterSyncService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromSeconds(5);

    private readonly IPlcService _plcService;
    private readonly IPlcParameterSettingsService _settingsService;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly ConcurrentDictionary<string, object> _desiredValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, bool> _syncStates = new(StringComparer.OrdinalIgnoreCase);

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private bool _disposed;

    public PlcParameterSyncService(IPlcService plcService, IPlcParameterSettingsService settingsService)
    {
        _plcService = plcService ?? throw new ArgumentNullException(nameof(plcService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public event EventHandler<PlcParameterSyncChangedEventArgs>? SyncStatesChanged;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            if (_loopTask is not null && !_loopTask.IsCompleted)
            {
                return;
            }

            await LoadDesiredValuesAsync(cancellationToken);

            _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token), CancellationToken.None);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken);
        try
        {
            if (_loopCts is null)
            {
                return;
            }

            _loopCts.Cancel();

            if (_loopTask is not null)
            {
                try
                {
                    await _loopTask;
                }
                catch (OperationCanceledException)
                {
                }
            }

            _loopCts.Dispose();
            _loopCts = null;
            _loopTask = null;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public bool IsTagSynced(string tagName)
    {
        return _syncStates.TryGetValue(tagName, out var isSynced) && isSynced;
    }

    public async Task SetDesiredValueAsync(string tagName, object typedValue, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        _desiredValues[tagName] = typedValue;
        await _settingsService.UpsertAsync(tagName, typedValue, cancellationToken);

        if (_plcService.IsConnected)
        {
            try
            {
                await _plcService.WriteAsync(tagName, typedValue, cancellationToken);
            }
            catch
            {
                UpdateSyncState(tagName, false);
                throw;
            }
        }

        var isSynced = EvaluateSyncState(tagName, typedValue);
        if (UpdateSyncState(tagName, isSynced))
        {
            RaiseSyncStatesChanged(new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase) { [tagName] = isSynced });
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            await StopAsync();
        }
        finally
        {
            _syncLock.Dispose();
        }
    }

    private async Task LoadDesiredValuesAsync(CancellationToken cancellationToken)
    {
        _desiredValues.Clear();
        _syncStates.Clear();

        var snapshot = await _settingsService.LoadDesiredSnapshotAsync(cancellationToken);
        foreach (var entry in snapshot)
        {
            _desiredValues[entry.Key] = entry.Value;
            _syncStates[entry.Key] = EvaluateSyncState(entry.Key, entry.Value);
        }

        await EnsureMissingTagsSeededAsync(cancellationToken);

        RaiseSyncStatesChanged(_syncStates);
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await SyncOnceAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
            }

            await Task.Delay(SyncInterval, cancellationToken);
        }
    }

    private async Task SyncOnceAsync(CancellationToken cancellationToken)
    {
        await EnsureMissingTagsSeededAsync(cancellationToken);

        var changedStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in _desiredValues.ToArray())
        {
            if (!PlcTagCatalog.TryGet(entry.Key, out var definition))
            {
                continue;
            }

            var desiredValue = entry.Value;
            var isSynced = EvaluateSyncState(entry.Key, desiredValue);

            if (_plcService.IsConnected && !isSynced)
            {
                try
                {
                    await _plcService.WriteAsync(entry.Key, desiredValue, cancellationToken);
                    isSynced = EvaluateSyncState(entry.Key, desiredValue);
                }
                catch
                {
                    isSynced = false;
                }
            }

            if (UpdateSyncState(entry.Key, isSynced))
            {
                changedStates[entry.Key] = isSynced;
            }
        }

        if (changedStates.Count > 0)
        {
            RaiseSyncStatesChanged(changedStates);
        }
    }

    private async Task EnsureMissingTagsSeededAsync(CancellationToken cancellationToken)
    {
        if (!_plcService.IsConnected)
        {
            return;
        }

        foreach (var groupName in PlcParameterGroups.All)
        {
            foreach (var tag in PlcParameterGroups.GetTags(groupName))
            {
                if (_desiredValues.ContainsKey(tag.Name))
                {
                    continue;
                }

                var initialValue = _plcService.GetValue<object?>(tag.Name, null) ?? PlcTagValueTextConverter.CreateDefaultValue(tag);
                _desiredValues[tag.Name] = initialValue;
                _syncStates[tag.Name] = EvaluateSyncState(tag.Name, initialValue);
                await _settingsService.UpsertAsync(tag.Name, initialValue, cancellationToken);
            }
        }
    }

    private bool EvaluateSyncState(string tagName, object desiredValue)
    {
        if (!_plcService.IsConnected || !PlcTagCatalog.TryGet(tagName, out var definition))
        {
            return false;
        }

        var plcValue = _plcService.GetValue<object?>(tagName, null);
        return PlcTagValueTextConverter.AreEqual(definition, plcValue, desiredValue);
    }

    private bool UpdateSyncState(string tagName, bool isSynced)
    {
        if (_syncStates.TryGetValue(tagName, out var currentState) && currentState == isSynced)
        {
            return false;
        }

        _syncStates[tagName] = isSynced;
        return true;
    }

    private void RaiseSyncStatesChanged(IReadOnlyDictionary<string, bool> syncStates)
    {
        SyncStatesChanged?.Invoke(this, new PlcParameterSyncChangedEventArgs(syncStates));
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
