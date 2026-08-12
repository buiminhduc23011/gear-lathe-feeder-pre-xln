using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class CreateShelfDeclarationRequest
{
    [Required]
    public string Mode { get; set; } = string.Empty;

    [Range(1, 4)]
    public int? StagingSlotIndex { get; set; }

    [Range(1, 1)]
    public int? MachineSlotIndex { get; set; }

    [Range(0, 0)]
    public int ShelfLayoutType { get; set; }

    [Required]
    [MinLength(1)]
    public List<ShelfOrderItem> Orders { get; set; } = [];
}

public sealed class ShelfOrderItem
{
    [Required]
    [StringLength(30)]
    public string? OrderId { get; set; }

    [Required]
    public string ModelName { get; set; } = string.Empty;

    public string? ReportModelName { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; }

    [Required]
    [Range(1, 4)]
    public int? CartPositionIndex { get; set; }

    [Range(1, 4)]
    public int JigType { get; set; }

    public float? InputThickness { get; set; }

    public float? JigHeightMm { get; set; }

    public int? JigCapacity { get; set; }
}
