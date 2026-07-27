namespace Server.Api.Contracts.Responses;

public sealed class AgvCallEligibilityResponse
{
    public int? MachineId { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string? MachineName { get; set; }
    public int MachineSlotIndex { get; set; }
    public int? StagingSlotIndex { get; set; }
    public bool HasActiveDeclaration { get; set; }
    public int? DeclarationId { get; set; }
    public string? DeclarationStatus { get; set; }
    public int? OrderCount { get; set; }
    public string ReasonCode { get; set; } = "Unknown";
    public string ReasonMessage { get; set; } = string.Empty;
}