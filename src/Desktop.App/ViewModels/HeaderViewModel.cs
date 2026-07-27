using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Messages;
using Desktop.App.Models.Alarms;
using Desktop.App.Models.Runtime;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;
using Desktop.App.Session;

namespace Desktop.App.ViewModels;

public partial class HeaderViewModel : ObservableObject
{
    private IAlarmMonitorService? _alarmMonitorService;
    private ILoginDialogService? _loginDialogService;
    private IPlcService? _plcService;
    private bool _isApplyingPlcBuzzerState;
    private bool _isApplyingPlcLightCurtainState;

    [ObservableProperty]
    private PageType currentPage = PageType.Auto;

    [ObservableProperty]
    private bool isHome = true;

    [ObservableProperty]
    private bool isActiveSetting = true;

    [ObservableProperty]
    private bool offBuzzer;

    [ObservableProperty]
    private bool offLightCurtain;

    [ObservableProperty]
    private bool isPlcConnected;

    [ObservableProperty]
    private string userDisplayName = "Khách Vận Hành";

    [ObservableProperty]
    private string authActionText = "Đăng Nhập";

    [ObservableProperty]
    private bool isAuthenticated;

    [ObservableProperty]
    private bool hasActiveAlarm;

    [ObservableProperty]
    private int activeAlarmCount;

    [ObservableProperty]
    private string alarmTickerText = "Máy hoạt động bình thường";

    [ObservableProperty]
    private string machineName = AppSettings.Current.MachineName;

    [ObservableProperty]
    private string machineCode = AppSettings.Current.MachineCode;

    public HeaderViewModel()
    {
        RefreshUserState();
        RefreshMachineIdentity();
        AppSettings.CurrentChanged += OnCurrentSettingsChanged;
        AppSession.SessionChanged += OnSessionChanged;
    }

    public string AlarmStateText => HasActiveAlarm ? "ALARM" : "STATUS";

    [RelayCommand]
    private void OpenAutoPage() => NavigateTo(PageType.Auto);

    [RelayCommand]
    private void OpenManualPage() => NavigateTo(PageType.Manual);

    [RelayCommand]
    private void OpenGpioPage() => NavigateTo(PageType.Gpio);

    [RelayCommand]
    private void OpenHistoryPage() => NavigateTo(PageType.History);

    [RelayCommand]
    private void OpenSettingsPage() => NavigateTo(PageType.Settings);

    [RelayCommand]
    private void OpenReportPage() => NavigateTo(PageType.Report);

    [RelayCommand]
    private void ChangePage(PageType page) => NavigateTo(page);

    [RelayCommand]
    private async Task ToggleAuthAsync()
    {
        if (AppSession.IsAuthenticated)
        {
            AppSession.Logout();
            return;
        }

        if (_loginDialogService is null)
        {
            return;
        }

        var result = await _loginDialogService.ShowAsync();

        if (result is { Success: true })
        {
            AppSession.SetSession(result);
        }
    }

    public void AttachLoginDialogService(ILoginDialogService loginDialogService)
    {
        _loginDialogService = loginDialogService;
    }

    public void AttachPlcService(IPlcService plcService)
    {
        if (ReferenceEquals(_plcService, plcService))
        {
            return;
        }

        if (_plcService is not null)
        {
            _plcService.ConnectionChanged -= OnPlcConnectionChanged;
            _plcService.DataUpdated -= OnPlcDataUpdated;
        }

        _plcService = plcService;
        _plcService.ConnectionChanged += OnPlcConnectionChanged;
        _plcService.DataUpdated += OnPlcDataUpdated;

        ApplyPlcConnectionState(_plcService.IsConnected);
        SyncBuzzerStateFromCache();
        SyncLightCurtainStateFromCache();
    }

    public void AttachAlarmMonitorService(IAlarmMonitorService alarmMonitorService)
    {
        if (ReferenceEquals(_alarmMonitorService, alarmMonitorService))
        {
            return;
        }

        if (_alarmMonitorService is not null)
        {
            _alarmMonitorService.StateChanged -= OnAlarmStateChanged;
        }

        _alarmMonitorService = alarmMonitorService;
        _alarmMonitorService.StateChanged += OnAlarmStateChanged;
        ApplyAlarmState(_alarmMonitorService.GetAlarmBarState());
    }

    partial void OnHasActiveAlarmChanged(bool value)
    {
        OnPropertyChanged(nameof(AlarmStateText));
    }

    partial void OnOffBuzzerChanged(bool value)
    {
        if (_isApplyingPlcBuzzerState)
        {
            return;
        }

        var plcService = _plcService;
        if (plcService is null)
        {
            return;
        }

        _ = PersistBuzzerStateAsync(plcService, value);
    }

    partial void OnOffLightCurtainChanged(bool value)
    {
        if (_isApplyingPlcLightCurtainState)
        {
            return;
        }

        var plcService = _plcService;
        if (plcService is null)
        {
            return;
        }

        if (!AppSession.IsAuthenticated)
        {
            PostToUiThread(SyncLightCurtainStateFromCache);
            _ = PromptLoginAndToggleAsync(plcService, value);
            return;
        }

        _ = PersistLightCurtainStateAsync(plcService, value);
    }

    private async Task PromptLoginAndToggleAsync(IPlcService plcService, bool value)
    {
        if (_loginDialogService is null)
        {
            return;
        }

        var result = await _loginDialogService.ShowAsync();
        if (result is { Success: true })
        {
            AppSession.SetSession(result);
            await PersistLightCurtainStateAsync(plcService, value);
        }
    }

    private void NavigateTo(PageType page)
    {
        CurrentPage = page;
        IsHome = page == PageType.Auto;
        WeakReferenceMessenger.Default.Send(new PageChangedMessage(page));
    }

    private void OnAlarmStateChanged(object? sender, AlarmStateChangedEventArgs e)
    {
        PostToUiThread(() => ApplyAlarmState(e.State));
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        PostToUiThread(RefreshUserState);
    }

    private void OnPlcConnectionChanged(object? sender, bool connected)
    {
        PostToUiThread(() =>
        {
            ApplyPlcConnectionState(connected);
            SyncBuzzerStateFromCache();
            SyncLightCurtainStateFromCache();
        });
    }

    private void OnPlcDataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (e.Snapshot.ContainsKey(PlcTagCatalog.Manual.BuzzerOnOff.Name))
        {
            PostToUiThread(SyncBuzzerStateFromCache);
        }

        if (e.Snapshot.ContainsKey(PlcTagCatalog.Manual.LightCurtainOnOff.Name))
        {
            PostToUiThread(SyncLightCurtainStateFromCache);
        }
    }

    private void OnCurrentSettingsChanged(object? sender, AppOptions options)
    {
        PostToUiThread(RefreshMachineIdentity);
    }

    private void RefreshUserState()
    {
        IsAuthenticated = AppSession.IsAuthenticated;
        UserDisplayName = AppSession.FullName ?? AppSession.CurrentUserName ?? "Khách Vận Hành";
        AuthActionText = AppSession.IsAuthenticated ? "Đăng Xuất" : "Đăng Nhập";
    }

    private void RefreshMachineIdentity()
    {
        MachineName = AppSettings.Current.MachineName;
        MachineCode = AppSettings.Current.MachineCode;
    }

    private void ApplyAlarmState(AlarmBarState state)
    {
        HasActiveAlarm = state.HasActiveAlarm;
        ActiveAlarmCount = state.ActiveAlarmCount;
        AlarmTickerText = state.TickerText;
    }

    private async Task PersistBuzzerStateAsync(IPlcService plcService, bool isMuted)
    {
        try
        {
            await plcService.WriteAsync(PlcTagCatalog.Manual.BuzzerOnOff.Name, isMuted).ConfigureAwait(false);
        }
        catch
        {
            PostToUiThread(SyncBuzzerStateFromCache);
        }
    }

    private async Task PersistLightCurtainStateAsync(IPlcService plcService, bool isMuted)
    {
        try
        {
            await plcService.WriteAsync(PlcTagCatalog.Manual.LightCurtainOnOff.Name, isMuted).ConfigureAwait(false);
        }
        catch
        {
            PostToUiThread(SyncLightCurtainStateFromCache);
        }
    }

    private void ApplyPlcConnectionState(bool connected)
    {
        IsPlcConnected = connected;
    }

    private void SyncBuzzerStateFromCache()
    {
        if (_plcService is null)
        {
            return;
        }

        ApplyBuzzerState(_plcService.GetValue(PlcTagCatalog.Manual.BuzzerOnOff.Name, false));
    }

    private void ApplyBuzzerState(bool isMuted)
    {
        _isApplyingPlcBuzzerState = true;

        try
        {
            OffBuzzer = isMuted;
        }
        finally
        {
            _isApplyingPlcBuzzerState = false;
        }
    }

    private void SyncLightCurtainStateFromCache()
    {
        if (_plcService is null)
        {
            return;
        }

        ApplyLightCurtainState(_plcService.GetValue(PlcTagCatalog.Manual.LightCurtainOnOff.Name, false));
    }

    private void ApplyLightCurtainState(bool isDisabled)
    {
        _isApplyingPlcLightCurtainState = true;

        try
        {
            OffLightCurtain = isDisabled;
        }
        finally
        {
            _isApplyingPlcLightCurtainState = false;
        }
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
}
