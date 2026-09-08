namespace Server.Api.Contracts.Responses;

public sealed class AgvSlotStatusResponse
{
    public int SlotIndex { get; set; }
    public bool IsOccupied { get; set; }
    public int? DeclarationId { get; set; }
    public int? MachineId { get; set; }
    public string? MachineCode { get; set; }
    public string? MachineName { get; set; }
    public string? Status { get; set; }
    public int? ShelfLayoutType { get; set; }
    public string? ShelfLayoutName { get; set; }
    public int? OrderCount { get; set; }
    public DateTimeOffset? CreatedAtUtc { get; set; }
}
