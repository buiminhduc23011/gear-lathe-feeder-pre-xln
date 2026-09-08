namespace Server.Api.Contracts.Responses;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public UserResponse User { get; init; } = new();
}
