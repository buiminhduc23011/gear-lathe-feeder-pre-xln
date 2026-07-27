namespace Server.Api.Contracts.Responses;

public sealed class UserResponse
{
    public int Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string Role { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool IsSystemAccount { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}
