using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Desktop.App.Models.Ui;

namespace Desktop.App.Services.Api;

public sealed class UserApiClient : IUserApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResult> LoginAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var body = new { username = userName, password };

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(
                "api/auth/login", body, JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            // Server unreachable — try bypass credentials for local Settings editing
            if (IsBypassCredentials(userName, password))
            {
                return LoginResult.Ok(
                    BypassUserName, BypassFullName, BypassRole,
                    "offline-bypass-token",
                    DateTimeOffset.UtcNow.AddDays(365));
            }

            return LoginResult.Fail($"Không thể kết nối đến máy chủ: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return LoginResult.Fail("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return LoginResult.Fail($"Lỗi máy chủ ({(int)response.StatusCode}).");
        }

        var dto = await response.Content
            .ReadFromJsonAsync<AuthResponseDto>(JsonOptions, cancellationToken);

        if (dto is null)
        {
            return LoginResult.Fail("Phản hồi từ máy chủ không hợp lệ.");
        }

        return LoginResult.Ok(
            dto.User.Username,
            dto.User.FullName,
            dto.User.Role,
            dto.AccessToken,
            dto.ExpiresAtUtc);
    }

    // ── Bypass credentials (offline Settings editing) ──
    private const string BypassUserName = "sti-local";
    private const string BypassPassword = "09052016";
    private const string BypassFullName = "STI-bypass";
    private const string BypassRole = "Admin";

    private static bool IsBypassCredentials(string userName, string password)
    {
        return string.Equals(userName, BypassUserName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, BypassPassword, StringComparison.Ordinal);
    }

    private sealed class AuthResponseDto
    {
        public string AccessToken { get; init; } = string.Empty;
        public DateTimeOffset ExpiresAtUtc { get; init; }
        public UserDto User { get; init; } = new();
    }

    private sealed class UserDto
    {
        public string Username { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}
