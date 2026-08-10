using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class AgvPickupRequest
{
    [Required]
    public string MachineCode { get; set; } = string.Empty;

    [Range(1, 1)]
    public int MachineSlotIndex { get; set; }

    [Range(1, 4)]
    public int? SlotIndex { get; set; }

    [Required]
    public string AgvId { get; set; } = string.Empty;

    [Required]
    public string AgvName { get; set; } = string.Empty;
}
