using System.Collections.Concurrent;
using System.Diagnostics;
using DBI.Drivers.Delta.PLC.Converters;
using DBI.Drivers.Delta.PLC.Interfaces;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Runtime;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Plc;

namespace Desktop.App.Services;

public sealed class PlcService : IPlcService
{
    private readonly ConcurrentDictionary<string, object?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDeltaClientFactory _clientFactory;
    private readonly AppOptions _options;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private readonly IReadOnlyList<PlcTagDefinition> _tagCatalog;
    private readonly DeltaPlcMetadataSet _metadataSet;
    private readonly Func<IDeltaClientFactory, AppOptions, IDeltaClient> _clientCreator;
    private readonly int _pollIntervalMs;
    private readonly IReadOnlySet<string>? _writableTagNames;

    private CancellationTokenSource? _pollingCts;
    private Task? _pollingTask;
    private IDeltaClient? _client;
    private bool _isDisposed;
    private volatile bool _isConnected;
    private volatile int _lastScanElapsedMs = -1;

    private short _clockValue = 0;
    private System.DateTime _lastClockToggle = System.DateTime.MinValue;
    private readonly string? _clockTagName;

    /// <summary>
    /// Unified constructor for all PLC instances (Robot, Line 1, Line 2).
    /// All parameters are explicit — no special-casing based on constructor overloads.
    /// </summary>
    public PlcService(
        string lineName,
        IDeltaClientFactory clientFactory,
        AppOptions options,
        IReadOnlyList<PlcTagDefinition> tagCatalog,
        DeltaPlcMetadataSet metadataSet,
        Func<IDeltaClientFactory, AppOptions, IDeltaClient> clientCreator,
        int pollIntervalMs,
        IReadOnlySet<string>? writableTagNames = null)
    {
        LineName = lineName;
        _clientFactory = clientFactory;
        _options = options;
        _tagCatalog = tagCatalog;
        _metadataSet = metadataSet;
        _clientCreator = clientCreator;
        _pollIntervalMs = pollIntervalMs;
        _writableTagNames = writableTagNames;

        _clockTagName = tagCatalog.FirstOrDefault(t => 
            string.Equals(t.Name, "data.clock_1s", StringComparison.OrdinalIgnoreCase))?.Name;

        foreach (var tag in tagCatalog)
        {
            _cache[tag.Name] = CreateDefaultValue(tag.DataType);
        }

        Debug.WriteLine($"[PLC-{LineName}] Service created with {tagCatalog.Count} tags, poll interval {pollIntervalMs}ms");
    }

    /// <summary>Human-readable name for this PLC instance (e.g. "Robot", "Line 1", "Line 2").</summary>
    public string LineName { get; }

    public bool IsConnected => _isConnected;

    public int LastScanElapsedMs => _lastScanElapsedMs;

    public event EventHandler<bool>? ConnectionChanged;

    public event EventHandler<PlcDataChangedEventArgs>? DataUpdated;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _lifecycleLock.WaitAsync(cancellationToken);

        try
        {
            _client ??= _clientCreator(_clientFactory, _options);

            if (!_client.IsConnected)
            {
                try
                {
                    Debug.WriteLine($"[PLC-{LineName}] Connecting to PLC...");
                    await Task.Run(_client.Connect, cancellationToken);
                    Debug.WriteLine($"[PLC-{LineName}] Connect result: {_client.IsConnected}");
                }
                catch (Exception ex) when (_options.AutoReconnect)
                {
                    Debug.WriteLine($"[PLC-{LineName}] Initial connect failed (auto-reconnect enabled): {ex.Message}");
                    UpdateConnectionState(_client.IsConnected);
                }
            }

            EnsurePollingStarted();
            UpdateConnectionState(_client.IsConnected);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);

        try
        {
            await StopPollingAsync();

            if (_client is not null)
            {
                Debug.WriteLine($"[PLC-{LineName}] Disconnecting...");

                try
                {
                    // Driver Disconnect() is a blocking socket call — timeout guard.
                    await Task.Run(_client.Disconnect, cancellationToken)
                        .WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine($"[PLC-{LineName}] Disconnect timed out — force-disposing client.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[PLC-{LineName}] Disconnect error: {ex.Message}");
                }

                try { _client.Dispose(); } catch { }
                _client = null;
            }

            UpdateConnectionState(false);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task<IReadOnlyDictionary<string, object?>> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var client = _client ?? throw new InvalidOperationException("PLC client has not been initialized.");
        if (!client.IsConnected)
        {
            UpdateConnectionState(false);
            throw new InvalidOperationException("PLC is not connected.");
        }

        var snapshot = await Task.Run(() => ReadSnapshot(client), cancellationToken);
        ApplySnapshot(snapshot);
        UpdateConnectionState(client.IsConnected);

        return snapshot;
    }

    public async Task WriteAsync(string tagName, object? value, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        PlcTagDefinition? definition = null;

        _metadataSet.Bindings.TryGetValue(tagName, out var binding);
        definition = binding?.Tag;

        if (definition is null)
        {
            throw new ArgumentException($"Unknown PLC tag: {tagName}", nameof(tagName));
        }

        if (_writableTagNames is not null && !_writableTagNames.Contains(tagName))
        {
            throw new InvalidOperationException($"PLC tag '{tagName}' is read-only and cannot be written.");
        }

        var client = _client ?? throw new InvalidOperationException("PLC client has not been initialized.");
        if (!client.IsConnected)
        {
            UpdateConnectionState(false);
            throw new InvalidOperationException("PLC is not connected.");
        }

        var resolvedBinding = _metadataSet.GetRequiredBinding(tagName);
        var typedValue = NormalizeValue(definition, value);

        await Task.Run(() => WriteValue(client, resolvedBinding, typedValue), cancellationToken);
        ApplySnapshot(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { [tagName] = typedValue });
        UpdateConnectionState(client.IsConnected);
    }

    public T GetValue<T>(string tagName, T fallback = default!)
    {
        if (!_cache.TryGetValue(tagName, out var value) || value is null)
        {
            return fallback;
        }

        if (value is T typed)
        {
            return typed;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return fallback;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        try
        {
            await DisconnectAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PLC-{LineName}] Dispose error (suppressed): {ex.Message}");
        }
        finally
        {
            // Force-dispose client if DisconnectAsync left it alive
            if (_client is not null)
            {
                try { _client.Dispose(); } catch { }
                _client = null;
            }

            _lifecycleLock.Dispose();
        }
    }

    private void EnsurePollingStarted()
    {
        if (_pollingTask is not null && !_pollingTask.IsCompleted)
        {
            return;
        }

        _pollingCts = new CancellationTokenSource();
        _pollingTask = Task.Run(() => PollAsync(_pollingCts.Token));
    }

    private async Task StopPollingAsync()
    {
        if (_pollingCts is null)
        {
            return;
        }

        await _pollingCts.CancelAsync();

        if (_pollingTask is not null)
        {
            try
            {
                // Polling loop may be stuck in a blocking driver call (ReadD, Connect)
                // that doesn't respect cancellation — don't wait forever.
                await _pollingTask.WaitAsync(TimeSpan.FromSeconds(3));
            }
            catch (OperationCanceledException)
            {
            }
            catch (TimeoutException)
            {
                Debug.WriteLine($"[PLC-{LineName}] Timed out waiting for polling task to complete. Abandoning.");
            }
        }

        _pollingCts.Dispose();
        _pollingCts = null;
        _pollingTask = null;
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        var cycleTimer = new Stopwatch();

        while (!cancellationToken.IsCancellationRequested)
        {
            var cycleElapsed = cycleTimer.IsRunning ? (int)cycleTimer.ElapsedMilliseconds : -1;
            cycleTimer.Restart();

            try
            {
                var client = _client;
                if (client is null)
                {
                    // Client was destroyed — recreate and reconnect
                    Debug.WriteLine($"[PLC-{LineName}] No client, creating new one...");
                    await RecreateAndConnectAsync(cancellationToken);
                    await Task.Delay(_pollIntervalMs, cancellationToken);
                    continue;
                }

                if (cycleElapsed >= 0 && client.IsConnected)
                {
                    _lastScanElapsedMs = cycleElapsed;
                }

                var snapshot = ReadSnapshot(client);
                ApplySnapshot(snapshot);
                UpdateConnectionState(client.IsConnected);

                // Toggle Clock1s (D5640) every 1 second as heartbeat when app is running
                if (_clockTagName != null && client.IsConnected && 
                    (System.DateTime.UtcNow - _lastClockToggle).TotalSeconds >= 1.0)
                {
                    _clockValue = (short)(_clockValue == 0 ? 1 : 0);
                    _lastClockToggle = System.DateTime.UtcNow;
                    _ = WriteAsync(_clockTagName, _clockValue);  // fire-and-forget to not block poll loop
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Socket is dead (SocketException / IOException) — the DeltaClient
                // cannot recover from a destroyed TCP connection on its own.
                // We must dispose the old client, create a fresh one, and reconnect.
                Debug.WriteLine($"[PLC-{LineName}] Poll error: {ex.Message}. Disposing client and reconnecting...");
                UpdateConnectionState(false);

                try
                {
                    await RecreateAndConnectAsync(cancellationToken);
                }
                catch (Exception reconnectEx) when (!cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine($"[PLC-{LineName}] Reconnect failed: {reconnectEx.Message}. Will retry next cycle.");
                }
            }

            await Task.Delay(_pollIntervalMs, cancellationToken);
        }
    }

    /// <summary>
    /// Disposes the current (broken) client, creates a brand new DeltaClient via the factory,
    /// and connects it. This is the only way to recover from a dead TCP socket.
    /// </summary>
    private async Task RecreateAndConnectAsync(CancellationToken cancellationToken = default)
    {
        // Dispose old client safely
        if (_client is not null)
        {
            try { _client.Disconnect(); } catch { }
            try { _client.Dispose(); } catch { }
            _client = null;
        }

        // Create fresh client through the factory (sets AutoReconnect, ReconnectInterval, MaxRetry)
        _client = _clientCreator(_clientFactory, _options);
        Debug.WriteLine($"[PLC-{LineName}] New client created. Connecting...");

        try
        {
            await Task.Run(_client.Connect, cancellationToken);
            UpdateConnectionState(_client.IsConnected);
            Debug.WriteLine($"[PLC-{LineName}] Connected: {_client.IsConnected}");
        }
        catch (Exception ex) when (_options.AutoReconnect && !cancellationToken.IsCancellationRequested)
        {
            Debug.WriteLine($"[PLC-{LineName}] Connect attempt failed: {ex.Message}. Will retry on next poll cycle.");
            UpdateConnectionState(false);
        }
    }



    private Dictionary<string, object?> ReadSnapshot(IDeltaClient client)
    {
        var wordValues = ReadWordValues(client);
        var discreteValues = ReadDiscreteValues(client, wordValues);
        var snapshot = new Dictionary<string, object?>(_tagCatalog.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var definition in _tagCatalog)
        {
            var binding = _metadataSet.GetRequiredBinding(definition.Name);
            snapshot[definition.Name] = ReadValue(binding, wordValues, discreteValues);
        }

        ApplyPackedDiscreteOverrides(snapshot, wordValues, _tagCatalog);
        return snapshot;
    }

    private Dictionary<int, short> ReadWordValues(IDeltaClient client)
    {
        var values = new Dictionary<int, short>();
        var ranges = _metadataSet.GetRanges(DeltaPlcArea.DWord);

        foreach (var range in ranges)
        {
            var words = client.ReadD(range.StartAddress, range.Count);
            for (var index = 0; index < words.Length; index++)
            {
                values[range.StartAddress + index] = words[index];
            }
        }

        return values;
    }

    private Dictionary<(DeltaPlcArea Area, int Address), bool> ReadDiscreteValues(
        IDeltaClient client,
        IReadOnlyDictionary<int, short> wordValues)
    {
        var values = new Dictionary<(DeltaPlcArea Area, int Address), bool>();

        foreach (var area in new[] { DeltaPlcArea.M, DeltaPlcArea.X, DeltaPlcArea.Y })
        {
            foreach (var range in GetDiscreteReadRanges(area, wordValues))
            {
                var bits = area switch
                {
                    DeltaPlcArea.M => client.ReadM(range.StartAddress, range.Count),
                    DeltaPlcArea.X => client.ReadX(range.StartAddress, range.Count),
                    DeltaPlcArea.Y => client.ReadY(range.StartAddress, range.Count),
                    _ => throw new InvalidOperationException($"Unsupported PLC area '{area}'."),
                };

                for (var index = 0; index < bits.Length; index++)
                {
                    values[(area, range.StartAddress + index)] = bits[index];
                }
            }
        }

        return values;
    }

    private IReadOnlyList<DeltaPlcReadRange> GetDiscreteReadRanges(
        DeltaPlcArea area,
        IReadOnlyDictionary<int, short> wordValues)
    {
        var segments = _metadataSet.Bindings.Values
            .Where(binding => binding.Area == area)
            .Where(binding => binding.Tag.DataType == PlcTagDataType.Bool)
            .Where(binding => !CanReadPackedDiscreteBit(binding, wordValues))
            .Select(static binding => (Start: binding.StartAddress, End: binding.StartAddress + binding.Span - 1))
            .OrderBy(static segment => segment.Start)
            .ToList();

        return DeltaPlcMetadata.BuildRanges(area, segments);
    }

    private static object ReadValue(
        DeltaPlcTagBinding binding,
        IReadOnlyDictionary<int, short> wordValues,
        IReadOnlyDictionary<(DeltaPlcArea Area, int Address), bool> discreteValues)
    {
        return binding.Tag.DataType switch
        {
            PlcTagDataType.Bool => ReadBool(binding, wordValues, discreteValues),
            PlcTagDataType.Int16 => WordConverter.ToInt16(ToWord(wordValues, binding.StartAddress)),
            PlcTagDataType.Int32 => DoubleWordConverter.ToInt32(ToWord(wordValues, binding.StartAddress), ToWord(wordValues, binding.StartAddress + 1)),
            PlcTagDataType.Float => FloatConverter.ToFloat(ToWord(wordValues, binding.StartAddress), ToWord(wordValues, binding.StartAddress + 1)),
            PlcTagDataType.String => ReadString(binding, wordValues),
            _ => CreateDefaultValue(binding.Tag.DataType)!,
        };
    }

    private static bool ReadBool(
        DeltaPlcTagBinding binding,
        IReadOnlyDictionary<int, short> wordValues,
        IReadOnlyDictionary<(DeltaPlcArea Area, int Address), bool> discreteValues)
    {
        if (binding.Area == DeltaPlcArea.DWord)
        {
            var word = ToWord(wordValues, binding.StartAddress);
            return (word & (1 << binding.BitIndex!.Value)) != 0;
        }

        if (TryReadPackedDiscreteBit(binding, wordValues, out var packedValue))
        {
            return packedValue;
        }

        return discreteValues.TryGetValue((binding.Area, binding.StartAddress), out var value) && value;
    }

    private static bool TryReadPackedDiscreteBit(
        DeltaPlcTagBinding binding,
        IReadOnlyDictionary<int, short> wordValues,
        out bool value)
    {
        value = false;

        if (binding.Tag.DataType != PlcTagDataType.Bool
            || string.IsNullOrWhiteSpace(binding.Tag.Address)
            || !TryResolvePackedWordAddress(binding.Tag.Address, out var wordAddress, out var bitIndex))
        {
            return false;
        }

        if (!wordValues.TryGetValue(wordAddress, out var wordValue))
        {
            return false;
        }

        value = (unchecked((ushort)wordValue) & (1 << bitIndex)) != 0;
        return true;
    }

    private static bool CanReadPackedDiscreteBit(
        DeltaPlcTagBinding binding,
        IReadOnlyDictionary<int, short> wordValues)
    {
        return binding.Area is DeltaPlcArea.X or DeltaPlcArea.Y
            && binding.Tag.DataType == PlcTagDataType.Bool
            && !string.IsNullOrWhiteSpace(binding.Tag.Address)
            && TryResolvePackedWordAddress(binding.Tag.Address, out var wordAddress, out _)
            && wordValues.ContainsKey(wordAddress);
    }

    private static bool TryResolvePackedWordAddress(string address, out int wordAddress, out int bitIndex)
    {
        wordAddress = 0;
        bitIndex = 0;

        if (address.Length < 4)
        {
            return false;
        }

        var area = char.ToUpperInvariant(address[0]);
        if (area is not ('X' or 'Y'))
        {
            return false;
        }

        var addressParts = address[1..].Split('.', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (addressParts.Length != 2
            || !int.TryParse(addressParts[0], out var block)
            || !int.TryParse(addressParts[1], out bitIndex)
            || bitIndex is < 0 or > 15)
        {
            return false;
        }

        wordAddress = area switch
        {
            'X' => 5120 + block,
            'Y' => 5130 + block,
            _ => 0,
        };

        return true;
    }

    private static void ApplyPackedDiscreteOverrides(
        IDictionary<string, object?> snapshot,
        IReadOnlyDictionary<int, short> wordValues,
        IReadOnlyList<PlcTagDefinition> tags)
    {
        foreach (var definition in tags)
        {
            if (definition.DataType != PlcTagDataType.Bool
                || string.IsNullOrWhiteSpace(definition.Address)
                || !TryResolvePackedWordAddress(definition.Address, out var wordAddress, out var bitIndex)
                || !wordValues.TryGetValue(wordAddress, out var wordValue))
            {
                continue;
            }

            snapshot[definition.Name] = (unchecked((ushort)wordValue) & (1 << bitIndex)) != 0;
        }
    }

    private static string ReadString(DeltaPlcTagBinding binding, IReadOnlyDictionary<int, short> wordValues)
    {
        var maxLength = binding.Tag.Length ?? binding.Tag.Lenght ?? (binding.Span * 2);
        var chars = new List<char>(Math.Min(maxLength, binding.Span * 2));

        for (var index = 0; index < binding.Span; index++)
        {
            var word = ToWord(wordValues, binding.StartAddress + index);
            if (chars.Count < maxLength)
            {
                chars.Add((char)(word & 0x00FF));
            }

            if (chars.Count < maxLength)
            {
                chars.Add((char)((word >> 8) & 0x00FF));
            }
        }

        return new string(chars.ToArray()).TrimEnd('\0', ' ');
    }

    private static ushort ToWord(IReadOnlyDictionary<int, short> wordValues, int address)
    {
        if (!wordValues.TryGetValue(address, out var value))
        {
            throw new KeyNotFoundException($"PLC word address D{address} was not loaded.");
        }

        return unchecked((ushort)value);
    }

    private void WriteValue(IDeltaClient client, DeltaPlcTagBinding binding, object typedValue)
    {
        switch (binding.Tag.DataType)
        {
            case PlcTagDataType.Bool:
                WriteBool(client, binding, (bool)typedValue);
                break;
            case PlcTagDataType.Int16:
                client.WriteD(binding.StartAddress, [(short)typedValue]);
                break;
            case PlcTagDataType.Int32:
                client.WriteDInt(binding.StartAddress, (int)typedValue);
                break;
            case PlcTagDataType.Float:
                client.WriteFloat(binding.StartAddress, (float)typedValue);
                break;
            case PlcTagDataType.String:
                client.WriteD(binding.StartAddress, CreateStringWords(binding, (string)typedValue));
                break;
            default:
                throw new InvalidOperationException($"Unsupported PLC data type '{binding.Tag.DataType}'.");
        }
    }

    private static void WriteBool(IDeltaClient client, DeltaPlcTagBinding binding, bool value)
    {
        switch (binding.Area)
        {
            case DeltaPlcArea.M:
                client.WriteM(binding.StartAddress, [value]);
                return;
            case DeltaPlcArea.Y:
                client.WriteY(binding.StartAddress, [value]);
                return;
            case DeltaPlcArea.DWord:
            {
                var currentWord = unchecked((ushort)client.ReadD(binding.StartAddress, 1)[0]);
                var updatedWord = value
                    ? (short)(currentWord | (1u << binding.BitIndex!.Value))
                    : (short)(currentWord & ~(1u << binding.BitIndex!.Value));

                client.WriteD(binding.StartAddress, [updatedWord]);
                return;
            }
            default:
                throw new InvalidOperationException($"PLC area '{binding.Area}' does not support bool writes.");
        }
    }

    private static short[] CreateStringWords(DeltaPlcTagBinding binding, string value)
    {
        var maxLength = binding.Tag.Length ?? binding.Tag.Lenght ?? (binding.Span * 2);
        var normalized = value.Length > maxLength
            ? value[..maxLength]
            : value.PadRight(maxLength, ' ');
        var words = new short[binding.Span];

        for (var index = 0; index < words.Length; index++)
        {
            var lowCharIndex = index * 2;
            var highCharIndex = lowCharIndex + 1;
            var low = lowCharIndex < normalized.Length ? (byte)normalized[lowCharIndex] : (byte)' ';
            var high = highCharIndex < normalized.Length ? (byte)normalized[highCharIndex] : (byte)' ';
            words[index] = (short)(low | (high << 8));
        }

        return words;
    }

    private IReadOnlyDictionary<string, object?> ApplySnapshot(IReadOnlyDictionary<string, object?> snapshot)
    {
        var changed = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in snapshot)
        {
            if (_cache.TryGetValue(entry.Key, out var currentValue) && Equals(currentValue, entry.Value))
            {
                continue;
            }

            _cache[entry.Key] = entry.Value;
            changed[entry.Key] = entry.Value;
        }

        if (changed.Count > 0)
        {
            DataUpdated?.Invoke(this, new PlcDataChangedEventArgs(changed));
        }

        return snapshot;
    }

    private void UpdateConnectionState(bool isConnected)
    {
        if (_isConnected == isConnected)
        {
            return;
        }

        _isConnected = isConnected;
        Debug.WriteLine($"[PLC-{LineName}] Connection state changed: {(isConnected ? "CONNECTED" : "DISCONNECTED")}");
        ConnectionChanged?.Invoke(this, isConnected);
    }

    private static object NormalizeValue(PlcTagDefinition definition, object? value)
    {
        try
        {
            return definition.DataType switch
            {
                PlcTagDataType.Bool => NormalizeBool(value),
                PlcTagDataType.Int16 => Convert.ToInt16(value),
                PlcTagDataType.Int32 => Convert.ToInt32(value),
                PlcTagDataType.Float => Convert.ToSingle(value),
                PlcTagDataType.String => NormalizeString(definition, Convert.ToString(value) ?? string.Empty),
                _ => throw new InvalidOperationException($"Unsupported PLC data type '{definition.DataType}'."),
            };
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentNullException)
        {
            throw new InvalidOperationException(
                $"Value '{value ?? "<null>"}' is not valid for PLC tag '{definition.Name}' ({definition.DataType}).",
                exception);
        }
    }

    private static bool NormalizeBool(object? value)
    {
        return value switch
        {
            bool boolValue => boolValue,
            string stringValue when bool.TryParse(stringValue, out var boolValue) => boolValue,
            string stringValue when int.TryParse(stringValue, out var intValue) => intValue != 0,
            sbyte or byte or short or ushort or int or uint or long or ulong => Convert.ToInt64(value) != 0,
            _ => Convert.ToBoolean(value),
        };
    }

    private static string NormalizeString(PlcTagDefinition definition, string value)
    {
        var maxLength = definition.Length ?? definition.Lenght;
        if (maxLength is null || value.Length <= maxLength.Value)
        {
            return value;
        }

        return value[..maxLength.Value];
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    private static object? CreateDefaultValue(PlcTagDataType dataType)
    {
        return dataType switch
        {
            PlcTagDataType.Bool => false,
            PlcTagDataType.Int16 => (short)0,
            PlcTagDataType.Int32 => 0,
            PlcTagDataType.Float => 0f,
            PlcTagDataType.String => string.Empty,
            _ => null,
        };
    }
}
