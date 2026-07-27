using Desktop.App.Models.Runtime;

namespace Desktop.App.Services.Abstractions;

public interface IPlcService : IDisposable, IAsyncDisposable
{
    bool IsConnected { get; }

    /// <summary>Thời gian giữa 2 lần bắt đầu quét PLC liên tiếp (ms). -1 nếu chưa đủ 2 vòng.</summary>
    int LastScanElapsedMs { get; }

    event EventHandler<bool>? ConnectionChanged;

    event EventHandler<PlcDataChangedEventArgs>? DataUpdated;

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, object?>> ReadAllAsync(CancellationToken cancellationToken = default);

    Task WriteAsync(string tagName, object? value, CancellationToken cancellationToken = default);

    T GetValue<T>(string tagName, T fallback = default!);
}
