namespace Server.Api.Contracts.Responses;

public sealed record UploadFileResponse(
    string MachineName,
    string Manufacturer,
    DateTimeOffset SentAtUtc,
    string OriginalFileName,
    string StoredFileName,
    long Size);
