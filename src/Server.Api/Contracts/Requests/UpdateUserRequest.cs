using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class UpdateUserRequest
{
    [Required]
    public string Username { get; init; } = string.Empty;

    [MinLength(6)]
    public string? Password { get; init; }

    [Required]
    public string FullName { get; init; } = string.Empty;

    [EmailAddress]
    public string? Email { get; init; }

    [Required]
    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}
