namespace Desktop.App.Models.Alarms;

public sealed class AlarmActiveItem
{
    public required string Key { get; init; }

    public required string TagName { get; init; }

    public required string Address { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Remedy { get; init; }

    public required AlarmType AlarmType { get; init; }

    public required AlarmSeverity Severity { get; init; }

    public bool StopMachine { get; init; }

    public int? CodeValue { get; init; }

    public required string RawValue { get; init; }

    public required DateTime StartedAtUtc { get; init; }
}

public sealed class AlarmBarState
{
    public bool HasActiveAlarm { get; init; }

    public int ActiveAlarmCount { get; init; }

    public string TickerText { get; init; } = "Máy hoạt động bình thường";

    public IReadOnlyList<AlarmActiveItem> Items { get; init; } = Array.Empty<AlarmActiveItem>();
}

public sealed class AlarmStateChangedEventArgs : EventArgs
{
    public AlarmStateChangedEventArgs(AlarmBarState state)
    {
        State = state;
    }

    public AlarmBarState State { get; }
}
