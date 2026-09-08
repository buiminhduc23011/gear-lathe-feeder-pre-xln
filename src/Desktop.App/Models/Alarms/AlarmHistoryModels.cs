namespace Desktop.App.Models.Alarms;

public sealed class AlarmHistoryItem
{
    public long Id { get; init; }

    public required string AlarmKey { get; init; }

    public required string TagName { get; init; }

    public required string Address { get; init; }

    public string? Title { get; init; }

    public required string Description { get; init; }

    public required string Remedy { get; init; }

    public required AlarmType AlarmType { get; init; }

    public required AlarmSeverity Severity { get; init; }

    public required AlarmRecordStatus Status { get; init; }

    public bool StopMachine { get; init; }

    public int? CodeValue { get; init; }

    public required string RawValue { get; init; }

    public required string MachineCode { get; init; }

    public required DateTime StartedAtUtc { get; init; }

    public DateTime? EndedAtUtc { get; init; }

    public int? DurationSeconds { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required DateTime UpdatedAtUtc { get; init; }
}

public sealed class AlarmHistoryQuery
{
    public string? SearchTag { get; init; }

    public AlarmRecordStatus? Status { get; init; }

    public AlarmType? Type { get; init; }

    public DateTime? FromLocalDate { get; init; }

    public DateTime? ToLocalDate { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 200;
}

public sealed class AlarmHistoryPageResult
{
    public IReadOnlyList<AlarmHistoryItem> Items { get; init; } = Array.Empty<AlarmHistoryItem>();

    public int TotalCount { get; init; }
}

public sealed class AlarmHistorySummary
{
    public int ActiveCount { get; init; }

    public int StopMachineActiveCount { get; init; }

    public int TodayCount { get; init; }
}
