using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data.Entities;
using Server.Api.Infrastructure;
using Server.Api.Services;

namespace Server.Api.Controllers;

[ApiController]
public sealed class ShelfDeclarationsController : ControllerBase
{
    private readonly IShelfDeclarationService _service;

    public ShelfDeclarationsController(IShelfDeclarationService service)
    {
        _service = service;
    }

    [HttpPost("api/machines/{machineId:int}/shelf-declarations")]
    [Authorize(Roles = AppRoles.ShelfDeclarationUsers)]
    [ProducesResponseType(typeof(ShelfDeclarationResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ShelfDeclarationResponse>> Create(
        int machineId,
        [FromBody] CreateShelfDeclarationRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
            var entity = await _service.CreateAsync(machineId, request, username, cancellationToken);
            return CreatedAtAction(nameof(GetByMachine), new { machineId }, MapResponse(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpGet("api/machines/{machineId:int}/shelf-declarations")]
    [Authorize(Roles = AppRoles.ShelfDeclarationUsers)]
    [ProducesResponseType(typeof(List<ShelfDeclarationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShelfDeclarationResponse>>> GetByMachine(
        int machineId,
        CancellationToken cancellationToken)
    {
        var entities = await _service.GetByMachineAsync(machineId, cancellationToken);
        return Ok(entities.Select(MapResponse).ToList());
    }

    [HttpGet("api/machines/{machineId:int}/shelf-declaration-slot-status")]
    [Authorize(Roles = AppRoles.ShelfDeclarationUsers)]
    [ProducesResponseType(typeof(List<ShelfDeclarationSlotStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShelfDeclarationSlotStatusResponse>>> GetSlotStatus(
        int machineId,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSlotStatusesAsync(machineId, cancellationToken));
    }

    [HttpGet("api/shelf-declarations/agv-slot-status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AgvSlotStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AgvSlotStatusResponse>>> GetAgvSlotStatus(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAgvSlotStatusesAsync(cancellationToken));
    }

    [HttpGet("api/shelf-declarations/agv-call-eligibility")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AgvCallEligibilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgvCallEligibilityResponse>> GetAgvCallEligibility(
        [FromQuery] string machineCode,
        [FromQuery] int machineSlotIndex,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineCode is required.");
        }

        if (machineSlotIndex is < 1 or > 2)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineSlotIndex must be between 1 and 2.");
        }

        try
        {
            return Ok(await _service.GetAgvCallEligibilityAsync(machineCode, machineSlotIndex, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpPost("api/shelf-declarations/agv-pickup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AgvPickupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgvPickupResponse>> PickupForAgv(
        [FromBody] AgvPickupRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var response = await _service.PickupForAgvAsync(request, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpGet("api/shelf-declarations/{id:int}/machine-load")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MachineLoadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineLoadResponse>> GetMachineLoad(
        int id,
        [FromQuery] string machineCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineCode is required.");
        }

        try
        {
            var response = await _service.GetMachineLoadAsync(id, machineCode, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpGet("api/shelf-declarations/manual-load/pending")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MachineLoadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineLoadResponse>> GetPendingManualLoad(
        [FromQuery] string machineCode,
        [FromQuery] int machineSlotIndex,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineCode is required.");
        }

        if (machineSlotIndex is < 1 or > 2)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineSlotIndex must be between 1 and 2.");
        }

        try
        {
            var response = await _service.GetPendingManualLoadAsync(machineCode, machineSlotIndex, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpGet("api/shelf-declarations/active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MachineLoadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineLoadResponse>> GetActiveLoad(
        [FromQuery] string machineCode,
        [FromQuery] int machineSlotIndex,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineCode is required.");
        }

        if (machineSlotIndex is < 1 or > 2)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineSlotIndex must be between 1 and 2.");
        }

        try
        {
            var response = await _service.GetActiveLoadAsync(machineCode, machineSlotIndex, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpPost("api/shelf-declarations/{id:int}/request-load")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Technician}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestLoad(int id, CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        var success = await _service.RequestLoadAsync(id, username, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("api/shelf-declarations/load-requests")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ShelfDeclarationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShelfDeclarationResponse>>> GetLoadRequests(
        [FromQuery] string machineCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineCode))
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "machineCode is required.");
        }

        var items = await _service.GetLoadRequestsByMachineCodeAsync(machineCode, cancellationToken);
        return Ok(items.Select(MapResponse).ToList());
    }

    [HttpPost("api/shelf-declarations/{id:int}/machine-events")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ShelfDeclarationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShelfDeclarationResponse>> ApplyMachineEvent(
        int id,
        [FromBody] MachineShelfEventRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var entity = await _service.ApplyMachineEventAsync(id, request, cancellationToken);
            return entity is null ? NotFound() : Ok(MapResponse(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
        }
    }

    [HttpGet("api/shelf-declarations/{id:int}/events")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Technician}")]
    [ProducesResponseType(typeof(List<ShelfDeclarationEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShelfDeclarationEventResponse>>> GetEvents(
        int id,
        CancellationToken cancellationToken)
    {
        var events = await _service.GetEventsAsync(id, cancellationToken);
        return Ok(events.Select(e => new ShelfDeclarationEventResponse
        {
            Id = e.Id,
            DeclarationId = e.DeclarationId,
            EventType = e.EventType,
            EventAtUtc = e.EventAtUtc,
            ActorType = e.ActorType,
            ActorId = e.ActorId,
            ActorName = e.ActorName,
            MachineId = e.MachineId,
            MachineSlotIndex = e.MachineSlotIndex,
            StagingSlotIndex = e.StagingSlotIndex,
            PayloadJson = e.PayloadJson
        }).ToList());
    }

    [HttpDelete("api/shelf-declarations/{id:int}")]
    [Authorize(Roles = AppRoles.ShelfDeclarationUsers)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        var success = await _service.CancelAsync(id, username, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    private static ShelfDeclarationResponse MapResponse(ManualShelfDeclarationEntity entity)
    {
        return new ShelfDeclarationResponse
        {
            Id = entity.Id,
            MachineId = entity.MachineId,
            MachineCode = entity.Machine?.MachineCode ?? entity.MachineCodeSnapshot,
            MachineName = entity.Machine?.MachineName ?? entity.MachineNameSnapshot,
            Mode = entity.Mode,
            StagingSlotIndex = entity.StagingSlotIndex,
            MachineSlotIndex = entity.MachineSlotIndex,
            ShelfLayoutType = entity.ShelfLayoutType,
            ShelfLayoutName = ShelfDeclarationService.GetLayoutName(entity.ShelfLayoutType),
            OrdersJson = entity.OrdersJson,
            OrderCount = CountOrders(entity.OrdersJson),
            Status = entity.Status,
            CreatedByUsername = entity.CreatedByUsername,
            CreatedAtUtc = entity.CreatedAtUtc,
            PickedByAgvId = entity.PickedByAgvId,
            PickedByAgvName = entity.PickedByAgvName,
            AgvTakenAtUtc = entity.AgvTakenAtUtc,
            LoadRequestedAtUtc = entity.LoadRequestedAtUtc,
            LoadedAtUtc = entity.LoadedAtUtc,
            ProductionStartedAtUtc = entity.ProductionStartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            ClearedAtUtc = entity.ClearedAtUtc,
            CancelledAtUtc = entity.CancelledAtUtc,
            CancelledByUsername = entity.CancelledByUsername,
            ClearedByUsername = entity.ClearedByUsername,
            ProductionDurationSeconds = entity.ProductionDurationSeconds,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static int CountOrders(string ordersJson)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(ordersJson);
            return document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array
                ? document.RootElement.GetArrayLength()
                : 0;
        }
        catch
        {
            return 0;
        }
    }
}
