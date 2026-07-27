namespace Desktop.App.Services.Api;

public sealed class MachineApiClient : IMachineApiClient
{
    public Task<string> GetMachineSummaryAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult("Machine API client scaffolded");
    }
}
