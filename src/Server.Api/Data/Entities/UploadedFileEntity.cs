namespace Server.Api.Data.Entities;

public sealed class UploadedFileEntity
{
    public int Id { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public long Size { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public DateTimeOffset SentAtUtc { get; set; }

    public DateTimeOffset UploadedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string UploadSource { get; set; } = string.Empty;

    public int? UploadedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }

    public int? DeletedByUserId { get; set; }
}
