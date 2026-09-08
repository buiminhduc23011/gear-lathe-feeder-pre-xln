using System.Net.Http;
using System.Net.Http.Json;
using Desktop.App.Configuration;
using Desktop.App.Models.Reports;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.Services;

public sealed class ProductionLifecycleReportApiService : IProductionLifecycleReportApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public ProductionLifecycleReportApiService(AppOptions options)
        : this(options, CreateHttpClient(options), disposeHttpClient: true)
    {
    }

    internal ProductionLifecycleReportApiService(AppOptions options, HttpClient httpClient, bool disposeHttpClient = false)
    {
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;

        if (_httpClient.BaseAddress == null && !string.IsNullOrWhiteSpace(options.ApiBaseUrl))
        {
            _httpClient.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
        }
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    public async Task<ProductionLifecycleReportDto?> GetReportAsync(
        string machineCode,
        string? status,
        int? shelfIndex,
        CancellationToken cancellationToken = default)
    {
        if (_httpClient.BaseAddress == null || string.IsNullOrWhiteSpace(machineCode))
        {
            return null;
        }

        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query.Add($"status={Uri.EscapeDataString(status.Trim())}");
            }

            if (shelfIndex.HasValue)
            {
                query.Add($"shelfIndex={shelfIndex.Value}");
            }

            var suffix = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";
            var response = await _httpClient.GetFromJsonAsync<ProductionLifecycleReportDto>(
                $"api/reports/production-lifecycle{suffix}",
                cancellationToken);
            if (response is null)
            {
                return null;
            }

            var filteredItems = response.Items
                .Where(x => string.Equals(x.MachineCode, machineCode, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return new ProductionLifecycleReportDto
            {
                GeneratedAtUtc = response.GeneratedAtUtc,
                TotalDeclarations = filteredItems.Length,
                InProgressDeclarations = filteredItems.Count(x => x.Status is "Created" or "AgvTaken" or "Loaded" or "InProduction"),
                CompletedDeclarations = filteredItems.Count(x => x.Status is "Completed" or "Cleared"),
                FailedDeclarations = filteredItems.Count(x => x.Status == "Cancelled"),
                Items = filteredItems
            };
        }
        catch
        {
            return null;
        }
    }

    private static HttpClient CreateHttpClient(AppOptions options)
    {
        return new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20),
            BaseAddress = string.IsNullOrWhiteSpace(options.ApiBaseUrl)
                ? null
                : new Uri(options.ApiBaseUrl.TrimEnd('/') + "/")
        };
    }
}
