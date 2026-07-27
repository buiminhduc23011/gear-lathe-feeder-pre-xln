namespace Server.Api.Contracts.Responses;

public sealed class ShelfDeclarationEventResponse
{
    public long Id { get; set; }
    public int DeclarationId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset EventAtUtc { get; set; }
    public string ActorType { get; set; } = string.Empty;
    public string? ActorId { get; set; }
    public string? ActorName { get; set; }
    public int? MachineId { get; set; }
    public int? MachineSlotIndex { get; set; }
    public int? StagingSlotIndex { get; set; }
    public string? PayloadJson { get; set; }
}
