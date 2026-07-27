namespace Server.Api.Contracts.Responses;

public sealed class ShelfDeclarationResponse
{
    public int Id { get; set; }
    public int MachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public int? StagingSlotIndex { get; set; }
    public int? MachineSlotIndex { get; set; }
    public int ShelfLayoutType { get; set; }
    public string ShelfLayoutName { get; set; } = string.Empty;
    public string OrdersJson { get; set; } = "[]";
    public int OrderCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? PickedByAgvId { get; set; }
    public string? PickedByAgvName { get; set; }
    public DateTimeOffset? AgvTakenAtUtc { get; set; }
    public DateTimeOffset? LoadRequestedAtUtc { get; set; }
    public DateTimeOffset? LoadedAtUtc { get; set; }
    public DateTimeOffset? ProductionStartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? ClearedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancelledByUsername { get; set; }
    public string? ClearedByUsername { get; set; }
    public int? ProductionDurationSeconds { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
