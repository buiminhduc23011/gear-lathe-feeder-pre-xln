namespace Server.Api.Contracts.Responses;

public sealed class MachineLoadResponse
{
    public int DeclarationId { get; set; }
    public int MachineSlotIndex { get; set; }
    public int ShelfLayoutType { get; set; }
    public string OrdersJson { get; set; } = "[]";
    public int OrderCount { get; set; }
}
