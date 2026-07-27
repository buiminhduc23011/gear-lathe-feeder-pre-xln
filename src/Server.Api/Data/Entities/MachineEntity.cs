namespace Server.Api.Data.Entities;

public sealed class MachineEntity
{
    public int MachineId { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? Location { get; set; }

    public int? AssignedStagingSlot1 { get; set; }

    public int? AssignedStagingSlot2 { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
