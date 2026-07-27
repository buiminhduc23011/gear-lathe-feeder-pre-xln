using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Contracts.Requests;
using Server.Api.Contracts.Responses;
using Server.Api.Data.Entities;
using Server.Api.Infrastructure;
using Server.Api.Services;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FilesController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;
    private readonly IUploadedFileService _uploadedFileService;

    public FilesController(
        IAuthService authService,
        IFileStorageService fileStorageService,
        IUploadedFileService uploadedFileService,
        ILogger<FilesController> logger)
    {
        _authService = authService;
        _fileStorageService = fileStorageService;
        _uploadedFileService = uploadedFileService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<UploadedFileListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UploadedFileListItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var files = await _uploadedFileService.GetActiveAsync(cancellationToken);
        var canDelete = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Technician);

        return Ok(files.Select(item => MapFile(item.File, item.UploadedByUsername, canDelete)).ToArray());
    }

    [HttpGet("{id:int}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var file = await _uploadedFileService.GetActiveByIdAsync(id, cancellationToken);
        if (file is null)
        {
            return NotFound();
        }

        var stream = await _fileStorageService.OpenReadAsync(file.StoragePath, cancellationToken);
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, "application/octet-stream", file.OriginalFileName);
    }

    [HttpPost("upload")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UploadFileResponse>> Upload(
        [FromForm] UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await ResolveAuthenticatedUserAsync(cancellationToken);
        if (User.Identity?.IsAuthenticated == true && currentUser is null)
        {
            return Unauthorized();
        }

        if (currentUser is not null &&
            currentUser.Role is not AppRoles.Admin and not AppRoles.Technician)
        {
            return Forbid();
        }

        if (request.File is null)
        {
            ModelState.AddModelError("file", "The file field is required.");
            return ValidationProblem(ModelState);
        }

        if (request.File.Length <= 0)
        {
            ModelState.AddModelError("file", "Uploaded file must not be empty.");
            return ValidationProblem(ModelState);
        }

        var machineName = string.IsNullOrWhiteSpace(request.MachineName) ? "" : request.MachineName;
        var manufacturer = string.IsNullOrWhiteSpace(request.Manufacturer) ? "" : request.Manufacturer;
        var sentAtUtc = request.SentAtUtc ?? DateTime.UtcNow;

        await using var stream = request.File.OpenReadStream();

        var storedFile = await _fileStorageService.SaveAsync(
            stream,
            request.File.FileName,
            request.File.Length,
            cancellationToken);

        var uploadSource = currentUser is null
            ? FileUploadSources.DeviceApi
            : FileUploadSources.WebManual;

        await _uploadedFileService.CreateAsync(
            new CreateUploadedFileCommand(
                machineName,
                manufacturer,
                sentAtUtc,
                storedFile,
                uploadSource,
                currentUser?.Id),
            cancellationToken);

        _logger.LogInformation(
            "Stored uploaded file {OriginalFileName} as {StoredFileName} from machine {MachineName} manufacturer {Manufacturer} sent at {SentAtUtc} source {UploadSource}",
            storedFile.OriginalFileName,
            storedFile.StoredFileName,
            machineName,
            manufacturer,
            sentAtUtc,
            uploadSource);

        return Ok(new UploadFileResponse(
            machineName,
            manufacturer,
            sentAtUtc,
            storedFile.OriginalFileName,
            storedFile.StoredFileName,
            storedFile.Size));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Technician}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var currentUser = await _authService.GetCurrentUserAsync(User, cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var result = await _uploadedFileService.SoftDeleteAsync(
            id,
            currentUser.Id,
            currentUser.Username,
            cancellationToken);

        if (result == DeleteUploadedFileResult.NotFound)
        {
            return NotFound();
        }

        return NoContent();
    }

    private async Task<UserEntity?> ResolveAuthenticatedUserAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return await _authService.GetCurrentUserAsync(User, cancellationToken);
    }

    private static UploadedFileListItemResponse MapFile(UploadedFileEntity file, string? uploadedByUsername, bool canDelete)
    {
        return new UploadedFileListItemResponse
        {
            Id = file.Id,
            OriginalFileName = file.OriginalFileName,
            StoredFileName = file.StoredFileName,
            Size = file.Size,
            MachineName = file.MachineName,
            Manufacturer = file.Manufacturer,
            SentAtUtc = file.SentAtUtc,
            UploadedAtUtc = file.UploadedAtUtc,
            UploadSource = file.UploadSource,
            UploadedByUsername = uploadedByUsername,
            CanDelete = canDelete
        };
    }
}
