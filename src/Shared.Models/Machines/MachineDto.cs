namespace Shared.Models.Machines;

public sealed class MachineDto
{
    public int MachineId { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public string MachineName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
