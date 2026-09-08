using System.IO;
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
    private Uri? _serverBaseUri;
    private DateTimeOffset _lastServerPollUtc = DateTimeOffset.MinValue;
    private int _serverPollInFlight;
    private long _serverEndpointVersion;

    [ObservableProperty]
    private bool isPlcConnected;

    [ObservableProperty]
    private bool isServerConnected;

    [ObservableProperty]
    private string currentTimeText = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string currentDateText = DateTime.Now.ToString("dd/MM/yyyy");

    [ObservableProperty]
    private string scanRateText = "-- ms";

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

    public string BuildDateText =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "BuildDate")?.Value
        ?? File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("dd/MM/yyyy");

    public string ConnectionBadgeText => "PLC";

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

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        PostToUiThread(() => IsPlcConnected = isConnected);
    }

    private void RefreshClock()
    {
        CurrentDateText = DateTime.Now.ToString("dd/MM/yyyy");
        CurrentTimeText = DateTime.Now.ToString("HH:mm:ss");

        if (_plcService is not null && _plcService.IsConnected)
        {
            ScanRateText = $"{_plcService.LastScanElapsedMs} ms";
        }
        else
        {
            ScanRateText = "-- ms";
        }

        CheckServerConnectionInBackground();
    }

    private void CheckServerConnectionInBackground()
    {
        var baseUri = _serverBaseUri;

        if (baseUri is null)
        {
            IsServerConnected = false;
            return;
        }

        var nowUtc = DateTimeOffset.UtcNow;

        if (nowUtc - _lastServerPollUtc < ServerPollInterval)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _serverPollInFlight, 1, 0) != 0)
        {
            return;
        }

        _lastServerPollUtc = nowUtc;
        var version = Interlocked.Read(ref _serverEndpointVersion);

        _ = Task.Run(async () =>
        {
            var isReachable = false;

            try
            {
                using var cts = new CancellationTokenSource(ServerPollTimeout);
                using var request = new HttpRequestMessage(HttpMethod.Head, baseUri);

                using var response = await ServerStatusHttpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                    .ConfigureAwait(false);

                isReachable = true;
            }
            catch
            {
                isReachable = false;
            }
            finally
            {
                Interlocked.Exchange(ref _serverPollInFlight, 0);
            }

            if (Interlocked.Read(ref _serverEndpointVersion) == version)
            {
                PostToUiThread(() => IsServerConnected = isReachable);
            }
        });
    }

    private void OnCurrentSettingsChanged(object? sender, AppOptions options)
    {
        PostToUiThread(() => UpdateServerEndpoint(options));
    }

    private void UpdateServerEndpoint(AppOptions options)
    {
        Interlocked.Increment(ref _serverEndpointVersion);
        _lastServerPollUtc = DateTimeOffset.MinValue;

        if (Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            _serverBaseUri = uri;
            IsServerConnected = true;
        }
        else
        {
            _serverBaseUri = null;
            IsServerConnected = false;
        }
    }

    private static void PostToUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.BeginInvoke(action);
        }
    }
}
