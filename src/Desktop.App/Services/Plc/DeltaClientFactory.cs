using System.IO.Ports;
using DBI.Drivers.Delta.PLC;
using DBI.Drivers.Delta.PLC.Interfaces;
using Desktop.App.Configuration;

namespace Desktop.App.Services.Plc;

public sealed class DeltaClientFactory : IDeltaClientFactory
{
    public IDeltaClient Create(AppOptions options)
    {
        return CreateClient(options.PlcHost, options.PlcPort, options.PlcConnectionMode, options.PlcSlaveId, options);
    }

    private static IDeltaClient CreateClient(string host, int port, string? connectionMode, int slaveId, AppOptions options)
    {
        var parsedMode = ParseConnectionMode(connectionMode);
        var slaveIdByte = checked((byte)slaveId);

        IDeltaClient client = parsedMode switch
        {
            DeltaConnectionType.TcpDVP or DeltaConnectionType.TcpAS => new DeltaClient(host, port, parsedMode, slaveId: slaveIdByte),
            DeltaConnectionType.Ascii => new DeltaClient(
                host,
                port,
                parsedMode,
                dataBits: 8,
                stopBits: StopBits.One,
                parity: Parity.None,
                slaveId: slaveIdByte),
            _ => throw new NotSupportedException($"PLC connection mode '{connectionMode}' is not supported."),
        };

        client.AutoReconnect = options.AutoReconnect;
        client.ReconnectInterval = options.ReconnectIntervalMs;
        client.MaxRetry = options.MaxRetry;

        return client;
    }

    private static DeltaConnectionType ParseConnectionMode(string? connectionMode)
    {
        if (Enum.TryParse<DeltaConnectionType>(connectionMode, ignoreCase: true, out var parsedMode))
        {
            return parsedMode;
        }

        if (string.Equals(connectionMode, "dvp", StringComparison.OrdinalIgnoreCase))
        {
            return DeltaConnectionType.TcpDVP;
        }

        if (string.Equals(connectionMode, "as", StringComparison.OrdinalIgnoreCase))
        {
            return DeltaConnectionType.TcpAS;
        }

        if (string.Equals(connectionMode, "tcp", StringComparison.OrdinalIgnoreCase))
        {
            return DeltaConnectionType.TcpDVP;
        }

        return DeltaConnectionType.TcpDVP;
    }
}
