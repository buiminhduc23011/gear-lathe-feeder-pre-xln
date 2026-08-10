using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Server.Api.Data.Entities;
using Server.Api.Exceptions;
using Server.Api.Infrastructure;
using Server.Api.Services;
using Shared.Models.ModelProfiles;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/machines/{machineId:int}/models")]
[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Technician}")]
public sealed class ModelProfilesController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IAuthService _authService;
    private readonly IModelProfileService _modelProfileService;

    public ModelProfilesController(IAuthService authService, IModelProfileService modelProfileService)
    {
        _authService = authService;
        _modelProfileService = modelProfileService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ModelProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ModelProfileDto>>> GetAll(int machineId, [FromQuery] bool includeDeleted, CancellationToken cancellationToken)
    {
        var profiles = await _modelProfileService.GetByMachineAsync(machineId, includeDeleted, cancellationToken);
        return Ok(profiles.Select(MapProfile).ToArray());
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ModelProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelProfileDto>> GetById(int machineId, int id, CancellationToken cancellationToken)
    {
        var profile = await _modelProfileService.GetByIdAsync(machineId, id, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(MapProfile(profile));
    }

    [HttpGet("by-name")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ModelProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelProfileDto>> GetByName(int machineId, [FromQuery] string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Tên model không được để trống.");
        }

        var profile = await _modelProfileService.GetByNameAsync(machineId, name, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(MapProfile(profile));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ModelProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ModelProfileDto>> Create(int machineId, [FromBody] SaveModelProfileRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUser = await GetCurrentUsernameAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var created = await _modelProfileService.CreateAsync(machineId, request, currentUser, cancellationToken);
            var response = MapProfile(created);
            return CreatedAtAction(nameof(GetById), new { machineId, id = response.Id }, response);
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ModelProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelProfileDto>> Update(int machineId, int id, [FromBody] SaveModelProfileRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUser = await GetCurrentUsernameAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _modelProfileService.UpdateAsync(machineId, id, request, currentUser, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(MapProfile(updated));
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int machineId, int id, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUsernameAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var deleted = await _modelProfileService.DeleteAsync(machineId, id, currentUser, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/enabled")]
    [ProducesResponseType(typeof(ModelProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModelProfileDto>> SetEnabled(int machineId, int id, [FromBody] SetModelProfileEnabledRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUsernameAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _modelProfileService.SetEnabledAsync(machineId, id, request.IsEnabled, currentUser, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(MapProfile(updated));
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<ModelProfileSnapshotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ModelProfileSnapshotDto>>> GetHistory(int machineId, int id, CancellationToken cancellationToken)
    {
        var snapshots = await _modelProfileService.GetSnapshotsAsync(id, cancellationToken);
        return Ok(snapshots.Select(MapSnapshot).ToArray());
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel(int machineId, CancellationToken cancellationToken)
    {
        var bytes = await _modelProfileService.ExportExcelAsync(machineId, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ModelProfiles_Machine{machineId}.xlsx");
    }

    [HttpPost("update-excel")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateExcel(int machineId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
        {
            ModelState.AddModelError("file", "The file field is required.");
            return ValidationProblem(ModelState);
        }

        var currentUser = await GetCurrentUsernameAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        await using var stream = file.OpenReadStream();
        try
        {
            var count = await _modelProfileService.UpdateExcelAsync(machineId, stream, currentUser, cancellationToken);
            return Ok(new { updated = count });
        }
        catch (ModelExcelValidationException exception)
        {
            return BadRequest(new
            {
                code = ModelExcelValidationException.ErrorCode,
                message = ModelExcelValidationException.ErrorMessage,
                errors = exception.Errors
            });
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportExcel(int machineId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
        {
            ModelState.AddModelError("file", "The file field is required.");
            return ValidationProblem(ModelState);
        }

        var currentUser = await GetCurrentUsernameAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        await using var stream = file.OpenReadStream();
        try
        {
            var count = await _modelProfileService.ReplaceExcelAsync(machineId, stream, currentUser, cancellationToken);
            return Ok(new { imported = count });
        }
        catch (ModelExcelValidationException exception)
        {
            return BadRequest(new
            {
                code = ModelExcelValidationException.ErrorCode,
                message = ModelExcelValidationException.ErrorMessage,
                errors = exception.Errors
            });
        }
        catch (ValidationProblemException exception)
        {
            return ValidationProblem(ToModelState(exception));
        }
    }

    // ───── Helpers ─────

    private async Task<string?> GetCurrentUsernameAsync(CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(User, cancellationToken);
        return user?.Username;
    }

    private static ModelProfileDto MapProfile(ModelProfileEntity entity)
    {
        return new ModelProfileDto
        {
            Id = entity.Id,
            MachineId = entity.MachineId,
            ModelName = entity.ModelName,
            ItemType = entity.ItemType,
            MachiningProgram = entity.MachiningProgram ?? 0,
            Spare1 = entity.Spare1,
            Spare2 = entity.Spare2,
            OuterShaftDiameter = entity.OuterShaftDiameter ?? 0m,
            DiameterOp1 = entity.DiameterOp1 ?? 0f,
            DiameterOp2 = entity.DiameterOp2 ?? 0f,
            InputBlankDiameter = entity.InputBlankDiameter ?? 0f,
            Op2ChuckSleeveDepth = entity.Op2ChuckSleeveDepth ?? 0f,
            TrayUsage = entity.TrayUsage ?? 0,
            TrayType = entity.TrayType ?? 0,
            OrderInput = entity.OrderInput ?? 1,
            RobotData = DeserializeData(entity.RobotData),
            Line1Data = DeserializeData(entity.Line1Data),
            Line2Data = DeserializeData(entity.Line2Data),
            CreatedByUsername = entity.CreatedByUsername,
            UpdatedByUsername = entity.UpdatedByUsername,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            IsDeleted = entity.IsDeleted,
            IsEnabled = entity.IsEnabled,
            DeletedAtUtc = entity.DeletedAtUtc,
            DeletedByUsername = entity.DeletedByUsername
        };
    }

    private static ModelProfileSnapshotDto MapSnapshot(ModelProfileSnapshotEntity entity)
    {
        return new ModelProfileSnapshotDto
        {
            Id = entity.Id,
            ModelProfileId = entity.ModelProfileId,
            ModelName = entity.ModelName,
            ItemType = entity.ItemType,
            MachiningProgram = entity.MachiningProgram ?? 0,
            Spare1 = entity.Spare1,
            Spare2 = entity.Spare2,
            OuterShaftDiameter = entity.OuterShaftDiameter ?? 0m,
            DiameterOp1 = entity.DiameterOp1 ?? 0f,
            DiameterOp2 = entity.DiameterOp2 ?? 0f,
            InputBlankDiameter = entity.InputBlankDiameter ?? 0f,
            Op2ChuckSleeveDepth = entity.Op2ChuckSleeveDepth ?? 0f,
            TrayUsage = entity.TrayUsage ?? 0,
            TrayType = entity.TrayType ?? 0,
            OrderInput = entity.OrderInput ?? 1,
            RobotData = DeserializeData(entity.RobotData),
            Line1Data = DeserializeData(entity.Line1Data),
            Line2Data = DeserializeData(entity.Line2Data),
            ChangeAction = entity.ChangeAction,
            IsEnabled = entity.IsEnabled,
            PerformedByUsername = entity.PerformedByUsername,
            PerformedAtUtc = entity.PerformedAtUtc
        };
    }

    private static Dictionary<string, object?> DeserializeData(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return new Dictionary<string, object?>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions)
               ?? new Dictionary<string, object?>();
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
