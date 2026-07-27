using System.Globalization;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Alarms;
using Desktop.App.Models.Alarms;
using Desktop.App.Models.Runtime;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.Services;

public sealed class AlarmMonitorService : IAlarmMonitorService
{
    private static readonly TimeSpan DefaultPlcDisconnectStartupGracePeriod = TimeSpan.FromSeconds(20);

    private readonly IAlarmHistoryRepository _alarmHistoryRepository;
    private readonly Dictionary<string, ActiveAlarmState> _activeAlarms = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _plcDisconnectScheduleLock = new();
    private readonly object _stateLock = new();
    private readonly SemaphoreSlim _processingLock = new(1, 1);
    private readonly IPlcService _plcService;
    private readonly TimeSpan _plcDisconnectStartupGracePeriod;
    private readonly DateTime _startupUtc;
    private CancellationTokenSource? _pendingPlcDisconnectPersistenceCts;
    private bool _disposed;
    private bool _hasEstablishedConnection;
    private bool _initialized;

    public AlarmMonitorService(
        IPlcService plcService,
        IAlarmHistoryRepository alarmHistoryRepository,
        TimeSpan? plcDisconnectStartupGracePeriod = null)
    {
        _plcService = plcService;
        _alarmHistoryRepository = alarmHistoryRepository;
        _plcDisconnectStartupGracePeriod = plcDisconnectStartupGracePeriod ?? DefaultPlcDisconnectStartupGracePeriod;
        _startupUtc = DateTime.UtcNow;
    }

    public event EventHandler<AlarmStateChangedEventArgs>? StateChanged;

    public IReadOnlyList<AlarmActiveItem> GetActiveAlarms()
    {
        lock (_stateLock)
        {
            return OrderActiveItems(_activeAlarms.Values.Select(static state => state.Item)).ToList();
        }
    }

    public AlarmBarState GetAlarmBarState()
    {
        var items = GetActiveAlarms();
        return BuildAlarmBarState(items);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_initialized)
        {
            return;
        }

        AlarmDefinitionCatalog.Validate();

        await _alarmHistoryRepository.ResolveAllActiveAsync(DateTime.UtcNow, cancellationToken);

        _plcService.ConnectionChanged += OnConnectionChanged;
        _plcService.DataUpdated += OnDataUpdated;
        _initialized = true;
        _hasEstablishedConnection = _plcService.IsConnected;

        if (!_plcService.IsConnected)
        {
            var disconnectDefinition = AlarmDefinitionCatalog.All.Single(
                static d => d.Key == AlarmDefinitionCatalog.PlcDisconnectedKey);
            await ActivatePlcDisconnectAlarmAsync(disconnectDefinition, cancellationToken);
        }

        PublishState();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _plcService.ConnectionChanged -= OnConnectionChanged;
        _plcService.DataUpdated -= OnDataUpdated;
        CancelPendingPlcDisconnectPersistence();
        _processingLock.Dispose();
    }

    private void OnConnectionChanged(object? sender, bool connected)
    {
        _ = ProcessConnectionStateAsync(connected);
    }

    private void OnDataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_disposed || e.Snapshot.Count == 0)
        {
            return;
        }

        _ = ProcessSnapshotAsync(e.Snapshot);
    }

    private async Task ProcessConnectionStateAsync(bool connected, CancellationToken cancellationToken = default)
    {
        await _processingLock.WaitAsync(cancellationToken);

        try
        {
            var disconnectDefinition = AlarmDefinitionCatalog.All.Single(
                definition => definition.Key == AlarmDefinitionCatalog.PlcDisconnectedKey);

            if (connected)
            {
                _hasEstablishedConnection = true;
                CancelPendingPlcDisconnectPersistence();
                await ResolveAsync(AlarmDefinitionCatalog.PlcDisconnectedKey, DateTime.UtcNow, cancellationToken);
            }
            else
            {
                await ActivatePlcDisconnectAlarmAsync(disconnectDefinition, cancellationToken);
            }

            PublishState();
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private async Task ProcessSnapshotAsync(IReadOnlyDictionary<string, object?> snapshot, CancellationToken cancellationToken = default)
    {
        await _processingLock.WaitAsync(cancellationToken);

        try
        {
            _hasEstablishedConnection = true;
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in AlarmDefinitionCatalog.All)
            {
                if (definition.SourceType == AlarmSourceType.System)
                {
                    continue;
                }

                if (definition.Suppress)
                {
                    continue;
                }

                foreach (var occurrence in EvaluateOccurrences(definition, snapshot))
                {
                    seenKeys.Add(occurrence.Item.Key);

                    if (!TryGetActiveState(occurrence.Item.Key, out var current) || current is null)
                    {
                        await ActivateAsync(occurrence.Item, cancellationToken);
                        continue;
                    }

                    if (!string.Equals(current.Item.RawValue, occurrence.Item.RawValue, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(current.Item.Description, occurrence.Item.Description, StringComparison.Ordinal)
                        || !string.Equals(current.Item.Remedy, occurrence.Item.Remedy, StringComparison.Ordinal))
                    {
                        await ResolveAsync(current.Item.Key, occurrence.NowUtc, cancellationToken);
                        await ActivateAsync(occurrence.Item, cancellationToken);
                    }
                }
            }

            List<string> activeKeys;
            lock (_stateLock)
            {
                activeKeys = _activeAlarms.Keys.ToList();
            }

            var keysToResolve = activeKeys
                .Where(key => !string.Equals(key, AlarmDefinitionCatalog.PlcDisconnectedKey, StringComparison.OrdinalIgnoreCase))
                .Where(key => !seenKeys.Contains(key))
                .ToList();

            foreach (var key in keysToResolve)
            {
                await ResolveAsync(key, DateTime.UtcNow, cancellationToken);
            }

            PublishState();
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private IEnumerable<EvaluatedAlarmOccurrence> EvaluateOccurrences(
        AlarmDefinition definition,
        IReadOnlyDictionary<string, object?> snapshot)
    {
        var nowUtc = DateTime.UtcNow;

        switch (definition.SourceType)
        {
            case AlarmSourceType.Bit:
            {
                var boolValue = ReadBoolean(definition.TagName, snapshot);
                if (boolValue == definition.ActiveWhenTrue)
                {
                    yield return new EvaluatedAlarmOccurrence(CreateAlarmActiveItem(definition, null, boolValue.ToString(), nowUtc), nowUtc);
                }

                yield break;
            }
            case AlarmSourceType.CodeWord:
            {
                var codeValue = ReadPositiveInteger(definition.TagName, snapshot);
                if (codeValue is null)
                {
                    yield break;
                }

                if (definition.MatchAnyPositiveCode || definition.CodeValue == codeValue)
                {
                    yield return new EvaluatedAlarmOccurrence(CreateAlarmActiveItem(definition, codeValue, codeValue.Value.ToString(CultureInfo.InvariantCulture), nowUtc), nowUtc);
                }

                yield break;
            }
            default:
                yield break;
        }
    }

    private AlarmActiveItem CreateAlarmActiveItem(AlarmDefinition definition, int? codeValue, string rawValue, DateTime startedAtUtc)
    {
        var key = definition.MatchAnyPositiveCode && codeValue is not null
            ? $"{definition.Key}:{codeValue.Value}"
            : definition.Key;

        var title = definition.MatchAnyPositiveCode && codeValue is not null
            ? $"{definition.Title} #{codeValue.Value}"
            : definition.Title;

        var description = definition.MatchAnyPositiveCode && codeValue is not null
            ? $"{AlarmDefinitionCatalog.NormalizeDescription(definition.Description)} (Ma: {codeValue.Value})"
            : AlarmDefinitionCatalog.NormalizeDescription(definition.Description);

        return new AlarmActiveItem
        {
            Key = key,
            TagName = definition.TagName,
            Address = definition.Address,
            Title = title,
            Description = description,
            Remedy = AlarmDefinitionCatalog.NormalizeRemedy(definition.Remedy),
            AlarmType = definition.AlarmType,
            Severity = definition.Severity,
            StopMachine = definition.StopMachine,
            CodeValue = codeValue,
            RawValue = rawValue,
            StartedAtUtc = startedAtUtc,
        };
    }

    private static AlarmActiveItem BuildSystemAlarm(AlarmDefinition definition, string rawValue, DateTime? startedAtUtc = null)
    {
        return new AlarmActiveItem
        {
            Key = definition.Key,
            TagName = definition.TagName,
            Address = definition.Address,
            Title = definition.Title,
            Description = AlarmDefinitionCatalog.NormalizeDescription(definition.Description),
            Remedy = AlarmDefinitionCatalog.NormalizeRemedy(definition.Remedy),
            AlarmType = definition.AlarmType,
            Severity = definition.Severity,
            StopMachine = definition.StopMachine,
            CodeValue = null,
            RawValue = rawValue,
            StartedAtUtc = startedAtUtc ?? DateTime.UtcNow,
        };
    }

    private async Task ActivateAsync(
        AlarmActiveItem item,
        CancellationToken cancellationToken,
        bool persistToHistory = true,
        DateTime? historyStartedAtUtc = null)
    {
        long? recordId = null;
        if (persistToHistory)
        {
            recordId = await _alarmHistoryRepository.InsertActiveAsync(
                CreateHistoryItem(item, historyStartedAtUtc ?? item.StartedAtUtc),
                cancellationToken);
        }

        lock (_stateLock)
        {
            _activeAlarms[item.Key] = new ActiveAlarmState(recordId, item);
        }
    }

    private async Task ResolveAsync(string key, DateTime endedAtUtc, CancellationToken cancellationToken)
    {
        ActiveAlarmState? removedState;
        lock (_stateLock)
        {
            if (!_activeAlarms.Remove(key, out removedState))
            {
                return;
            }
        }

        if (removedState?.IsPersisted == true)
        {
            await _alarmHistoryRepository.ResolveAsync(key, endedAtUtc, cancellationToken);
        }
    }

    private bool ReadBoolean(string tagName, IReadOnlyDictionary<string, object?> snapshot)
    {
        if (snapshot.TryGetValue(tagName, out var value) && value is bool boolValue)
        {
            return boolValue;
        }

        return _plcService.GetValue(tagName, false);
    }

    private int? ReadPositiveInteger(string tagName, IReadOnlyDictionary<string, object?> snapshot)
    {
        object? value = snapshot.TryGetValue(tagName, out var snapshotValue)
            ? snapshotValue
            : _plcService.GetValue<object?>(tagName, null);

        return value switch
        {
            short int16Value when int16Value > 0 => int16Value,
            int int32Value when int32Value > 0 => int32Value,
            long int64Value when int64Value > 0 && int64Value <= int.MaxValue => (int)int64Value,
            string stringValue when int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0 => parsed,
            _ => null,
        };
    }

    private void PublishState()
    {
        StateChanged?.Invoke(this, new AlarmStateChangedEventArgs(BuildAlarmBarState(GetActiveAlarms())));
    }

    private bool ContainsActiveKey(string key)
    {
        lock (_stateLock)
        {
            return _activeAlarms.ContainsKey(key);
        }
    }

    private bool TryGetActiveState(string key, out ActiveAlarmState? state)
    {
        lock (_stateLock)
        {
            if (_activeAlarms.TryGetValue(key, out var current))
            {
                state = current;
                return true;
            }
        }

        state = null;
        return false;
    }

    private async Task ActivatePlcDisconnectAlarmAsync(AlarmDefinition disconnectDefinition, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var deadlineUtc = GetPlcDisconnectGraceDeadlineUtc();
        var shouldDelayPersistence = nowUtc < deadlineUtc;

        if (!TryGetActiveState(AlarmDefinitionCatalog.PlcDisconnectedKey, out var existingState) || existingState is null)
        {
            await ActivateAsync(
                BuildSystemAlarm(disconnectDefinition, "DISCONNECTED", nowUtc),
                cancellationToken,
                persistToHistory: !shouldDelayPersistence);

            if (shouldDelayPersistence)
            {
                SchedulePendingPlcDisconnectPersistence();
            }

            return;
        }

        if (!existingState.IsPersisted)
        {
            if (shouldDelayPersistence)
            {
                SchedulePendingPlcDisconnectPersistence();
            }
            else
            {
                await PersistActiveAlarmAsync(existingState.Item.Key, deadlineUtc, cancellationToken);
            }
        }
    }

    private async Task PersistActiveAlarmAsync(string key, DateTime historyStartedAtUtc, CancellationToken cancellationToken)
    {
        if (!TryGetActiveState(key, out var existingState) || existingState is null || existingState.IsPersisted)
        {
            return;
        }

        var recordId = await _alarmHistoryRepository.InsertActiveAsync(
            CreateHistoryItem(existingState.Item, historyStartedAtUtc),
            cancellationToken);

        lock (_stateLock)
        {
            if (_activeAlarms.TryGetValue(key, out var currentState) && !currentState.IsPersisted)
            {
                _activeAlarms[key] = currentState with { RecordId = recordId };
            }
        }
    }

    private AlarmHistoryItem CreateHistoryItem(AlarmActiveItem item, DateTime startedAtUtc)
    {
        return new AlarmHistoryItem
        {
            AlarmKey = item.Key,
            TagName = item.TagName,
            Address = item.Address,
            Title = item.Title,
            Description = item.Description,
            Remedy = item.Remedy,
            AlarmType = item.AlarmType,
            Severity = item.Severity,
            Status = AlarmRecordStatus.Active,
            StopMachine = item.StopMachine,
            CodeValue = item.CodeValue,
            RawValue = item.RawValue,
            MachineCode = AppSettings.Current.MachineCode,
            StartedAtUtc = startedAtUtc,
            EndedAtUtc = null,
            DurationSeconds = null,
            CreatedAtUtc = startedAtUtc,
            UpdatedAtUtc = startedAtUtc,
        };
    }

    private void SchedulePendingPlcDisconnectPersistence()
    {
        if (_disposed)
        {
            return;
        }

        CancellationTokenSource cts;
        lock (_plcDisconnectScheduleLock)
        {
            if (_pendingPlcDisconnectPersistenceCts is not null)
            {
                return;
            }

            cts = new CancellationTokenSource();
            _pendingPlcDisconnectPersistenceCts = cts;
        }

        _ = PersistPlcDisconnectWhenGraceEndsAsync(cts);
    }

    private void CancelPendingPlcDisconnectPersistence()
    {
        CancellationTokenSource? cts;
        lock (_plcDisconnectScheduleLock)
        {
            cts = _pendingPlcDisconnectPersistenceCts;
            _pendingPlcDisconnectPersistenceCts = null;
        }

        if (cts is null)
        {
            return;
        }

        cts.Cancel();
        cts.Dispose();
    }

    private async Task PersistPlcDisconnectWhenGraceEndsAsync(CancellationTokenSource cts)
    {
        try
        {
            var remaining = GetPlcDisconnectGraceDeadlineUtc() - DateTime.UtcNow;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, cts.Token);
            }

            await _processingLock.WaitAsync(cts.Token);
            try
            {
                if (_disposed || _plcService.IsConnected)
                {
                    return;
                }

                if (TryGetActiveState(AlarmDefinitionCatalog.PlcDisconnectedKey, out var existingState)
                    && existingState is not null
                    && !existingState.IsPersisted)
                {
                    await PersistActiveAlarmAsync(existingState.Item.Key, GetPlcDisconnectGraceDeadlineUtc(), cts.Token);
                    PublishState();
                }
            }
            finally
            {
                _processingLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_plcDisconnectScheduleLock)
            {
                if (ReferenceEquals(_pendingPlcDisconnectPersistenceCts, cts))
                {
                    _pendingPlcDisconnectPersistenceCts = null;
                }
            }

            cts.Dispose();
        }
    }

    private DateTime GetPlcDisconnectGraceDeadlineUtc()
    {
        return _startupUtc + _plcDisconnectStartupGracePeriod;
    }

    private static AlarmBarState BuildAlarmBarState(IReadOnlyList<AlarmActiveItem> items)
    {
        return new AlarmBarState
        {
            HasActiveAlarm = items.Count > 0,
            ActiveAlarmCount = items.Count,
            TickerText = items.Count > 0
                ? string.Join("     |     ", items.Select(static item => $"[{item.Address}] {item.Title}"))
                : "Máy hoạt động bình thường",
            Items = items,
        };
    }

    private static IEnumerable<AlarmActiveItem> OrderActiveItems(IEnumerable<AlarmActiveItem> items)
    {
        return items
            .OrderByDescending(static item => item.Severity)
            .ThenBy(static item => item.AlarmType)
            .ThenBy(static item => item.StartedAtUtc);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private sealed record ActiveAlarmState(long? RecordId, AlarmActiveItem Item)
    {
        public bool IsPersisted => RecordId.HasValue;
    }

    private sealed record EvaluatedAlarmOccurrence(AlarmActiveItem Item, DateTime NowUtc);
}
