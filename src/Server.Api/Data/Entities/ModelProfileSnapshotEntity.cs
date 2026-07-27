namespace Server.Api.Data.Entities;

public sealed class ModelProfileSnapshotEntity
{
    public long Id { get; set; }

    public int ModelProfileId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string? ItemType { get; set; }

    public int? MachiningProgram { get; set; }

    public string? Spare1 { get; set; }

    public string? Spare2 { get; set; }

    public decimal? OuterShaftDiameter { get; set; }

    public float? DiameterOp1 { get; set; }

    public float? DiameterOp2 { get; set; }

    public int? TrayUsage { get; set; }

    public int? TrayType { get; set; }

    public int? OrderInput { get; set; }

    public string RobotData { get; set; } = "{}";

    public string Line1Data { get; set; } = "{}";

    public string Line2Data { get; set; } = "{}";

    /// <summary>"Created", "Updated", "Deleted", "Enabled", or "Disabled".</summary>
    public string ChangeAction { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public string PerformedByUsername { get; set; } = string.Empty;

    public DateTimeOffset PerformedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ModelProfileEntity ModelProfile { get; set; } = null!;
}
