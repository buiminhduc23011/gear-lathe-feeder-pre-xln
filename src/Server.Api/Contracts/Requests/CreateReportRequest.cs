namespace Server.Api.Contracts.Requests;

public sealed class CreateReportRequest
{
    public string MachineCode { get; init; } = string.Empty;

    public DateOnly ReportDate { get; init; }

    public int TotalJobs { get; init; }

    public int PassedJobs { get; init; }

    public int FailedJobs { get; init; }
}
