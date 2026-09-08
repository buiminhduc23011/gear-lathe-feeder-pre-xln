namespace Server.Api.Services;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(
        Stream content,
        string originalFileName,
        long size,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}

public sealed record StoredFileResult(
    string OriginalFileName,
    string StoredFileName,
    long Size,
    DateTimeOffset SavedAtUtc,
    string StoragePath);
