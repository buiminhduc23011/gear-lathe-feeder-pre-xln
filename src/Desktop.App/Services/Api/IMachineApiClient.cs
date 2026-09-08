namespace Desktop.App.Services.Api;

public interface IMachineApiClient
{
    Task<string> GetMachineSummaryAsync(CancellationToken cancellationToken = default);
}
