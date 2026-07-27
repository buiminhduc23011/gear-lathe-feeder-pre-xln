namespace Server.Api.Data.Entities;

public sealed class ModelProfileEntity
{
    public int Id { get; set; }

    public int MachineId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string? ItemType { get; set; }

    public int? MachiningProgram { get; set; }

    public string? Spare1 { get; set; }

    public string? Spare2 { get; set; }

    public decimal? OuterShaftDiameter { get; set; }

    public float? DiameterOp1 { get; set; }

    public float? DiameterOp2 { get; set; }

    public int? TrayUsage { get; set; }

    /// <summary>0 = small tray, 1 = large tray.</summary>
    public int? TrayType { get; set; }

    /// <summary>0 = do not input order, 1 = input order.</summary>
    public int? OrderInput { get; set; }

    /// <summary>JSON object holding Robot tab parameter values.</summary>
    public string RobotData { get; set; } = "{}";

    /// <summary>JSON object holding Line 1 tab parameter values.</summary>
    public string Line1Data { get; set; } = "{}";

    /// <summary>JSON object holding Line 2 tab parameter values.</summary>
    public string Line2Data { get; set; } = "{}";

    public string? CreatedByUsername { get; set; }

    public string? UpdatedByUsername { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }

    public bool IsEnabled { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }

    public string? DeletedByUsername { get; set; }

    public MachineEntity Machine { get; set; } = null!;

    public ICollection<ModelProfileSnapshotEntity> Snapshots { get; set; } = new List<ModelProfileSnapshotEntity>();
}
