namespace Shared.Models.Machines;

public sealed class MachineSettingsDto
{
    public string MachineCode { get; set; } = string.Empty;

    public string PlcEndpoint { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
