using Microsoft.EntityFrameworkCore;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Shared.Models.Reports;

namespace Server.Api.Services;

/// <summary>
/// Persistent implementation using SQL Server via EF Core (replaces previous in-memory List to prevent data loss on restarts).
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReportDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Reports
            .OrderByDescending(x => x.ReportDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(e => new ReportDto
        {
            MachineCode = e.MachineCode,
            ReportDate = e.ReportDate,
            TotalJobs = e.TotalJobs,
            PassedJobs = e.PassedJobs,
            FailedJobs = e.FailedJobs,
        }).ToArray();
    }

    public async Task<ReportDto> CreateAsync(
        string machineCode,
        DateOnly reportDate,
        int totalJobs,
        int passedJobs,
        int failedJobs,
        CancellationToken cancellationToken = default)
    {
        var entity = new ReportEntity
        {
            MachineCode = machineCode,
            ReportDate = reportDate,
            TotalJobs = totalJobs,
            PassedJobs = passedJobs,
            FailedJobs = failedJobs,
        };

        _dbContext.Reports.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ReportDto
        {
            MachineCode = entity.MachineCode,
            ReportDate = entity.ReportDate,
            TotalJobs = entity.TotalJobs,
            PassedJobs = entity.PassedJobs,
            FailedJobs = entity.FailedJobs,
        };
    }
}
