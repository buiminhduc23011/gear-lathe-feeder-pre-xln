using Desktop.App.Services.Abstractions;

namespace Desktop.App.Services;

public sealed class RobotRuntimeService : IRobotRuntimeService
{
    private readonly IPlcService _plcService;
    private readonly IPlcParameterSyncService _plcParameterSyncService;

    public RobotRuntimeService(IPlcService plcService, IPlcParameterSyncService plcParameterSyncService)
    {
        _plcService = plcService;
        _plcParameterSyncService = plcParameterSyncService;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _plcService.ConnectAsync(cancellationToken);

        try
        {
            await _plcService.ReadAllAsync(cancellationToken);
        }
        catch
        {
        }

        await _plcParameterSyncService.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _plcParameterSyncService.StopAsync(cancellationToken);
        await _plcService.DisconnectAsync(cancellationToken);
    }
}
