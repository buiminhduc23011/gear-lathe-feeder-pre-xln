namespace Desktop.App.Services.Abstractions;

public interface IRobotRuntimeService
{
    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
