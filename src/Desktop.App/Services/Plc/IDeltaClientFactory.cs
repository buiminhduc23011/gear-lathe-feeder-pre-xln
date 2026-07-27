using DBI.Drivers.Delta.PLC.Interfaces;
using Desktop.App.Configuration;

namespace Desktop.App.Services.Plc;

public interface IDeltaClientFactory
{
    IDeltaClient Create(AppOptions options);

    IDeltaClient CreateForLine1(AppOptions options);

    IDeltaClient CreateForLine2(AppOptions options);
}
