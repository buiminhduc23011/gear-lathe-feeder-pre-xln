namespace Server.Api.Contracts.Responses;

public sealed class UploadedFileListItemResponse
{
    public int Id { get; init; }

    public string OriginalFileName { get; init; } = string.Empty;

    public string StoredFileName { get; init; } = string.Empty;

    public long Size { get; init; }

    public string MachineName { get; init; } = string.Empty;

    public string Manufacturer { get; init; } = string.Empty;

    public DateTimeOffset SentAtUtc { get; init; }

    public DateTimeOffset UploadedAtUtc { get; init; }

    public string UploadSource { get; init; } = string.Empty;

    public string? UploadedByUsername { get; init; }

    public bool CanDelete { get; init; }
}
