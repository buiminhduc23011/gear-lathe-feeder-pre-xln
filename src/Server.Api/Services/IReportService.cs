using Shared.Models.Reports;

namespace Server.Api.Services;

public interface IReportService
{
    Task<IReadOnlyList<ReportDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ReportDto> CreateAsync(
        string machineCode,
        DateOnly reportDate,
        int totalJobs,
        int passedJobs,
        int failedJobs,
        CancellationToken cancellationToken = default);
}
