namespace Desktop.App.Models.Alarms;

public sealed class AlarmDefinition
{
    public required string Key { get; init; }

    public required string TagName { get; init; }

    public required string Address { get; init; }

    public required AlarmSourceType SourceType { get; init; }

    public required AlarmType AlarmType { get; init; }

    public required AlarmSeverity Severity { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public string? Remedy { get; init; }

    public bool StopMachine { get; init; }

    public bool ActiveWhenTrue { get; init; } = true;

    public int? CodeValue { get; init; }

    public bool MatchAnyPositiveCode { get; init; }

    /// <summary>
    /// Khi true, alarm này sẽ không được log vào DB hoặc hiển thị trên UI.
    /// </summary>
    public bool Suppress { get; init; }
}
