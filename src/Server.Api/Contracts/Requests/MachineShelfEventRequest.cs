using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class MachineShelfEventRequest
{
    [Required]
    public string MachineCode { get; set; } = string.Empty;

    [Range(1, 2)]
    public int MachineSlotIndex { get; set; }

    [Required]
    public string EventType { get; set; } = string.Empty;

    public int? OrderSequence { get; set; }

    public string? OrderId { get; set; }

    /// <summary>Username của người thực hiện (nếu có đăng nhập). Dùng cho audit trail.</summary>
    public string? ActorUsername { get; set; }
}
