namespace Server.Api.Contracts.Responses;

public sealed class MachineResponse
{
    public int MachineId { get; init; }

    public string MachineCode { get; init; } = string.Empty;

    public string MachineName { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public string? Location { get; init; }

    public IReadOnlyList<int> StagingSlotIndices { get; init; } = Array.Empty<int>();

    public bool IsActive { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}
