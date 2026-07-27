using System.Net.Http;
using System.Windows;
using Desktop.App.Behaviors;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Alarms;
using Desktop.App.Configuration.Plc;
using Desktop.App.Data.Repositories;
using Desktop.App.Services;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Api;
using Desktop.App.Services.Plc;
using Desktop.App.Services.Agv;
using Desktop.App.Services.Line;
using Desktop.App.ViewModels;
using HandyControl.Themes;
using Desktop.App.Session;

namespace Desktop.App;

public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;

    public IPlcService PlcService { get; private set; } = null!;

    public IPlcService PlcServiceLine1 { get; private set; } = null!;

    public IPlcService PlcServiceLine2 { get; private set; } = null!;

    public IRobotRuntimeService RobotRuntimeService { get; private set; } = null!;

    public ISettingsService SettingsService { get; private set; } = null!;

    public IPlcParameterSettingsService PlcParameterSettingsService { get; private set; } = null!;

    public IPlcParameterSyncService PlcParameterSyncService { get; private set; } = null!;

    public IAlarmHistoryRepository AlarmHistoryRepository { get; private set; } = null!;

    public IAlarmMonitorService AlarmMonitorService { get; private set; } = null!;

    public AgvCallHistoryRepository AgvCallHistoryRepository { get; private set; } = null!;

    public AgvTransferApiService AgvTransferApiService { get; private set; } = null!;

    public AgvBackgroundService AgvBackgroundService { get; private set; } = null!;

    public IProductionLifecycleReportApiService ProductionLifecycleReportApiService { get; private set; } = null!;

    public TrayConfigRepository TrayConfigRepository { get; private set; } = null!;

    public ShelfOrderCacheRepository ShelfOrderCacheRepository { get; private set; } = null!;

    public AuditLogRepository AuditLogRepository { get; private set; } = null!;

    /// <summary>
    /// Shared HeaderViewModel instance consumed by both Header and NavSidebar
    /// so that CurrentPage stays in sync across both controls.
    /// </summary>
    public HeaderViewModel HeaderViewModel { get; private set; } = null!;

    public FooterViewModel FooterViewModel { get; private set; } = null!;

    public IModelProfileApiClient ModelProfileApiClient { get; private set; } = null!;

    public INotificationDialogService NotificationDialogService { get; private set; } = null!;

    public IPlcMessageMonitorService PlcMessageMonitorService { get; private set; } = null!;

    public ILoginDialogService LoginDialogService { get; private set; } = null!;

    public LineModelPlcService LineModelService1 { get; private set; } = null!;

    public LineModelPlcService LineModelService2 { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(true, "Global\\GearLatheFeederDesktop_SingleInstance", out var isNewInstance);

        if (!isNewInstance)
        {
            MessageBox.Show(
                "Ứng dụng đã đang chạy. Không thể mở thêm.",
                "Gear Lathe Feeder Desktop",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            Shutdown();
            return;
        }

        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        SettingsService = new SettingsService(new SettingsRepository());
        PlcParameterSettingsService = new PlcParameterSettingsService(new Data.Repositories.PlcParameterSettingsRepository());

        try
        {
            Task.Run(() => AppConfigurationBootstrapper.InitializeAsync(SettingsService)).GetAwaiter().GetResult();
        }
        catch
        {
            AppSettings.Initialize();
        }

        // Sync virtual keyboard enabled state from persisted settings
        VirtualKeyboardStateService.Instance.IsEnabled = AppSettings.Current.VirtualKeyboardEnabled;

        // Register global GotKeyboardFocus hook so ALL TextBox / PasswordBox
        // automatically get the on-screen keyboard (no per-control markup needed).
        VirtualKeyboardBehavior.RegisterGlobalHook();

        var factory = new DeltaClientFactory();

        PlcService = new PlcService(
            "Robot",
            factory,
            AppSettings.Current,
            PlcTagCatalog.All,
            DeltaPlcMetadata.BuildFor(PlcTagCatalog.All),
            static (f, o) => f.Create(o),
            AppSettings.Current.PollIntervalMs,
            PlcTagCatalog.WritableTagNames);
        PlcServiceLine1 = new PlcService(
            "Line 1",
            factory,
            AppSettings.Current,
            PlcTagCatalog.AllLine1,
            DeltaPlcMetadata.BuildFor(PlcTagCatalog.AllLine1),
            static (f, o) => f.CreateForLine1(o),
            AppSettings.Current.PlcLine1PollIntervalMs);
        PlcServiceLine2 = new PlcService(
            "Line 2",
            factory,
            AppSettings.Current,
            PlcTagCatalog.AllLine2,
            DeltaPlcMetadata.BuildFor(PlcTagCatalog.AllLine2),
            static (f, o) => f.CreateForLine2(o),
            AppSettings.Current.PlcLine2PollIntervalMs);

        PlcParameterSyncService = new PlcParameterSyncService(PlcService, PlcParameterSettingsService);
        AlarmDefinitionCatalog.Validate();
        AlarmHistoryRepository = new AlarmHistoryRepository();
        AlarmMonitorService = new AlarmMonitorService(PlcService, AlarmHistoryRepository);
        AlarmMonitorService.InitializeAsync().GetAwaiter().GetResult();
        RobotRuntimeService = new RobotRuntimeService(PlcService, PlcParameterSyncService);
        
        AgvCallHistoryRepository = new AgvCallHistoryRepository();
        AgvTransferApiService = new AgvTransferApiService(AppSettings.Current);
        ProductionLifecycleReportApiService = new ProductionLifecycleReportApiService(AppSettings.Current);
        TrayConfigRepository = new TrayConfigRepository();
        ShelfOrderCacheRepository = new ShelfOrderCacheRepository();
        AgvBackgroundService = new AgvBackgroundService(AgvTransferApiService, AgvCallHistoryRepository, PlcService, ShelfOrderCacheRepository, TrayConfigRepository);

        AuditLogRepository = new AuditLogRepository();

        HeaderViewModel = new HeaderViewModel();
        HeaderViewModel.AttachAlarmMonitorService(AlarmMonitorService);
        HeaderViewModel.AttachPlcService(PlcService);
        FooterViewModel = new FooterViewModel();
        FooterViewModel.AttachPlcService(PlcService);
        FooterViewModel.AttachPlcLine1Service(PlcServiceLine1);
        FooterViewModel.AttachPlcLine2Service(PlcServiceLine2);

        var authTokenHandler = new AuthTokenHandler
        {
            InnerHandler = new HttpClientHandler()
        };
        var desktopHttpClient = new HttpClient(authTokenHandler)
        {
            BaseAddress = new Uri(AppSettings.Current.ApiBaseUrl.TrimEnd('/') + "/")
        };
        var lineHttpClient = new HttpClient(new HttpClientHandler())
        {
            BaseAddress = new Uri(AppSettings.Current.ApiBaseUrl.TrimEnd('/') + "/")
        };

        var desktopUserApiClient = new UserApiClient(desktopHttpClient);
        var lineUserApiClient = new UserApiClient(lineHttpClient);
        var lineModelProfileApiClient = new ModelProfileApiClient(lineHttpClient);

        LoginDialogService = new LoginDialogService(desktopUserApiClient);
        ModelProfileApiClient = new ModelProfileApiClient(desktopHttpClient);
        AgvBackgroundService.AttachModelProfileApiClient(ModelProfileApiClient);
        NotificationDialogService = new NotificationDialogService();
        HeaderViewModel.AttachLoginDialogService(LoginDialogService);

        PlcMessageMonitorService = new PlcMessageMonitorService(PlcService, NotificationDialogService);
        PlcMessageMonitorService.InitializeAsync().GetAwaiter().GetResult();

        LineModelService1 = new LineModelPlcService("Line 1", PlcServiceLine1, lineModelProfileApiClient, lineUserApiClient, LineTagSet.ForLine1());
        LineModelService2 = new LineModelPlcService("Line 2", PlcServiceLine2, lineModelProfileApiClient, lineUserApiClient, LineTagSet.ForLine2());

        base.OnStartup(e);
        _ = StartRuntimeAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            // Run all cleanup in parallel with a hard 5-second deadline.
            // PLC driver socket calls can block indefinitely — we must not wait forever.
            var shutdownTask = Task.Run(async () =>
            {
                try { await RobotRuntimeService.StopAsync(); } catch { }

                await Task.WhenAll(
                    SafeDisposeAsync(PlcService),
                    SafeDisposeAsync(PlcServiceLine1),
                    SafeDisposeAsync(PlcServiceLine2));

                try { AlarmMonitorService.Dispose(); } catch { }
                try { PlcMessageMonitorService.Dispose(); } catch { }
                try { await PlcParameterSyncService.DisposeAsync(); } catch { }
                try { LineModelService1.Dispose(); } catch { }
                try { LineModelService2.Dispose(); } catch { }
                try { AgvBackgroundService.Dispose(); } catch { }
            });

            if (!shutdownTask.Wait(TimeSpan.FromSeconds(5)))
            {
                System.Diagnostics.Debug.WriteLine("[App] Shutdown timed out after 5s — forcing exit.");
            }
        }
        catch
        {
        }

        base.OnExit(e);

        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
    }

    private static async Task SafeDisposeAsync(IPlcService service)
    {
        try { await service.DisposeAsync(); } catch { }
    }

    private async Task StartRuntimeAsync()
    {
        // Connect all 3 PLCs in parallel with unified style
        await Task.WhenAll(
            ConnectPlcAsync("Robot", PlcService),
            ConnectPlcAsync("Line 1", PlcServiceLine1),
            ConnectPlcAsync("Line 2", PlcServiceLine2));
        // Start Robot-specific runtime (parameter sync, etc.)
        try
        {
            await PlcService.ReadAllAsync();
            await PlcParameterSyncService.StartAsync();
        }
        catch
        {
        }

        AgvBackgroundService.Start();

        // Start Line Model services (event-driven, listen to DataUpdated)
        _ = Task.Run(async () =>
        {
            try { await LineModelService1.StartAsync(); } catch { }
        });
        _ = Task.Run(async () =>
        {
            try { await LineModelService2.StartAsync(); } catch { }
        });
    }

    private static async Task ConnectPlcAsync(string name, IPlcService plcService)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[PLC] Connecting {name}...");
            await plcService.ConnectAsync();
            System.Diagnostics.Debug.WriteLine($"[PLC] {name} connected: {plcService.IsConnected}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PLC] {name} connect failed: {ex.Message}");
        }
    }
}
