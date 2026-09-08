using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class UploadFileRequest
{
    [Required]
    public IFormFile? File { get; init; }

    [Required]
    public string? MachineName { get; init; }

    [Required]
    public string? Manufacturer { get; init; }

    [Required]
    public DateTimeOffset? SentAtUtc { get; init; }
}
