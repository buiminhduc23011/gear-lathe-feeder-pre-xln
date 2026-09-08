namespace Shared.Models.Files;

public sealed class UploadResultDto
{
    public string MachineName { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public DateTimeOffset SentAtUtc { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public long Size { get; set; }
}
