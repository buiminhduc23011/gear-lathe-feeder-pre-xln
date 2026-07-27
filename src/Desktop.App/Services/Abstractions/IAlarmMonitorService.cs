using Desktop.App.Models.Alarms;

namespace Desktop.App.Services.Abstractions;

public interface IAlarmMonitorService : IDisposable
{
    event EventHandler<AlarmStateChangedEventArgs>? StateChanged;

    IReadOnlyList<AlarmActiveItem> GetActiveAlarms();

    AlarmBarState GetAlarmBarState();

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
