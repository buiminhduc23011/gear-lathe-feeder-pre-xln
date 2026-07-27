namespace Desktop.App.Configuration;

public sealed class AppOptions
{
    public string MachineName { get; set; } = "Gear Lathe Feeder Pre-XLN";

    public string MachineCode { get; set; } = "GLF-01";

    public string Description { get; set; } = string.Empty;

    public string ApiBaseUrl { get; set; } = "http://localhost:5095";

    public string PlcHost { get; set; } = "192.168.3.6";


    public int PlcPort { get; set; } = 502;

    public int PlcSlaveId { get; set; } = 1;

    public string PlcConnectionMode { get; set; } = "DVP";

    public int PollIntervalMs { get; set; } = 100;

    public bool AutoReconnect { get; set; } = true;

    public int ReconnectIntervalMs { get; set; } = 5000;

    public int MaxRetry { get; set; } = -1;


    // === AGV ===
    public string AgvBaseUrl { get; set; } = "";

    public bool AgvAutoCallEnabled { get; set; } = false;
    public int AgvKe1AutoCallRemainingBelow { get; set; } = 5;
    public int AgvKe2AutoCallRemainingBelow { get; set; } = 5;

    // --- UI ---
    public bool VirtualKeyboardEnabled { get; set; } = false;

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
            VirtualKeyboardEnabled = VirtualKeyboardEnabled,
            AgvBaseUrl = AgvBaseUrl,

            AgvAutoCallEnabled = AgvAutoCallEnabled,
            AgvKe1AutoCallRemainingBelow = AgvKe1AutoCallRemainingBelow,
            AgvKe2AutoCallRemainingBelow = AgvKe2AutoCallRemainingBelow
        };
    }
}
