using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data.Entities;
using Server.Api.Exceptions;
using Server.Api.Infrastructure;
using Server.Api.Services;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/machines")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Technician}")]
public sealed class MachinesController : ControllerBase
{
    private readonly IMachineService _machineService;

    public MachinesController(IMachineService machineService)
    {
        _machineService = machineService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MachineResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MachineResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var machines = await _machineService.GetAllAsync(cancellationToken);
        return Ok(machines.Select(MapMachine).ToArray());
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MachineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var machine = await _machineService.GetByIdAsync(id, cancellationToken);
        if (machine is null)
        {
            return NotFound();
        }

        return Ok(MapMachine(machine));
    }

    [HttpPost]
    [ProducesResponseType(typeof(MachineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MachineResponse>> Create(
        [FromBody] CreateMachineRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _machineService.CreateAsync(request, cancellationToken);
            var response = MapMachine(created);
            return CreatedAtAction(nameof(GetById), new { id = response.MachineId }, response);
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MachineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MachineResponse>> Update(
        int id,
        [FromBody] UpdateMachineRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var updated = await _machineService.UpdateAsync(id, request, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(MapMachine(updated));
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _machineService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static MachineResponse MapMachine(MachineEntity machine)
    {
        return new MachineResponse
        {
            MachineId = machine.MachineId,
            MachineCode = machine.MachineCode,
            MachineName = machine.MachineName,
            Manufacturer = machine.Manufacturer,
            Description = machine.Description,
            Model = machine.Model,
            SerialNumber = machine.SerialNumber,
            Location = machine.Location,
            Jig1HeightMm = machine.Jig1HeightMm,
            Jig2HeightMm = machine.Jig2HeightMm,
            Jig3HeightMm = machine.Jig3HeightMm,
            Jig4HeightMm = machine.Jig4HeightMm,
            StagingSlotIndices = GetStagingSlotIndices(machine),
            IsActive = machine.IsActive,
            CreatedAtUtc = machine.CreatedAtUtc,
            UpdatedAtUtc = machine.UpdatedAtUtc
        };
    }

    private static int[] GetStagingSlotIndices(MachineEntity machine)
    {
        var assignedSlot = machine.AssignedStagingSlot1;
        return assignedSlot.HasValue ? [assignedSlot.Value] : Array.Empty<int>();
    }

    private static ModelStateDictionary ToModelState(ValidationProblemException exception)
    {
        var modelState = new ModelStateDictionary();
        foreach (var entry in exception.Errors)
        {
            foreach (var error in entry.Value)
            {
                modelState.AddModelError(entry.Key, error);
            }
        }

        return modelState;
    }
}
