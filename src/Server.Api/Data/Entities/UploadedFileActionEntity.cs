namespace Server.Api.Data.Entities;

public sealed class UploadedFileActionEntity
{
    public int Id { get; set; }

    public int UploadedFileId { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public int PerformedByUserId { get; set; }

    public string PerformedByUsernameSnapshot { get; set; } = string.Empty;

    public DateTimeOffset PerformedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
