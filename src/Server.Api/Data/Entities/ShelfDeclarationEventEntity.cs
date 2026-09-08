namespace Server.Api.Data.Entities;

public sealed class ShelfDeclarationEventEntity
{
    public long Id { get; set; }

    public int DeclarationId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTimeOffset EventAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string ActorType { get; set; } = string.Empty;

    public string? ActorId { get; set; }

    public string? ActorName { get; set; }

    public int? MachineId { get; set; }

    public int? MachineSlotIndex { get; set; }

    public int? StagingSlotIndex { get; set; }

    public string? PayloadJson { get; set; }

    public ManualShelfDeclarationEntity Declaration { get; set; } = null!;
}
