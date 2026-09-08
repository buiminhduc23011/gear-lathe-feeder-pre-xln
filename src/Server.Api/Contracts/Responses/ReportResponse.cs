namespace Server.Api.Contracts.Responses;

public sealed class ReportResponse
{
    public string MachineCode { get; init; } = string.Empty;

    public DateOnly ReportDate { get; init; }

    public int TotalJobs { get; init; }

    public int PassedJobs { get; init; }

    public int FailedJobs { get; init; }
}
