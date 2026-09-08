namespace Server.Api.Contracts.Responses;

public sealed class ShelfDeclarationSlotStatusResponse
{
    public int SlotIndex { get; set; }
    public bool IsOccupied { get; set; }
    public int? DeclarationId { get; set; }
    public string? Status { get; set; }
    public string? CreatedByUsername { get; set; }
    public DateTimeOffset? CreatedAtUtc { get; set; }
    public int? ShelfLayoutType { get; set; }
    public string? ShelfLayoutName { get; set; }
    public int? OrderCount { get; set; }
}
