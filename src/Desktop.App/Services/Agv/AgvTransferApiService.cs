using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Desktop.App.Configuration;
using Desktop.App.Models.Agv;

namespace Desktop.App.Services.Agv;

public class AgvTransferApiService : IAgvTransferApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _machineCode;
    private readonly bool _disposeHttpClient;

    public AgvTransferApiService(AppOptions options)
        : this(options, CreateHttpClient(options), disposeHttpClient: true)
    {
    }

    internal AgvTransferApiService(AppOptions options, HttpClient httpClient, bool disposeHttpClient = false)
    {
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(options.AgvBaseUrl))
        {
            _httpClient.BaseAddress = new Uri(options.AgvBaseUrl);
        }

        _machineCode = options.MachineCode;
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private static HttpClient CreateHttpClient(AppOptions options)
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
            BaseAddress = string.IsNullOrWhiteSpace(options.AgvBaseUrl) ? null : new Uri(options.AgvBaseUrl)
        };
    }

    public async Task<(bool Success, string Message)> CreateCommandAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress == null)
            return (false, "URL AGV chưa được cấu hình.");

        try
        {
            var request = new CreateCommandRequest
            {
                MachineCode = _machineCode,
                Position = (int)position
            };

            var response = await _httpClient.PostAsJsonAsync("/api-system/create-command-trans-gear-lathe-feeder", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

            var result = await response.Content.ReadFromJsonAsync<AgvApiResponse<object>>(cancellationToken: cancellationToken);
            if (result == null)
                return (false, "Không thể đọc dữ liệu phản hồi.");

            if (result.Code != 200)
                return (false, result.Message ?? "Lỗi từ AGV Server.");

            return (true, "Thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối: {ex.Message}");
        }
    }

    public async Task<(AgvTransferStatus Status, string Message, int? DeclarationId)> CheckStatusAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress == null)
            return (AgvTransferStatus.None, "URL AGV chưa được cấu hình.", null);

        try
        {
            var request = new CheckStatusRequest
            {
                MachineCode = _machineCode,
                Position = (int)position
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/api-system/check-status-command-trans-gear-lathe-feeder")
            {
                Content = JsonContent.Create(request)
            };
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return (AgvTransferStatus.None, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", null);

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<AgvApiResponse<JsonElement>>(jsonContent);

            if (result == null)
                return (AgvTransferStatus.None, "Không thể đọc dữ liệu phản hồi.", null);

            if (result.Code != 200)
                return (AgvTransferStatus.None, result.Message ?? "Lỗi từ AGV Server.", null);

            var status = DeserializeStatus(result.Data);
            int? declarationId = null;

            if (status == (int)AgvTransferStatus.Completed)
            {
                LogRawJson(position, jsonContent);
                declarationId = DeserializeDeclarationId(result.Data);

                if (!declarationId.HasValue)
                    return (AgvTransferStatus.Completed, "Server trả status = 3 nhưng thiếu declarationId.", null);
            }

            return ((AgvTransferStatus)status, "Thành công", declarationId);
        }
        catch (Exception ex)
        {
            return (AgvTransferStatus.None, $"Lỗi kết nối: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> ConfirmCommandAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress == null)
            return (false, "URL AGV chưa được cấu hình.");

        try
        {
            var request = new ConfirmCommandRequest
            {
                MachineCode = _machineCode,
                Position = (int)position
            };

            var response = await _httpClient.PostAsJsonAsync("/api-system/confirm-command-trans-gear-lathe-feeder", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");

            var result = await response.Content.ReadFromJsonAsync<AgvApiResponse<object>>(cancellationToken: cancellationToken);
            if (result == null)
                return (false, "Không thể đọc dữ liệu phản hồi.");

            if (result.Code != 200)
                return (false, result.Message ?? "Lỗi từ AGV Server.");

            return (true, "Thành công");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi kết nối: {ex.Message}");
        }
    }

    internal static int DeserializeStatus(JsonElement? data)
    {
        if (!data.HasValue)
            return 0;

        if (data.Value.ValueKind == JsonValueKind.Number)
            return data.Value.GetInt32();

        if (data.Value.ValueKind == JsonValueKind.Object
            && data.Value.TryGetProperty("status", out var statusProperty)
            && statusProperty.ValueKind == JsonValueKind.Number)
        {
            return statusProperty.GetInt32();
        }

        return 0;
    }

    internal static int? DeserializeDeclarationId(JsonElement? data)
    {
        if (!data.HasValue || data.Value.ValueKind != JsonValueKind.Object)
            return null;

        if (data.Value.TryGetProperty("declarationId", out var declarationIdProperty)
            && declarationIdProperty.ValueKind == JsonValueKind.Number)
        {
            return declarationIdProperty.GetInt32();
        }

        return null;
    }

    private void LogRawJson(AgvPosition position, string rawJson)
    {
        try
        {
            string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logsDir);
            string fileName = $"agv-payloads-{DateTime.Now:yyyyMMdd}.txt";
            string path = Path.Combine(logsDir, fileName);

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] Position: {position}\n{rawJson}\n----------------------------------\n";
            File.AppendAllText(path, logEntry);
        }
        catch
        {
            // Ignore logging failures.
        }
    }
}
