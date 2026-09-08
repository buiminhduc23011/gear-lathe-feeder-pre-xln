namespace Shared.Models.ModelProfiles;

/// <summary>
/// Dùng cho cả Create và Update (shape giống nhau).
/// </summary>
public sealed class SaveModelProfileRequest
{
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
}
