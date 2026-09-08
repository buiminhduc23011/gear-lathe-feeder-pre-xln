namespace Desktop.App.Models.Ui;

public sealed record LoginResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; init; }

    public static LoginResult Fail(string errorMessage)
        => new() { Success = false, ErrorMessage = errorMessage };

    public static LoginResult Ok(
        string username,
        string fullName,
        string role,
        string accessToken,
        DateTimeOffset expiresAtUtc)
        => new()
        {
            Success = true,
            Username = username,
            FullName = fullName,
            Role = role,
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc
        };
}
