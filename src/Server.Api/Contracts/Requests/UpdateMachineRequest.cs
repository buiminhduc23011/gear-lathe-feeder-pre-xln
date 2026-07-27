using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class UpdateMachineRequest
{
    [Required]
    public string MachineCode { get; init; } = string.Empty;

    [Required]
    public string MachineName { get; init; } = string.Empty;

    [Required]
    public string Manufacturer { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public string? Location { get; init; }

    [Required]
    public int[] StagingSlotIndices { get; init; } = Array.Empty<int>();

    public bool IsActive { get; init; } = true;
}
