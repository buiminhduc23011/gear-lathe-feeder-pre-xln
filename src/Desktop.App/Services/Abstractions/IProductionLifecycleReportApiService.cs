using Desktop.App.Models.Reports;

namespace Desktop.App.Services.Abstractions;

public interface IProductionLifecycleReportApiService
{
    Task<ProductionLifecycleReportDto?> GetReportAsync(
        string machineCode,
        string? status,
        int? shelfIndex,
        CancellationToken cancellationToken = default);
}
