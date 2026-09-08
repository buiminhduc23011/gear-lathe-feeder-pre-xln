using Server.Api.Data.Entities;

namespace Server.Api.Services;

public interface IUploadedFileService
{
    Task<UploadedFileEntity> CreateAsync(CreateUploadedFileCommand command, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(UploadedFileEntity File, string? UploadedByUsername)>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<UploadedFileEntity?> GetActiveByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<DeleteUploadedFileResult> SoftDeleteAsync(
        int id,
        int performedByUserId,
        string performedByUsername,
        CancellationToken cancellationToken = default);
}

public sealed record CreateUploadedFileCommand(
    string MachineName,
    string Manufacturer,
    DateTimeOffset SentAtUtc,
    StoredFileResult StoredFile,
    string UploadSource,
    int? UploadedByUserId);
