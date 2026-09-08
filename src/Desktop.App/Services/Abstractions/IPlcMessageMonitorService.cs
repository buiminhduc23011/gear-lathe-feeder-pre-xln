using System;

namespace Desktop.App.Services.Abstractions;

public interface IPlcMessageMonitorService : IDisposable
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
