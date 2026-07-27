using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Runtime;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.ViewModels.Pages;

public partial class ManualPageViewModel : ObservableObject, IDisposable
{
    private static readonly IReadOnlyList<PlcTagDefinition> AlarmBitTags = typeof(PlcTagCatalog.Alarms)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(field => field.FieldType == typeof(PlcTagDefinition))
        .Select(field => (PlcTagDefinition?)field.GetValue(null))
        .OfType<PlcTagDefinition>()
        .Where(tag => tag.DataType == PlcTagDataType.Bool)
        .ToList();

    private readonly Dictionary<string, ManualHomeActionState> _originActionsByTagName;
    private readonly object _queueLock = new();
    private readonly IPlcService _plcService;
    private readonly IPlcParameterSettingsService _plcParameterSettingsService;
    private readonly INotificationDialogService _notificationDialog;
    private bool _isDisposed;
    private bool _isInitialized;
    private bool _isInitializing;
    private bool _isSyncQueued;
    private string? _activeJogTagName;

    public ManualPageViewModel(
        IPlcService plcService,
        IPlcParameterSettingsService plcParameterSettingsService,
        INotificationDialogService notificationDialog)
    {
        _plcService = plcService ?? throw new ArgumentNullException(nameof(plcService));
        _plcParameterSettingsService = plcParameterSettingsService ?? throw new ArgumentNullException(nameof(plcParameterSettingsService));
        _notificationDialog = notificationDialog ?? throw new ArgumentNullException(nameof(notificationDialog));

        AxisX = new ManualAxisState(
            "axis_x",
            "Trục X",
            "X -",
            "X +",
            PlcTagCatalog.Manual.MoveXBackward,
            PlcTagCatalog.Manual.MoveXForward,
            PlcTagCatalog.Manual.HomeX,
            PlcTagCatalog.Manual.MoveXToPoint,
            PlcTagCatalog.Manual.ManualSpeedX,
            PlcTagCatalog.Manual.MovePointX,
            PlcTagCatalog.Manual.CurrentPositionX,
            PlcTagCatalog.Manual.IsHomingX,
            PlcTagCatalog.Manual.IsHomedX,
            PlcTagCatalog.Alarms.XLimitNegative,
            PlcTagCatalog.Alarms.XLimitPositive,
            PlcTagCatalog.Outputs.Y0_09AxisXOn);

        AxisY = new ManualAxisState(
            "axis_y",
            "Trục Y",
            "Y -",
            "Y +",
            PlcTagCatalog.Manual.MoveYLeft,
            PlcTagCatalog.Manual.MoveYRight,
            PlcTagCatalog.Manual.HomeY,
            PlcTagCatalog.Manual.MoveYToPoint,
            PlcTagCatalog.Manual.ManualSpeedY,
            PlcTagCatalog.Manual.MovePointY,
            PlcTagCatalog.Manual.CurrentPositionY,
            PlcTagCatalog.Manual.IsHomingY,
            PlcTagCatalog.Manual.IsHomedY,
            PlcTagCatalog.Alarms.YLimitNegative,
            PlcTagCatalog.Alarms.YLimitPositive,
            PlcTagCatalog.Outputs.Y0_10AxisYOn);

        AxisZ = new ManualAxisState(
            "axis_z",
            "Trục Z",
            "Z -",
            "Z +",
            PlcTagCatalog.Manual.MoveZDown,
            PlcTagCatalog.Manual.MoveZUp,
            PlcTagCatalog.Manual.HomeZ,
            PlcTagCatalog.Manual.MoveZToPoint,
            PlcTagCatalog.Manual.ManualSpeedZ,
            PlcTagCatalog.Manual.MovePointZ,
            PlcTagCatalog.Manual.CurrentPositionZ,
            PlcTagCatalog.Manual.IsHomingZ,
            PlcTagCatalog.Manual.IsHomedZ,
            PlcTagCatalog.Alarms.ZLimitNegative,
            PlcTagCatalog.Alarms.ZLimitPositive,
            PlcTagCatalog.Outputs.Y0_11AxisZOn);

        Axes = [AxisX, AxisY, AxisZ];
        foreach (var axis in Axes)
        {
            axis.PropertyChanged += OnAxisPropertyChanged;
        }

        ToolClampCylinder = new ManualCylinderState(
            "tool_clamp",
            "Kẹp tay tool",
            "Điều khiển đóng/mở tay kẹp tool và đọc phản hồi công tắc hành trình.",
            "Kẹp vào",
            "Mở ra",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.ToolClampIn,
            PlcTagCatalog.Manual.ToolClampOut,
            PlcTagCatalog.Manual.ToolClosedSignal,
            PlcTagCatalog.Manual.ToolOpenedSignal);

        RotateCylinder = new ManualCylinderState(
            "rotate",
            "Xy lanh xoay",
            "Điều khiển góc quay 0 và 90 độ cho tool.",
            "Về 0°",
            "Đến 90°",
            "Đã ở 0°",
            "Đã ở 90°",
            PlcTagCatalog.Manual.ToolRotate0,
            PlcTagCatalog.Manual.ToolRotate90,
            PlcTagCatalog.Manual.RotatedTo0Signal,
            PlcTagCatalog.Manual.RotatedTo90Signal);

        ClampCart1Cylinder = new ManualCylinderState(
            "cart_1",
            "Kẹp xe hàng 1",
            "Điều khiển kẹp/mở gá cho xe hàng 1.",
            "Kẹp xe 1",
            "Mở xe 1",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.ClampCart1,
            PlcTagCatalog.Manual.UnclampCart1,
            PlcTagCatalog.Manual.Cart1ClosedSignal,
            PlcTagCatalog.Manual.Cart1OpenedSignal);

        ClampCart2Cylinder = new ManualCylinderState(
            "cart_2",
            "Kẹp xe hàng 2",
            "Điều khiển kẹp/mở gá cho xe hàng 2.",
            "Kẹp xe 2",
            "Mở xe 2",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.ClampCart2,
            PlcTagCatalog.Manual.UnclampCart2,
            PlcTagCatalog.Manual.Cart2ClosedSignal,
            PlcTagCatalog.Manual.Cart2OpenedSignal);

        Cylinders = [ToolClampCylinder, RotateCylinder, ClampCart1Cylinder, ClampCart2Cylinder];

        OriginActions =
        [
            new ManualHomeActionState("Home All", "Về gốc toàn bộ 3 trục và cơ cấu xy lanh.", PlcTagCatalog.Manual.HomeAll.Name),
            new ManualHomeActionState("Home X", "Trục X về vị trí gốc.", PlcTagCatalog.Manual.HomeX.Name),
            new ManualHomeActionState("Home Y", "Trục Y về vị trí gốc.", PlcTagCatalog.Manual.HomeY.Name),
            new ManualHomeActionState("Home Z", "Trục Z về vị trí gốc.", PlcTagCatalog.Manual.HomeZ.Name),
            new ManualHomeActionState("Home Xy Lanh Xoay", "Về gốc cơ cấu xoay 0/90.", PlcTagCatalog.Manual.HomeRotateCylinder.Name),
            new ManualHomeActionState("Home Tay Tool", "Về gốc cơ cấu kẹp tool.", PlcTagCatalog.Manual.HomeToolClampCylinder.Name),
        ];
        _originActionsByTagName = OriginActions.ToDictionary(item => item.CommandTagName, StringComparer.OrdinalIgnoreCase);

        SelectOriginTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Origin));
        SelectAxisTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Axis));
        SelectCylinderTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Cylinder));
        RunOneShotCommand = new AsyncRelayCommand<string?>(ExecuteOneShotAsync, CanExecuteOneShot);
        ApplyAxisSpeedCommand = new AsyncRelayCommand<ManualAxisState?>(ApplyAxisSpeedAsync, CanApplyAxisSpeed);
        WriteMovePointValueCommand = new AsyncRelayCommand<ManualAxisState?>(WriteMovePointValueAsync, CanApplyAxisSpeed);
        MoveAxisToPointCommand = new AsyncRelayCommand<ManualAxisState?>(MoveAxisToPointAsync, CanMoveAxisToPoint);
        StartJogCommand = new AsyncRelayCommand<string?>(StartJogAsync, CanStartJog);
        StopJogCommand = new AsyncRelayCommand<string?>(StopJogAsync, CanStopJog);

        SetSelectedTab(ManualTabType.Origin);
        SyncStatesFromCache(updateTimestamp: false);
        UpdateConnectionState(_plcService.IsConnected);

        _plcService.ConnectionChanged += OnConnectionChanged;
        _plcService.DataUpdated += OnDataUpdated;
    }

    public IReadOnlyList<ManualAxisState> Axes { get; }

    public IReadOnlyList<ManualCylinderState> Cylinders { get; }

    public IReadOnlyList<ManualHomeActionState> OriginActions { get; }

    public ManualAxisState AxisX { get; }

    public ManualAxisState AxisY { get; }

    public ManualAxisState AxisZ { get; }

    public ManualCylinderState ToolClampCylinder { get; }

    public ManualCylinderState RotateCylinder { get; }

    public ManualCylinderState ClampCart1Cylinder { get; }

    public ManualCylinderState ClampCart2Cylinder { get; }

    public IRelayCommand SelectOriginTabCommand { get; }

    public IRelayCommand SelectAxisTabCommand { get; }

    public IRelayCommand SelectCylinderTabCommand { get; }

    public IAsyncRelayCommand<string?> RunOneShotCommand { get; }

    public IAsyncRelayCommand<ManualAxisState?> ApplyAxisSpeedCommand { get; }

    public IAsyncRelayCommand<ManualAxisState?> WriteMovePointValueCommand { get; }

    public IAsyncRelayCommand<ManualAxisState?> MoveAxisToPointCommand { get; }

    public IAsyncRelayCommand<string?> StartJogCommand { get; }

    public IAsyncRelayCommand<string?> StopJogCommand { get; }

    [ObservableProperty]
    private string title = "Chế độ bằng tay";

    [ObservableProperty]
    private string subtitle = "Màn hình thao tác bằng tay cho phép vận hành và kiểm tra máy";

    [ObservableProperty]
    private bool isOriginTabSelected = true;

    [ObservableProperty]
    private bool isAxisTabSelected;

    [ObservableProperty]
    private bool isCylinderTabSelected;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionText = "PLC disconnected";

    [ObservableProperty]
    private string lastUpdatedText = "Waiting for PLC data";

    [ObservableProperty]
    private bool hasActiveAlarm;

    [ObservableProperty]
    private string alarmSummaryText = "Không có alarm đang kích hoạt";

    [ObservableProperty]
    private bool isAnyHoming;

    [ObservableProperty]
    private string homingSummaryText = "Không có lệnh home đang chạy";

    [ObservableProperty]
    private bool hasSevereInterlock;

    [ObservableProperty]
    private string interlockSummaryText = "Liên động an toàn OK";

    [ObservableProperty]
    private string currentModeText = "Không xác định";

    [ObservableProperty]
    private string servoSummaryText = "Servo OFF";

    [ObservableProperty]
    private bool isAutoMode;

    [ObservableProperty]
    private bool isEStopActive;

    [ObservableProperty]
    private bool isLightCurtainActive;

    [ObservableProperty]
    private bool isAirPressureAlarmActive;

    [ObservableProperty]
    private bool isPressureInputHealthy;

    public bool CanIssueCommands => IsConnected && !HasSevereInterlock;

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanIssueCommands));
        RefreshCommandStates();
    }

    partial void OnHasSevereInterlockChanged(bool value)
    {
        OnPropertyChanged(nameof(CanIssueCommands));
        RefreshCommandStates();
    }



    public async Task InitializeAsync()
    {
        if (_isDisposed || _isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _isInitializing = true;

        try
        {
            await LoadAxisLimitsAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Loi", ex.Message);
        }

        await InvokeOnUiThreadAsync(
            () =>
            {
                try
                {
                    SyncStatesFromCache(updateTimestamp: _plcService.IsConnected);
                    UpdateConnectionState(_plcService.IsConnected);
                }
                finally
                {
                    _isInitializing = false;
                }
            });
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _plcService.ConnectionChanged -= OnConnectionChanged;
        _plcService.DataUpdated -= OnDataUpdated;
        foreach (var axis in Axes)
        {
            axis.PropertyChanged -= OnAxisPropertyChanged;
        }

        if (!string.IsNullOrWhiteSpace(_activeJogTagName))
        {
            _ = StopJogAsync(_activeJogTagName);
        }
    }

    private async Task LoadAxisLimitsAsync()
    {
        var fields = await _plcParameterSettingsService.LoadGroupAsync(PlcParameterGroups.DataMachine).ConfigureAwait(false);
        var fieldLookup = fields.ToDictionary(field => field.TagName, StringComparer.OrdinalIgnoreCase);

        AxisX.ApplyLimitProfile(CreateAxisLimitProfile(
            fieldLookup,
            PlcTagCatalog.DataMachine.AxisXSpeedLimit.Name,
            PlcTagCatalog.DataMachine.AxisXNegativeLimit.Name,
            PlcTagCatalog.DataMachine.AxisXPositiveLimit.Name));
        AxisY.ApplyLimitProfile(CreateAxisLimitProfile(
            fieldLookup,
            PlcTagCatalog.DataMachine.AxisYSpeedLimit.Name,
            PlcTagCatalog.DataMachine.AxisYNegativeLimit.Name,
            PlcTagCatalog.DataMachine.AxisYPositiveLimit.Name));
        AxisZ.ApplyLimitProfile(CreateAxisLimitProfile(
            fieldLookup,
            PlcTagCatalog.DataMachine.AxisZSpeedLimit.Name,
            PlcTagCatalog.DataMachine.AxisZNegativeLimit.Name,
            PlcTagCatalog.DataMachine.AxisZPositiveLimit.Name));
    }

    private void OnConnectionChanged(object? sender, bool connected)
    {
        PostToUiThread(
            () =>
            {
                UpdateConnectionState(connected);
                SyncStatesFromCache(updateTimestamp: connected);
            });
    }

    private void OnDataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_isDisposed || _isInitializing)
        {
            return;
        }

        lock (_queueLock)
        {
            if (_isSyncQueued)
            {
                return;
            }

            _isSyncQueued = true;
        }

        PostToUiThread(ProcessQueuedSync);
    }

    private void ProcessQueuedSync()
    {
        lock (_queueLock)
        {
            _isSyncQueued = false;
        }

        SyncStatesFromCache(updateTimestamp: true);
    }

    private void SyncStatesFromCache(bool updateTimestamp)
    {
        EnsureAllJogTagsReleased();

        foreach (var axis in Axes)
        {
            axis.ApplyObservedValues(
                ReadSingle(axis.CurrentPositionTag.Name),
                ReadSingle(axis.ManualSpeedTag.Name),
                ReadSingle(axis.MovePointTag.Name),
                ReadBool(axis.NegativeJogTag.Name),
                ReadBool(axis.PositiveJogTag.Name),
                ReadBool(axis.HomeTag.Name),
                ReadBool(axis.MoveToPointTag.Name),
                ReadBool(axis.IsHomingTag.Name),
                ReadBool(axis.IsHomedTag.Name),
                ReadBool(axis.NegativeLimitAlarmTag.Name),
                ReadBool(axis.PositiveLimitAlarmTag.Name),
                ReadBool(axis.ServoTag.Name));
        }

        foreach (var cylinder in Cylinders)
        {
            cylinder.ApplyObservedValues(
                ReadBool(cylinder.PrimaryCommandTag.Name),
                ReadBool(cylinder.SecondaryCommandTag.Name),
                ReadBool(cylinder.PrimaryFeedbackTag.Name),
                ReadBool(cylinder.SecondaryFeedbackTag.Name));
        }

        SyncOriginActions();
        SyncSummaryStatus();

        if (updateTimestamp && IsConnected)
        {
            LastUpdatedText = $"Last updated {DateTime.Now:HH:mm:ss}";
        }
    }

    /// <summary>
    /// Safety watchdog: mỗi chu kỳ sync, duyệt tất cả jog tag.
    /// Nếu tag nào đang true trong PLC mà không phải nút đang được ấn → ghi false.
    /// Giải quyết tình trạng bị miss Release event hoặc dính nút.
    /// </summary>
    private void EnsureAllJogTagsReleased()
    {
        if (!IsConnected)
        {
            return;
        }

        foreach (var axis in Axes)
        {
            ClearStaleJogTag(axis.NegativeJogTag.Name);
            ClearStaleJogTag(axis.PositiveJogTag.Name);
        }
    }

    private void ClearStaleJogTag(string tagName)
    {
        if (!ReadBool(tagName))
        {
            return;
        }

        if (string.Equals(_activeJogTagName, tagName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Tag đang true trong PLC nhưng không ai ấn → force release
        _ = _plcService.WriteAsync(tagName, false);
    }

    private void SyncOriginActions()
    {
        UpdateOriginAction(
            PlcTagCatalog.Manual.HomeAll.Name,
            IsAnyOriginBusy(),
            AreAllOriginsDone(),
            ReadBool(PlcTagCatalog.Manual.HomeAll.Name));

        UpdateOriginAction(PlcTagCatalog.Manual.HomeX.Name, AxisX.IsHoming || AxisX.IsHomeCommandActive, AxisX.IsHomed, AxisX.IsHomeCommandActive);
        UpdateOriginAction(PlcTagCatalog.Manual.HomeY.Name, AxisY.IsHoming || AxisY.IsHomeCommandActive, AxisY.IsHomed, AxisY.IsHomeCommandActive);
        UpdateOriginAction(PlcTagCatalog.Manual.HomeZ.Name, AxisZ.IsHoming || AxisZ.IsHomeCommandActive, AxisZ.IsHomed, AxisZ.IsHomeCommandActive);
        UpdateOriginAction(
            PlcTagCatalog.Manual.HomeRotateCylinder.Name,
            ReadBool(PlcTagCatalog.Manual.HomeRotateCylinder.Name),
            ReadBool(PlcTagCatalog.Manual.HomeRotateCylinderDone.Name),
            ReadBool(PlcTagCatalog.Manual.HomeRotateCylinder.Name));
        UpdateOriginAction(
            PlcTagCatalog.Manual.HomeToolClampCylinder.Name,
            ReadBool(PlcTagCatalog.Manual.HomeToolClampCylinder.Name),
            ReadBool(PlcTagCatalog.Manual.HomeToolClampDone.Name),
            ReadBool(PlcTagCatalog.Manual.HomeToolClampCylinder.Name));
    }

    private void SyncSummaryStatus()
    {
        var activeAlarmTags = AlarmBitTags.Where(tag => ReadBool(tag.Name)).ToList();
        HasActiveAlarm = activeAlarmTags.Count > 0;
        AlarmSummaryText = HasActiveAlarm
            ? string.Join(" | ", activeAlarmTags.Take(3).Select(tag => tag.Description))
            : "Không có alarm đang kích hoạt";

        IsAutoMode = ReadBool(PlcTagCatalog.Inputs.X1_02.Name);
        CurrentModeText = IsAutoMode ? "Auto" : "Manual / Service";

        IsEStopActive = ReadBool(PlcTagCatalog.Alarms.EStop.Name);
        IsLightCurtainActive = ReadBool(PlcTagCatalog.Alarms.LightCurtain.Name);
        IsAirPressureAlarmActive = ReadBool(PlcTagCatalog.Alarms.AirPressureLost.Name);
        IsPressureInputHealthy = ReadBool(PlcTagCatalog.Inputs.X1_00.Name);

        var interlockMessages = new List<string>();
        if (IsEStopActive)
        {
            interlockMessages.Add("E-Stop");
        }

        if (IsLightCurtainActive)
        {
            interlockMessages.Add("Light curtain");
        }

        if (IsAirPressureAlarmActive)
        {
            interlockMessages.Add("Air pressure");
        }

        HasSevereInterlock = interlockMessages.Count > 0;
        InterlockSummaryText = HasSevereInterlock
            ? $"Khóa thao tác: {string.Join(", ", interlockMessages)}"
            : "Liên động an toàn OK";

        var activeHoming = OriginActions.Where(item => item.IsActive).Select(item => item.Title).ToList();
        IsAnyHoming = activeHoming.Count > 0;
        HomingSummaryText = IsAnyHoming
            ? string.Join(", ", activeHoming.Take(3))
            : "Không có lệnh home đang chạy";

        var activeServos = Axes.Where(axis => axis.IsServoOn).Select(axis => axis.DisplayName).ToList();
        ServoSummaryText = activeServos.Count > 0
            ? $"ON: {string.Join(", ", activeServos)}"
            : "Tất cả servo OFF";

        RefreshCommandStates();
    }

    private void UpdateOriginAction(string commandTagName, bool isActive, bool isDone, bool isCommandActive)
    {
        if (_originActionsByTagName.TryGetValue(commandTagName, out var action))
        {
            action.IsActive = isActive;
            action.IsDone = isDone;
            action.IsCommandActive = isCommandActive;
        }
    }

    private async Task ExecuteOneShotAsync(string? tagName)
    {
        if (!CanExecuteOneShot(tagName))
        {
            return;
        }

        try
        {
            // Cylinder mutual exclusion: clear the opposite command before activating.
            if (TryResolveCylinderCommand(tagName!, out var cylinder, out var isPrimary))
            {
                var oppositeTag = isPrimary
                    ? cylinder.SecondaryCommandTag.Name
                    : cylinder.PrimaryCommandTag.Name;

                await _plcService.WriteAsync(oppositeTag, false).ConfigureAwait(false);
            }

            await _plcService.WriteAsync(tagName!, true).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể ghi lệnh {ResolveCommandName(tagName!)}: {exception.Message}");
        }
    }

    private bool CanExecuteOneShot(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName) || !CanIssueCommands)
        {
            return false;
        }

        if (_originActionsByTagName.TryGetValue(tagName, out var originAction))
        {
            return !originAction.IsCommandActive && !originAction.IsActive;
        }

        if (TryResolveCylinderCommand(tagName, out var cylinder, out var isPrimaryAction))
        {
            return isPrimaryAction
                ? !cylinder.IsPrimaryCommandActive
                : !cylinder.IsSecondaryCommandActive;
        }

        return false;
    }

    private async Task ApplyAxisSpeedAsync(ManualAxisState? axis)
    {
        if (axis is null || !CanIssueCommands)
        {
            return;
        }

        if (!axis.TryGetValidatedManualSpeed(out var speedValue, out _))
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Giá trị tốc độ của {axis.DisplayName} không hợp lệ.");
            return;
        }

        try
        {
            await _plcService.WriteAsync(axis.ManualSpeedTag.Name, speedValue).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() =>
            {
                axis.MarkManualSpeedApplied(speedValue);
                SyncStatesFromCache(updateTimestamp: true);
            });
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể ghi tốc độ manual cho {axis.DisplayName}: {exception.Message}");
        }
    }

    private bool CanApplyAxisSpeed(ManualAxisState? axis)
    {
        return axis is not null
            && CanIssueCommands
            && !axis.HasManualSpeedValidationMessage;
    }

    private async Task WriteMovePointValueAsync(ManualAxisState? axis)
    {
        if (axis is null || !CanApplyAxisSpeed(axis))
        {
            return;
        }

        if (!axis.TryGetValidatedMovePoint(out var movePointValue, out _))
        {
            return;
        }

        try
        {
            await _plcService.WriteAsync(axis.MovePointTag.Name, movePointValue).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() =>
            {
                axis.MarkMovePointApplied(movePointValue);
                SyncStatesFromCache(updateTimestamp: true);
            });
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể ghi điểm chạy cho {axis.DisplayName}: {exception.Message}");
        }
    }

    private async Task MoveAxisToPointAsync(ManualAxisState? axis)
    {
        if (axis is null || !CanMoveAxisToPoint(axis))
        {
            return;
        }

        if (!axis.TryGetValidatedMovePoint(out var movePointValue, out var errorMessage))
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Giá trị điểm chạy của {axis.DisplayName} không hợp lệ.");
            return;
        }

        try
        {
            await _plcService.WriteAsync(axis.MovePointTag.Name, movePointValue).ConfigureAwait(false);
            await _plcService.WriteAsync(axis.MoveToPointTag.Name, true).ConfigureAwait(false);

            await InvokeOnUiThreadAsync(
                () =>
                {
                    axis.MarkMovePointApplied(movePointValue);
                    SyncStatesFromCache(updateTimestamp: true);
                });
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể chạy tới điểm cho {axis.DisplayName}: {exception.Message}");
        }
    }

    private bool CanMoveAxisToPoint(ManualAxisState? axis)
    {
        return axis is not null
            && CanIssueCommands
            && !axis.IsMoveToPointCommandActive
            && !axis.IsHomeCommandActive
            && !axis.IsHoming
            && !axis.HasMovePointValidationMessage;
    }

    private async Task StartJogAsync(string? tagName)
    {
        if (!CanStartJog(tagName))
        {
            return;
        }

        // Set active tag BEFORE writing to PLC — prevents the watchdog
        // (EnsureAllJogTagsReleased) from clearing it during the write-to-read gap.
        _activeJogTagName = tagName;

        try
        {
            await _plcService.WriteAsync(tagName!, true).ConfigureAwait(false);

            await InvokeOnUiThreadAsync(
                () => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            _activeJogTagName = null;
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể jog {ResolveCommandName(tagName!)}: {exception.Message}");
        }
    }

    private bool CanStartJog(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName) || !CanIssueCommands || !string.IsNullOrWhiteSpace(_activeJogTagName))
        {
            return false;
        }

        var axis = ResolveAxisByJogTag(tagName!);
        if (axis is null)
        {
            return false;
        }

        return string.Equals(axis.NegativeJogTag.Name, tagName, StringComparison.OrdinalIgnoreCase)
            ? axis.CanJogNegative
            : axis.CanJogPositive;
    }

    private async Task StopJogAsync(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return;
        }

        try
        {
            if (IsConnected)
            {
                await _plcService.WriteAsync(tagName, false).ConfigureAwait(false);
            }
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể dừng jog {ResolveCommandName(tagName)}: {exception.Message}");
        }
        finally
        {
            await InvokeOnUiThreadAsync(
                () =>
                {
                    if (string.Equals(_activeJogTagName, tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        _activeJogTagName = null;
                    }

                    SyncStatesFromCache(updateTimestamp: true);
                    RefreshCommandStates();
                });
        }
    }

    private bool CanStopJog(string? tagName)
    {
        return !string.IsNullOrWhiteSpace(tagName)
            && string.Equals(_activeJogTagName, tagName, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateConnectionState(bool connected)
    {
        IsConnected = connected;
        ConnectionText = connected ? "PLC online" : "PLC offline";

        if (!connected)
        {
            LastUpdatedText = "Waiting for PLC data";
        }
    }

    private void SetSelectedTab(ManualTabType tab)
    {
        IsOriginTabSelected = tab == ManualTabType.Origin;
        IsAxisTabSelected = tab == ManualTabType.Axis;
        IsCylinderTabSelected = tab == ManualTabType.Cylinder;
    }

    private bool AreAllOriginsDone()
    {
        return AxisX.IsHomed
            && AxisY.IsHomed
            && AxisZ.IsHomed
            && ReadBool(PlcTagCatalog.Manual.HomeRotateCylinderDone.Name)
            && ReadBool(PlcTagCatalog.Manual.HomeToolClampDone.Name);
    }

    private bool IsAnyOriginBusy()
    {
        return AxisX.IsHoming
            || AxisY.IsHoming
            || AxisZ.IsHoming
            || ReadBool(PlcTagCatalog.Manual.HomeAll.Name)
            || ReadBool(PlcTagCatalog.Manual.HomeRotateCylinder.Name)
            || ReadBool(PlcTagCatalog.Manual.HomeToolClampCylinder.Name);
    }

    private ManualAxisState? ResolveAxisByJogTag(string tagName)
    {
        return Axes.FirstOrDefault(
            axis => string.Equals(axis.NegativeJogTag.Name, tagName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(axis.PositiveJogTag.Name, tagName, StringComparison.OrdinalIgnoreCase));
    }

    private bool TryResolveCylinderCommand(string tagName, out ManualCylinderState cylinder, out bool isPrimaryAction)
    {
        foreach (var item in Cylinders)
        {
            if (string.Equals(item.PrimaryCommandTag.Name, tagName, StringComparison.OrdinalIgnoreCase))
            {
                cylinder = item;
                isPrimaryAction = true;
                return true;
            }

            if (string.Equals(item.SecondaryCommandTag.Name, tagName, StringComparison.OrdinalIgnoreCase))
            {
                cylinder = item;
                isPrimaryAction = false;
                return true;
            }
        }

        cylinder = null!;
        isPrimaryAction = false;
        return false;
    }

    private bool ReadBool(string tagName)
    {
        return _plcService.GetValue(tagName, false);
    }

    private float ReadSingle(string tagName)
    {
        var value = _plcService.GetValue<object?>(tagName, null);
        return value switch
        {
            float floatValue => floatValue,
            double doubleValue => (float)doubleValue,
            int intValue => intValue,
            short shortValue => shortValue,
            long longValue => longValue,
            _ => 0f,
        };
    }

    private string ResolveCommandName(string tagName)
    {
        if (PlcTagCatalog.TryGet(tagName, out var definition))
        {
            return definition.Description;
        }

        return tagName;
    }



    private void RefreshCommandStates()
    {
        RunOneShotCommand.NotifyCanExecuteChanged();
        ApplyAxisSpeedCommand.NotifyCanExecuteChanged();
        WriteMovePointValueCommand.NotifyCanExecuteChanged();
        MoveAxisToPointCommand.NotifyCanExecuteChanged();
        StartJogCommand.NotifyCanExecuteChanged();
        StopJogCommand.NotifyCanExecuteChanged();
    }

    private void OnAxisPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is ManualAxisState)
        {
            RefreshCommandStates();
        }
    }

    private static AxisLimitProfile CreateAxisLimitProfile(
        IReadOnlyDictionary<string, EditablePlcParameterField> fieldLookup,
        string speedTagName,
        string negativeTagName,
        string positiveTagName)
    {
        return new AxisLimitProfile(
            ReadLimit(fieldLookup, speedTagName),
            ReadLimit(fieldLookup, negativeTagName),
            ReadLimit(fieldLookup, positiveTagName));
    }

    private static float? ReadLimit(IReadOnlyDictionary<string, EditablePlcParameterField> fieldLookup, string tagName)
    {
        return fieldLookup.TryGetValue(tagName, out var field) && ManualNumeric.TryParse(field.ValueText, out var value)
            ? value
            : null;
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

    private static Task InvokeOnUiThreadAsync(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(action, DispatcherPriority.DataBind).Task;
    }
}
