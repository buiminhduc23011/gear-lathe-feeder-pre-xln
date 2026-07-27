using Microsoft.AspNetCore.Mvc;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Services;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly IShelfDeclarationService _shelfDeclarationService;

    public ReportsController(IReportService reportService, IShelfDeclarationService shelfDeclarationService)
    {
        _reportService = reportService;
        _shelfDeclarationService = shelfDeclarationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReportResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReportResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var reports = await _reportService.GetAllAsync(cancellationToken);

        var response = reports
            .Select(x => new ReportResponse
            {
                MachineCode = x.MachineCode,
                ReportDate = x.ReportDate,
                TotalJobs = x.TotalJobs,
                PassedJobs = x.PassedJobs,
                FailedJobs = x.FailedJobs,
            })
            .ToArray();

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportResponse>> Create([FromBody] CreateReportRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MachineCode))
        {
            ModelState.AddModelError(nameof(request.MachineCode), "The machineCode field is required.");
            return ValidationProblem(ModelState);
        }

        var created = await _reportService.CreateAsync(
            request.MachineCode,
            request.ReportDate,
            request.TotalJobs,
            request.PassedJobs,
            request.FailedJobs,
            cancellationToken);

        var response = new ReportResponse
        {
            MachineCode = created.MachineCode,
            ReportDate = created.ReportDate,
            TotalJobs = created.TotalJobs,
            PassedJobs = created.PassedJobs,
            FailedJobs = created.FailedJobs,
        };

        return Ok(response);
    }

    [HttpGet("production-lifecycle")]
    [ProducesResponseType(typeof(ProductionLifecycleReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductionLifecycleReportResponse>> GetProductionLifecycle(
        [FromQuery] int? machineId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] string? status,
        [FromQuery] int? shelfIndex,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _shelfDeclarationService.GetProductionLifecycleReportAsync(
                machineId,
                fromUtc,
                toUtc,
                status,
                shelfIndex,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }
}
