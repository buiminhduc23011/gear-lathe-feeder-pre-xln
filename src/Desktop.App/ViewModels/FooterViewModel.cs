using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Desktop.App.Configuration;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.ViewModels;

public partial class FooterViewModel : ObservableObject
{
    private static readonly TimeSpan ServerPollInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ServerPollTimeout = TimeSpan.FromSeconds(3);
    private static readonly HttpClient ServerStatusHttpClient = new()
    {
        Timeout = Timeout.InfiniteTimeSpan
    };

    private readonly DispatcherTimer _clockTimer;
    private IPlcService? _plcService;
    private IPlcService? _plcServiceLine1;
    private IPlcService? _plcServiceLine2;
    private Uri? _serverBaseUri;
    private DateTimeOffset _lastServerPollUtc = DateTimeOffset.MinValue;
    private int _serverPollInFlight;
    private long _serverEndpointVersion;

    [ObservableProperty]
    private bool isPlcConnected;

    [ObservableProperty]
    private bool isPlcLine1Connected;

    [ObservableProperty]
    private bool isPlcLine2Connected;

    [ObservableProperty]
    private bool isServerConnected;

    [ObservableProperty]
    private string currentTimeText = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string currentDateText = DateTime.Now.ToString("dd/MM/yyyy");

    [ObservableProperty]
    private string scanRateText = "-- ms";

    [ObservableProperty]
    private string scanRateLine1Text = "-- ms";

    [ObservableProperty]
    private string scanRateLine2Text = "-- ms";

    public FooterViewModel()
    {
        UpdateServerEndpoint(AppSettings.Current);
        AppSettings.CurrentChanged += OnCurrentSettingsChanged;

        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) => RefreshClock();
        _clockTimer.Start();
        RefreshClock();
    }

    public string VersionText =>
        $"Desktop {Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}";

    public string ConnectionBadgeText => "PLC Robot";

    public string Line1BadgeText => "PLC Line 1";

    public string Line2BadgeText => "PLC Line 2";

    public string ServerBadgeText => "SERVER";

    public void AttachPlcService(IPlcService plcService)
    {
        if (ReferenceEquals(_plcService, plcService))
        {
            return;
        }

        if (_plcService is not null)
        {
            _plcService.ConnectionChanged -= OnConnectionChanged;
        }

        _plcService = plcService;
        _plcService.ConnectionChanged += OnConnectionChanged;
        IsPlcConnected = _plcService.IsConnected;
    }

    public void AttachPlcLine1Service(IPlcService plcService)
    {
        if (ReferenceEquals(_plcServiceLine1, plcService))
        {
            return;
        }

        if (_plcServiceLine1 is not null)
        {
            _plcServiceLine1.ConnectionChanged -= OnLine1ConnectionChanged;
        }

        _plcServiceLine1 = plcService;
        _plcServiceLine1.ConnectionChanged += OnLine1ConnectionChanged;
        IsPlcLine1Connected = _plcServiceLine1.IsConnected;
    }

    public void AttachPlcLine2Service(IPlcService plcService)
    {
        if (ReferenceEquals(_plcServiceLine2, plcService))
        {
            return;
        }

        if (_plcServiceLine2 is not null)
        {
            _plcServiceLine2.ConnectionChanged -= OnLine2ConnectionChanged;
        }

        _plcServiceLine2 = plcService;
        _plcServiceLine2.ConnectionChanged += OnLine2ConnectionChanged;
        IsPlcLine2Connected = _plcServiceLine2.IsConnected;
    }

    private void OnConnectionChanged(object? sender, bool connected)
    {
        PostToUiThread(() =>
        {
            IsPlcConnected = connected;
            if (!connected)
            {
                ScanRateText = "-- ms";
            }
        });
    }

    private void OnLine1ConnectionChanged(object? sender, bool connected)
    {
        PostToUiThread(() =>
        {
            IsPlcLine1Connected = connected;
            if (!connected)
            {
                ScanRateLine1Text = "-- ms";
            }
        });
    }

    private void OnLine2ConnectionChanged(object? sender, bool connected)
    {
        PostToUiThread(() =>
        {
            IsPlcLine2Connected = connected;
            if (!connected)
            {
                ScanRateLine2Text = "-- ms";
            }
        });
    }

    private void RefreshClock()
    {
        var now = DateTime.Now;
        CurrentTimeText = now.ToString("HH:mm:ss");
        CurrentDateText = now.ToString("dd/MM/yyyy");

        if (_plcService is not null && IsPlcConnected)
        {
            var ms = _plcService.LastScanElapsedMs;
            ScanRateText = ms >= 0 ? $"{ms} ms" : "-- ms";
        }

        if (_plcServiceLine1 is not null && IsPlcLine1Connected)
        {
            var ms = _plcServiceLine1.LastScanElapsedMs;
            ScanRateLine1Text = ms >= 0 ? $"{ms} ms" : "-- ms";
        }

        if (_plcServiceLine2 is not null && IsPlcLine2Connected)
        {
            var ms = _plcServiceLine2.LastScanElapsedMs;
            ScanRateLine2Text = ms >= 0 ? $"{ms} ms" : "-- ms";
        }

        TryStartServerPoll(DateTimeOffset.UtcNow);
    }

    private static void PostToUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            _ = dispatcher.BeginInvoke(action, DispatcherPriority.DataBind);
            return;
        }

        action();
    }

    private void OnCurrentSettingsChanged(object? sender, AppOptions options)
    {
        PostToUiThread(() => UpdateServerEndpoint(options));
    }

    private void UpdateServerEndpoint(AppOptions options)
    {
        _serverBaseUri = TryCreateServerUri(options.ApiBaseUrl);
        _lastServerPollUtc = DateTimeOffset.MinValue;
        Interlocked.Increment(ref _serverEndpointVersion);
        IsServerConnected = false;
    }

    private void TryStartServerPoll(DateTimeOffset nowUtc)
    {
        if (_serverBaseUri is null)
        {
            IsServerConnected = false;
            return;
        }

        if (nowUtc - _lastServerPollUtc < ServerPollInterval)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _serverPollInFlight, 1, 0) != 0)
        {
            return;
        }

        _lastServerPollUtc = nowUtc;
        var endpoint = _serverBaseUri;
        var version = Interlocked.Read(ref _serverEndpointVersion);
        _ = PollServerConnectionAsync(endpoint, version);
    }

    private async Task PollServerConnectionAsync(Uri endpoint, long version)
    {
        var isConnected = false;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            using var timeoutCts = new CancellationTokenSource(ServerPollTimeout);
            using var response = await ServerStatusHttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);

            isConnected = true;
        }
        catch
        {
            isConnected = false;
        }
        finally
        {
            Interlocked.Exchange(ref _serverPollInFlight, 0);
        }

        if (version != Interlocked.Read(ref _serverEndpointVersion))
        {
            return;
        }

        PostToUiThread(() => IsServerConnected = isConnected);
    }

    private static Uri? TryCreateServerUri(string? apiBaseUrl)
    {
        var normalizedUrl = apiBaseUrl?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri;
    }
}
