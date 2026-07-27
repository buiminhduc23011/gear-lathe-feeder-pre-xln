namespace Desktop.App.Configuration;

public sealed class AppOptions
{
    public string MachineName { get; set; } = "Gear Lathe Feeder Pre-XLN";

    public string MachineCode { get; set; } = "GLF-01";

    public string Description { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = "http://localhost:5090";

    public string PlcHost { get; set; } = "127.0.0.1";

    public int PlcPort { get; set; } = 502;

    public int PlcSlaveId { get; set; } = 1;

    public string PlcConnectionMode { get; set; } = "DVP";

    public int PollIntervalMs { get; set; } = 100;

    public bool AutoReconnect { get; set; } = true;

    public int ReconnectIntervalMs { get; set; } = 5000;

    public int MaxRetry { get; set; } = -1;

    // --- PLC Line 1 ---
    public string PlcLine1Host { get; set; } = "127.0.0.1";

    public int PlcLine1Port { get; set; } = 502;

    public int PlcLine1SlaveId { get; set; } = 1;

    public string PlcLine1ConnectionMode { get; set; } = "DVP";

    public int PlcLine1PollIntervalMs { get; set; } = 100;

    // --- PLC Line 2 ---
    public string PlcLine2Host { get; set; } = "127.0.0.1";

    public int PlcLine2Port { get; set; } = 502;

    public int PlcLine2SlaveId { get; set; } = 1;

    public string PlcLine2ConnectionMode { get; set; } = "DVP";

    public int PlcLine2PollIntervalMs { get; set; } = 100;

    // === AGV ===
    public string AgvBaseUrl { get; set; } = "";

    public bool AgvAutoCallEnabled { get; set; } = false;
    public int AgvKe1AutoCallRemainingBelow { get; set; } = 5;
    public int AgvKe2AutoCallRemainingBelow { get; set; } = 5;

    // --- UI ---
    public bool VirtualKeyboardEnabled { get; set; } = true;

    public string PlcEndpoint
    {
        get => $"{PlcHost}:{PlcPort}";
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                PlcHost = "127.0.0.1";
                PlcPort = 502;
                return;
            }

            var endpointParts = value.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            PlcHost = endpointParts[0];

            if (endpointParts.Length > 1 && int.TryParse(endpointParts[1], out var port))
            {
                PlcPort = port;
            }
        }
    }

    public AppOptions Clone()
    {
        return new AppOptions
        {
            MachineName = MachineName,
            MachineCode = MachineCode,
            Description = Description,
            ApiBaseUrl = ApiBaseUrl,
            PlcHost = PlcHost,
            PlcPort = PlcPort,
            PlcSlaveId = PlcSlaveId,
            PlcConnectionMode = PlcConnectionMode,
            PollIntervalMs = PollIntervalMs,
            AutoReconnect = AutoReconnect,
            ReconnectIntervalMs = ReconnectIntervalMs,
            MaxRetry = MaxRetry,
            PlcLine1Host = PlcLine1Host,
            PlcLine1Port = PlcLine1Port,
            PlcLine1SlaveId = PlcLine1SlaveId,
            PlcLine1ConnectionMode = PlcLine1ConnectionMode,
            PlcLine1PollIntervalMs = PlcLine1PollIntervalMs,
            PlcLine2Host = PlcLine2Host,
            PlcLine2Port = PlcLine2Port,
            PlcLine2SlaveId = PlcLine2SlaveId,
            PlcLine2ConnectionMode = PlcLine2ConnectionMode,
            PlcLine2PollIntervalMs = PlcLine2PollIntervalMs,
            VirtualKeyboardEnabled = VirtualKeyboardEnabled,
            AgvBaseUrl = AgvBaseUrl,

            AgvAutoCallEnabled = AgvAutoCallEnabled,
            AgvKe1AutoCallRemainingBelow = AgvKe1AutoCallRemainingBelow,
            AgvKe2AutoCallRemainingBelow = AgvKe2AutoCallRemainingBelow
        };
    }
}
