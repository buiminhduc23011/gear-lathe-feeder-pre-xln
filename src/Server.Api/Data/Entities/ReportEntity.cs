namespace Server.Api.Data.Entities;

public sealed class ReportEntity
{
    public int Id { get; set; }

    public string MachineCode { get; set; } = string.Empty;

    public DateOnly ReportDate { get; set; }

    public int TotalJobs { get; set; }

    public int PassedJobs { get; set; }

    public int FailedJobs { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
