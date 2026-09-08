using System.ComponentModel.DataAnnotations;

namespace Server.Api.Contracts.Requests;

public sealed class CreateUserRequest
{
    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string FullName { get; init; } = string.Empty;

    [EmailAddress]
    public string? Email { get; init; }

    [Required]
    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;

    public bool IsSystemAccount { get; init; }
}
