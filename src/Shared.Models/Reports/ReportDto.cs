namespace Shared.Models.Reports;

public sealed class ReportDto
{
    public string MachineCode { get; set; } = string.Empty;

    public DateOnly ReportDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public int TotalJobs { get; set; }

    public int PassedJobs { get; set; }

    public int FailedJobs { get; set; }
}
