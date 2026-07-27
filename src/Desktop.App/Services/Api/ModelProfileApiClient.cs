using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Shared.Models.Machines;
using Shared.Models.ModelProfiles;

namespace Desktop.App.Services.Api;

public sealed class ModelProfileApiClient : IModelProfileApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private int? _cachedMachineId;

    public ModelProfileApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> ResolveMachineIdAsync(string machineCode, CancellationToken cancellationToken = default)
    {
        if (_cachedMachineId.HasValue)
        {
            return _cachedMachineId.Value;
        }

        var machines = await _httpClient.GetFromJsonAsync<IReadOnlyList<MachineDto>>(
            "api/machines", JsonOptions, cancellationToken)
            ?? [];

        var machine = machines.FirstOrDefault(m =>
            string.Equals(m.MachineCode, machineCode, StringComparison.OrdinalIgnoreCase));

        if (machine is null)
        {
            throw new InvalidOperationException($"Không tìm thấy máy với mã '{machineCode}'.");
        }

        _cachedMachineId = machine.MachineId;
        return machine.MachineId;
    }

    public async Task<IReadOnlyList<ModelProfileDto>> GetAllAsync(int machineId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<ModelProfileDto>>(
            $"api/machines/{machineId}/models", JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task<ModelProfileDto?> GetByIdAsync(int machineId, int profileId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/machines/{machineId}/models/{profileId}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ModelProfileDto>(JsonOptions, cancellationToken);
    }

    public async Task<ModelProfileDto?> GetByNameAsync(int machineId, string modelName, CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(modelName.Trim());
        var response = await _httpClient.GetAsync(
            $"api/machines/{machineId}/models/by-name?name={encoded}", cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ModelProfileDto>(JsonOptions, cancellationToken);
    }

    public async Task<ModelProfileDto> CreateAsync(
        int machineId,
        SaveModelProfileRequest request,
        CancellationToken cancellationToken = default,
        string? accessTokenOverride = null)
    {
        return await SendAsync<ModelProfileDto>(
            HttpMethod.Post,
            $"api/machines/{machineId}/models",
            request,
            cancellationToken,
            accessTokenOverride);
    }

    public async Task<ModelProfileDto> UpdateAsync(
        int machineId,
        int profileId,
        SaveModelProfileRequest request,
        CancellationToken cancellationToken = default,
        string? accessTokenOverride = null)
    {
        return await SendAsync<ModelProfileDto>(
            HttpMethod.Put,
            $"api/machines/{machineId}/models/{profileId}",
            request,
            cancellationToken,
            accessTokenOverride);
    }

    public async Task DeleteAsync(
        int machineId,
        int profileId,
        CancellationToken cancellationToken = default,
        string? accessTokenOverride = null)
    {
        using var request = CreateRequest(
            HttpMethod.Delete,
            $"api/machines/{machineId}/models/{profileId}",
            null,
            accessTokenOverride);
        var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string uri,
        object? payload,
        CancellationToken cancellationToken,
        string? accessTokenOverride)
    {
        using var request = CreateRequest(method, uri, payload, accessTokenOverride);
        var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken))!;
    }

    private static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string uri,
        object? payload,
        string? accessTokenOverride)
    {
        var request = new HttpRequestMessage(method, uri);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: JsonOptions);
        }

        if (!string.IsNullOrWhiteSpace(accessTokenOverride))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessTokenOverride);
        }

        return request;
    }
}
