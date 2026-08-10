namespace Shared.Models.ModelProfiles;

public sealed class ModelProfileDto
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
    public float? InputBlankDiameter { get; set; }
    public float? Op2ChuckSleeveDepth { get; set; }
    public int? TrayUsage { get; set; }
    public int? TrayType { get; set; }
    public int? OrderInput { get; set; }
    public Dictionary<string, object?> RobotData { get; set; } = new();
    public Dictionary<string, object?> Line1Data { get; set; } = new();
    public Dictionary<string, object?> Line2Data { get; set; } = new();
    public string? CreatedByUsername { get; set; }
    public string? UpdatedByUsername { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedByUsername { get; set; }
}
