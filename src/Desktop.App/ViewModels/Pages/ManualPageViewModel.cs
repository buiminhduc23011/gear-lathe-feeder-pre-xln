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
            "Lùi (-)",
            "Tiến (+)",
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
            PlcTagCatalog.Outputs.Y0_07);

        AxisZ = new ManualAxisState(
            "axis_z",
            "Trục Z",
            "Xuống (-)",
            "Lên (+)",
            PlcTagCatalog.Manual.MoveZLeft,
            PlcTagCatalog.Manual.MoveZRight,
            PlcTagCatalog.Manual.HomeZ,
            PlcTagCatalog.Manual.MoveZToPoint,
            PlcTagCatalog.Manual.ManualSpeedZ,
            PlcTagCatalog.Manual.MovePointZ,
            PlcTagCatalog.Manual.CurrentPositionZ,
            PlcTagCatalog.Manual.IsHomingZ,
            PlcTagCatalog.Manual.IsHomedZ,
            PlcTagCatalog.Alarms.ZLimitNegative,
            PlcTagCatalog.Alarms.ZLimitPositive,
            PlcTagCatalog.Outputs.Y0_08);

        AxisLifter = new ManualAxisState(
            "axis_lifter",
            "Trục Cấp Phôi",
            "Xuống (-)",
            "Lên (+)",
            PlcTagCatalog.Manual.MoveLifterDown,
            PlcTagCatalog.Manual.MoveLifterUp,
            PlcTagCatalog.Manual.HomeLifter,
            PlcTagCatalog.Manual.MoveLifterToPoint,
            PlcTagCatalog.Manual.ManualSpeedLifter,
            PlcTagCatalog.Manual.MovePointLifter,
            PlcTagCatalog.Manual.CurrentPositionLifter,
            PlcTagCatalog.Manual.IsHomingLifter,
            PlcTagCatalog.Manual.IsHomedLifter,
            PlcTagCatalog.Alarms.LifterHardLimitBottom,
            PlcTagCatalog.Alarms.LifterHardLimitTop,
            PlcTagCatalog.Outputs.Y2_03);

        AxisRotary = new ManualAxisState(
            "axis_rotary",
            "Bàn Xoay",
            "Nghịch (-)",
            "Thuận (+)",
            PlcTagCatalog.Manual.MoveRotaryReverse,
            PlcTagCatalog.Manual.MoveRotaryForward,
            PlcTagCatalog.Manual.HomeRotary,
            PlcTagCatalog.Manual.MoveRotaryToPoint,
            PlcTagCatalog.Manual.ManualSpeedRotary,
            PlcTagCatalog.Manual.MovePointRotary,
            PlcTagCatalog.Manual.CurrentPositionRotary,
            PlcTagCatalog.Manual.IsHomingRotary,
            PlcTagCatalog.Manual.IsHomedRotary,
            PlcTagCatalog.Alarms.RotaryNotAtHome,
            PlcTagCatalog.Alarms.RotaryPulseSlip,
            PlcTagCatalog.Outputs.Y1_14,
            hasLimits: false);

        AxisY = AxisZ;

        Axes = [AxisX, AxisZ, AxisLifter, AxisRotary];
        foreach (var axis in Axes)
        {
            axis.PropertyChanged += OnAxisPropertyChanged;
        }

        ClampCartCylinder = new ManualCylinderState(
            "clamp_cart",
            "XL Kẹp Xe",
            "Điều khiển kẹp/mở gá cho xe hàng.",
            "Kẹp Xe",
            "Mở Xe",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.ClampCart,
            PlcTagCatalog.Manual.UnclampCart,
            PlcTagCatalog.Manual.CartClampedSignal,
            PlcTagCatalog.Manual.CartUnclampedSignal);

        LiftMotorCylinder = new ManualCylinderState(
            "lift_motor",
            "XL Nâng ĐC Bàn Xoay",
            "Điều khiển nâng/hạ xilanh động cơ bàn xoay.",
            "Nâng ĐC",
            "Hạ ĐC",
            "Đã nâng",
            "Đã hạ",
            PlcTagCatalog.Manual.LiftMotorUp,
            PlcTagCatalog.Manual.LiftMotorDown,
            PlcTagCatalog.Manual.LiftMotorUpSignal,
            PlcTagCatalog.Manual.LiftMotorDownSignal);

        InputClampCylinder = new ManualCylinderState(
            "input_clamp",
            "XL Kẹp Phôi Đầu Vào",
            "Điều khiển kẹp/mở phôi ở cụm đầu vào.",
            "Kẹp Phôi",
            "Mở Phôi",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.InputClampPart,
            PlcTagCatalog.Manual.InputUnclampPart,
            PlcTagCatalog.Manual.InputClampedSignal,
            PlcTagCatalog.Manual.InputUnclampedSignal);

        InputFlipCylinder = new ManualCylinderState(
            "input_flip",
            "XL Lật Phôi Đầu Vào",
            "Điều khiển xoay 0° và 90° lật phôi đầu vào.",
            "Xoay 0°",
            "Xoay 90°",
            "Đã xoay 0°",
            "Đã xoay 90°",
            PlcTagCatalog.Manual.InputRotate0,
            PlcTagCatalog.Manual.InputRotate90,
            PlcTagCatalog.Manual.InputRotated0Signal,
            PlcTagCatalog.Manual.InputRotated90Signal);

        RodalArmCylinder = new ManualCylinderState(
            "rodal_arm",
            "Tay Cấp Phôi Rodal",
            "Điều khiển xoay 0° và 180° cụm cấp phôi Rodal.",
            "Xoay 0°",
            "Xoay 180°",
            "Đã xoay 0°",
            "Đã xoay 180°",
            PlcTagCatalog.Manual.RodalRotate0,
            PlcTagCatalog.Manual.RodalRotate180,
            PlcTagCatalog.Manual.RodalRotated0Signal,
            PlcTagCatalog.Manual.RodalRotated180Signal);

        Lathe2FlipCylinder = new ManualCylinderState(
            "lathe2_flip",
            "XL Lật Sau Máy Tiện 2",
            "Điều khiển quay 0° và 90° lật phôi sau tiện 2.",
            "Quay 0°",
            "Quay 90°",
            "Đã xoay 0°",
            "Đã quay 90°",
            PlcTagCatalog.Manual.Lathe2FlipRotate0,
            PlcTagCatalog.Manual.Lathe2FlipRotate90,
            PlcTagCatalog.Manual.Lathe2FlipRotated0Signal,
            PlcTagCatalog.Manual.Lathe2FlipRotated90Signal);

        Lathe2TransferCylinder = new ManualCylinderState(
            "lathe2_transfer",
            "XL Transfer Sau Tiện 2",
            "Điều khiển đi ra và đi vào xilanh transfer sau tiện 2.",
            "Đi Ra",
            "Đi Vào",
            "Đã ra",
            "Đã vào",
            PlcTagCatalog.Manual.Lathe2TransferOut,
            PlcTagCatalog.Manual.Lathe2TransferIn,
            PlcTagCatalog.Manual.Lathe2TransferOutSignal,
            PlcTagCatalog.Manual.Lathe2TransferInSignal);

        ProductOutCylinder = new ManualCylinderState(
            "product_out",
            "XL Out Phôi Thành Phẩm",
            "Điều khiển đi ra và đi vào xilanh out thành phẩm.",
            "Đi Ra",
            "Đi Vào",
            "Đã ra",
            "Đã vào",
            PlcTagCatalog.Manual.ProductOutExtend,
            PlcTagCatalog.Manual.ProductOutRetract,
            PlcTagCatalog.Manual.ProductOutExtendedSignal,
            PlcTagCatalog.Manual.ProductOutRetractedSignal);

        ProductClampCylinder = new ManualCylinderState(
            "product_clamp",
            "XL Kẹp Phôi Thành Phẩm",
            "Điều khiển kẹp/mở phôi thành phẩm.",
            "Kẹp Phôi",
            "Mở Phôi",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.ProductClampPart,
            PlcTagCatalog.Manual.ProductUnclampPart,
            PlcTagCatalog.Manual.ProductClampedSignal,
            PlcTagCatalog.Manual.ProductUnclampedSignal);

        OutputMagnetCylinder = new ManualCylinderState(
            "output_magnet_cylinder",
            "XL Trước Nam Châm Cụm Output",
            "Điều khiển xilanh trước nam châm cụm Output đi vào/đi ra.",
            "Đi Vào",
            "Đi Ra",
            "Đã vào",
            "Đã ra",
            PlcTagCatalog.Manual.OutputMagnetCylinderIn,
            PlcTagCatalog.Manual.OutputMagnetCylinderOut,
            PlcTagCatalog.Manual.OutputMagnetCylinderInSignal,
            PlcTagCatalog.Manual.OutputMagnetCylinderOutSignal);

        IntermediateClampCylinder = new ManualCylinderState(
            "intermediate_clamp",
            "XL Kẹp Cụm Trung Gian",
            "Điều khiển xilanh kẹp cụm trung gian đi vào/đi ra.",
            "Đi Vào",
            "Đi Ra",
            "Đã vào",
            "Đã ra",
            PlcTagCatalog.Manual.IntermediateClampIn,
            PlcTagCatalog.Manual.IntermediateClampOut,
            PlcTagCatalog.Manual.IntermediateClampInSignal,
            PlcTagCatalog.Manual.IntermediateClampOutSignal);

        InputCylinders =
        [
            ClampCartCylinder,
            LiftMotorCylinder,
            InputClampCylinder,
            InputFlipCylinder,
        ];

        ToolArmCylinders =
        [
            RodalArmCylinder,
            IntermediateClampCylinder,
        ];

        ToolArmBinaryOutputs =
        [
            new ManualBinaryOutputState("Nam Châm 1 Tay Tool", "Hút/nhả nam châm số 1 trên tay Tool.", PlcTagCatalog.Manual.ToolArmMagnet1, "Hút", "Nhả", "Đang hút", "Đang nhả"),
            new ManualBinaryOutputState("Nam Châm 2 Tay Tool", "Hút/nhả nam châm số 2 trên tay Tool.", PlcTagCatalog.Manual.ToolArmMagnet2, "Hút", "Nhả", "Đang hút", "Đang nhả"),
            new ManualBinaryOutputState("Nam Châm 3 Tay Tool", "Hút/nhả nam châm số 3 trên tay Tool.", PlcTagCatalog.Manual.ToolArmMagnet3, "Hút", "Nhả", "Đang hút", "Đang nhả"),
            new ManualBinaryOutputState("Nam Châm 4 Tay Tool", "Hút/nhả nam châm số 4 trên tay Tool.", PlcTagCatalog.Manual.ToolArmMagnet4, "Hút", "Nhả", "Đang hút", "Đang nhả"),
            new ManualBinaryOutputState("Xì Khí 1 Tay Tool 1", "Bật/tắt xì khí số 1 trên tay Tool 1.", PlcTagCatalog.Manual.ToolArmAir1, "Bật Khí", "Tắt Khí", "Đang xì", "Đã tắt"),
            new ManualBinaryOutputState("Xì Khí 2 Tay Tool 1", "Bật/tắt xì khí số 2 trên tay Tool 1.", PlcTagCatalog.Manual.ToolArmAir2, "Bật Khí", "Tắt Khí", "Đang xì", "Đã tắt"),
        ];

        OutputCylinders =
        [
            Lathe2FlipCylinder,
            Lathe2TransferCylinder,
            ProductOutCylinder,
            ProductClampCylinder,
            OutputMagnetCylinder,
        ];

        OutputBinaryOutputs =
        [
            new ManualBinaryOutputState("Nam Châm 1 Cụm Output", "Hút/nhả nam châm số 1 tại cụm Output.", PlcTagCatalog.Manual.OutputMagnet1, "Hút", "Nhả", "Đang hút", "Đang nhả"),
            new ManualBinaryOutputState("Nam Châm 2 Cụm Output", "Hút/nhả nam châm số 2 tại cụm Output.", PlcTagCatalog.Manual.OutputMagnet2, "Hút", "Nhả", "Đang hút", "Đang nhả"),
        ];

        Lathe1ChuckCylinder = new ManualCylinderState(
            "lathe1_chuck",
            "Chấu Kẹp Máy Tiện 1",
            "Điều khiển kẹp và mở chấu kẹp máy tiện 1.",
            "Kẹp Chấu",
            "Mở Chấu",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.Lathe1ClampChuck,
            PlcTagCatalog.Manual.Lathe1UnclampChuck,
            PlcTagCatalog.Manual.Lathe1ChuckClampedSignal,
            PlcTagCatalog.Manual.Lathe1ChuckUnclampedSignal);

        Lathe2ChuckCylinder = new ManualCylinderState(
            "lathe2_chuck",
            "Chấu Kẹp Máy Tiện 2",
            "Điều khiển kẹp và mở chấu kẹp máy tiện 2.",
            "Kẹp Chấu",
            "Mở Chấu",
            "Đã kẹp",
            "Đã mở",
            PlcTagCatalog.Manual.Lathe2ClampChuck,
            PlcTagCatalog.Manual.Lathe2UnclampChuck,
            PlcTagCatalog.Manual.Lathe2ChuckClampedSignal,
            PlcTagCatalog.Manual.Lathe2ChuckUnclampedSignal);

        LatheCylinders =
        [
            Lathe1ChuckCylinder,
            Lathe2ChuckCylinder,
        ];

        LatheRunActions =
        [
            new ManualLatheRunState("Máy Tiện 1", "Lệnh chạy máy tiện 1 (nhấn nhả).", PlcTagCatalog.Manual.Lathe1Run, PlcTagCatalog.Manual.Lathe1RunningSignal),
            new ManualLatheRunState("Máy Tiện 2", "Lệnh chạy máy tiện 2 (nhấn nhả).", PlcTagCatalog.Manual.Lathe2Run, PlcTagCatalog.Manual.Lathe2RunningSignal),
        ];

        Cylinders =
        [
            ..InputCylinders,
            ..ToolArmCylinders,
            ..OutputCylinders,
            ..LatheCylinders,
        ];

        BinaryOutputs =
        [
            ..ToolArmBinaryOutputs,
            ..OutputBinaryOutputs,
        ];

        OriginActions =
        [
            new ManualHomeActionState("Home Trục X", "Trục X về vị trí gốc.", PlcTagCatalog.Manual.HomeX.Name),
            new ManualHomeActionState("Home Trục Z", "Trục Z về vị trí gốc.", PlcTagCatalog.Manual.HomeZ.Name),
            new ManualHomeActionState("Home Trục Cấp Phôi", "Trục Cấp Phôi về vị trí gốc.", PlcTagCatalog.Manual.HomeLifter.Name),
            new ManualHomeActionState("Home Bàn Xoay", "Bàn Xoay về vị trí gốc.", PlcTagCatalog.Manual.HomeRotary.Name),
        ];
        _originActionsByTagName = OriginActions.ToDictionary(item => item.CommandTagName, StringComparer.OrdinalIgnoreCase);

        SelectOriginTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Origin));
        SelectAxisTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Axis));
        SelectInputClusterTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.InputCluster));
        SelectToolArmClusterTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.ToolArmCluster));
        SelectOutputClusterTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.OutputCluster));
        SelectLatheTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Lathe));
        SelectCylinderTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Cylinder));
        RunLatheCommand = new AsyncRelayCommand<string?>(ExecutePulseAsync, CanExecuteOneShot);
        SelectMagnetTabCommand = new RelayCommand(() => SetSelectedTab(ManualTabType.Magnet));
        RunOneShotCommand = new AsyncRelayCommand<string?>(ExecuteOneShotAsync, CanExecuteOneShot);
        ActivateBinaryOutputCommand = new AsyncRelayCommand<string?>(ActivateBinaryOutputAsync, CanExecuteOneShot);
        DeactivateBinaryOutputCommand = new AsyncRelayCommand<string?>(DeactivateBinaryOutputAsync, CanExecuteOneShot);
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

    public IReadOnlyList<ManualCylinderState> InputCylinders { get; }

    public IReadOnlyList<ManualCylinderState> ToolArmCylinders { get; }

    public IReadOnlyList<ManualBinaryOutputState> ToolArmBinaryOutputs { get; }

    public IReadOnlyList<ManualCylinderState> OutputCylinders { get; }

    public IReadOnlyList<ManualBinaryOutputState> OutputBinaryOutputs { get; }

    public IReadOnlyList<ManualCylinderState> LatheCylinders { get; }

    public IReadOnlyList<ManualLatheRunState> LatheRunActions { get; }

    public IReadOnlyList<ManualBinaryOutputState> BinaryOutputs { get; }

    public IReadOnlyList<ManualHomeActionState> OriginActions { get; }

    public ManualAxisState AxisX { get; }

    public ManualAxisState AxisY { get; }

    public ManualAxisState AxisZ { get; }

    public ManualAxisState AxisLifter { get; }

    public ManualAxisState AxisRotary { get; }

    public ManualCylinderState ClampCartCylinder { get; }

    public ManualCylinderState LiftMotorCylinder { get; }

    public ManualCylinderState InputClampCylinder { get; }

    public ManualCylinderState InputFlipCylinder { get; }

    public ManualCylinderState RodalArmCylinder { get; }

    public ManualCylinderState Lathe2FlipCylinder { get; }

    public ManualCylinderState Lathe2TransferCylinder { get; }

    public ManualCylinderState ProductOutCylinder { get; }

    public ManualCylinderState ProductClampCylinder { get; }

    public ManualCylinderState OutputMagnetCylinder { get; }

    public ManualCylinderState IntermediateClampCylinder { get; }

    public ManualCylinderState Lathe1ChuckCylinder { get; }

    public ManualCylinderState Lathe2ChuckCylinder { get; }

    public ManualCylinderState ToolClampCylinder => InputClampCylinder;

    public ManualCylinderState RotateCylinder => InputFlipCylinder;

    public ManualCylinderState ClampCart1Cylinder => ClampCartCylinder;

    public ManualCylinderState ClampCart2Cylinder => ClampCartCylinder;

    public IRelayCommand SelectOriginTabCommand { get; }

    public IRelayCommand SelectAxisTabCommand { get; }

    public IRelayCommand SelectInputClusterTabCommand { get; }

    public IRelayCommand SelectToolArmClusterTabCommand { get; }

    public IRelayCommand SelectOutputClusterTabCommand { get; }

    public IRelayCommand SelectLatheTabCommand { get; }

    public IRelayCommand SelectCylinderTabCommand { get; }

    public IAsyncRelayCommand<string?> RunLatheCommand { get; }

    public IRelayCommand SelectMagnetTabCommand { get; }

    public IAsyncRelayCommand<string?> RunOneShotCommand { get; }

    public IAsyncRelayCommand<string?> ActivateBinaryOutputCommand { get; }

    public IAsyncRelayCommand<string?> DeactivateBinaryOutputCommand { get; }

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
    private bool isInputClusterTabSelected;

    [ObservableProperty]
    private bool isToolArmClusterTabSelected;

    [ObservableProperty]
    private bool isOutputClusterTabSelected;

    [ObservableProperty]
    private bool isLatheTabSelected;

    [ObservableProperty]
    private bool isCylinderTabSelected;

    [ObservableProperty]
    private bool isMagnetTabSelected;

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

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _plcService.ConnectionChanged -= OnConnectionChanged;
        _plcService.DataUpdated -= OnDataUpdated;
        EnsureAllJogTagsReleased();
    }

    public Task InitializeAsync()
    {
        if (_isInitialized || _isInitializing)
        {
            return Task.CompletedTask;
        }

        _isInitializing = true;

        try
        {
            UpdateConnectionState(_plcService.IsConnected);

            foreach (var axis in Axes)
            {
                axis.ManualSpeedInput = FormatSingle(ReadSingle(axis.ManualSpeedTag.Name));
                axis.MovePointInput = FormatSingle(ReadSingle(axis.MovePointTag.Name));
            }

            SyncStatesFromCache(updateTimestamp: true);
            _isInitialized = true;
        }
        finally
        {
            _isInitializing = false;
        }

        return Task.CompletedTask;
    }

    private static void PostToUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
    }

    private static async Task InvokeOnUiThreadAsync(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        await dispatcher.InvokeAsync(action);
    }

    private void SetSelectedTab(ManualTabType tabType)
    {
        IsOriginTabSelected = tabType == ManualTabType.Origin;
        IsAxisTabSelected = tabType == ManualTabType.Axis;
        IsInputClusterTabSelected = tabType == ManualTabType.InputCluster;
        IsToolArmClusterTabSelected = tabType == ManualTabType.ToolArmCluster;
        IsOutputClusterTabSelected = tabType == ManualTabType.OutputCluster;
        IsLatheTabSelected = tabType == ManualTabType.Lathe;
        IsCylinderTabSelected = tabType == ManualTabType.Cylinder;
        IsMagnetTabSelected = tabType == ManualTabType.Magnet;
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        PostToUiThread(() =>
        {
            UpdateConnectionState(isConnected);
            SyncStatesFromCache(updateTimestamp: true);
        });
    }

    private void OnDataUpdated(object? sender, PlcDataChangedEventArgs eventArgs)
    {
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

        foreach (var output in BinaryOutputs)
        {
            output.IsActive = ReadBool(output.CommandTag.Name);
        }

        foreach (var lathe in LatheRunActions)
        {
            lathe.IsRunning = ReadBool(lathe.RunningFeedbackTag.Name);
        }

        SyncOriginActions();
        SyncSummaryStatus();

        if (updateTimestamp && IsConnected)
        {
            LastUpdatedText = $"Last updated {DateTime.Now:HH:mm:ss}";
        }
    }

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

        _ = _plcService.WriteAsync(tagName, false);
    }

    private void SyncOriginActions()
    {
        UpdateOriginAction(PlcTagCatalog.Manual.HomeX.Name, AxisX.IsHoming || AxisX.IsHomeCommandActive, AxisX.IsHomed, AxisX.IsHomeCommandActive);
        UpdateOriginAction(PlcTagCatalog.Manual.HomeZ.Name, AxisZ.IsHoming || AxisZ.IsHomeCommandActive, AxisZ.IsHomed, AxisZ.IsHomeCommandActive);
        UpdateOriginAction(PlcTagCatalog.Manual.HomeLifter.Name, AxisLifter.IsHoming || AxisLifter.IsHomeCommandActive, AxisLifter.IsHomed, AxisLifter.IsHomeCommandActive);
        UpdateOriginAction(PlcTagCatalog.Manual.HomeRotary.Name, AxisRotary.IsHoming || AxisRotary.IsHomeCommandActive, AxisRotary.IsHomed, AxisRotary.IsHomeCommandActive);
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

    private async Task ExecutePulseAsync(string? tagName)
    {
        if (!CanExecuteOneShot(tagName))
        {
            return;
        }

        try
        {
            await _plcService.WriteAsync(tagName!, true).ConfigureAwait(false);
            await Task.Delay(300).ConfigureAwait(false);
            await _plcService.WriteAsync(tagName!, false).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            await ShowErrorMessageAsync($"Không thể gửi lệnh xung {tagName}", exception).ConfigureAwait(false);
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
            await ShowErrorMessageAsync($"Không thể gửi lệnh {tagName}", exception).ConfigureAwait(false);
        }
    }

    private bool CanExecuteOneShot(string? tagName)
    {
        return CanIssueCommands && !string.IsNullOrWhiteSpace(tagName);
    }

    private Task ActivateBinaryOutputAsync(string? tagName)
    {
        return WriteBinaryOutputAsync(tagName, true);
    }

    private Task DeactivateBinaryOutputAsync(string? tagName)
    {
        return WriteBinaryOutputAsync(tagName, false);
    }

    private async Task WriteBinaryOutputAsync(string? tagName, bool value)
    {
        if (!CanExecuteOneShot(tagName))
        {
            return;
        }

        try
        {
            await _plcService.WriteAsync(tagName!, value).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            await ShowErrorMessageAsync($"Không thể {(value ? "hút/bật" : "nhả/tắt")} {tagName}", exception).ConfigureAwait(false);
        }
    }

    private async Task ApplyAxisSpeedAsync(ManualAxisState? axis)
    {
        if (!CanApplyAxisSpeed(axis))
        {
            return;
        }

        if (!float.TryParse(axis!.ManualSpeedInput, out var parsedValue) || parsedValue < 0)
        {
            axis.ManualSpeedValidationMessage = "Tốc độ phải là số lớn hơn hoặc bằng 0.";
            return;
        }

        axis.ManualSpeedValidationMessage = string.Empty;

        try
        {
            await _plcService.WriteAsync(axis.ManualSpeedTag.Name, parsedValue).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            await ShowErrorMessageAsync($"Không thể cập nhật tốc độ {axis.DisplayName}", exception).ConfigureAwait(false);
        }
    }

    private async Task WriteMovePointValueAsync(ManualAxisState? axis)
    {
        if (!CanApplyAxisSpeed(axis))
        {
            return;
        }

        if (!float.TryParse(axis!.MovePointInput, out var parsedValue))
        {
            axis.MovePointValidationMessage = "Vị trí chạy điểm không hợp lệ.";
            return;
        }

        axis.MovePointValidationMessage = string.Empty;

        try
        {
            await _plcService.WriteAsync(axis.MovePointTag.Name, parsedValue).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            await ShowErrorMessageAsync($"Không thể cập nhật vị trí chạy điểm {axis.DisplayName}", exception).ConfigureAwait(false);
        }
    }

    private async Task MoveAxisToPointAsync(ManualAxisState? axis)
    {
        if (!CanMoveAxisToPoint(axis))
        {
            return;
        }

        try
        {
            if (float.TryParse(axis!.MovePointInput, out var parsedValue))
            {
                await _plcService.WriteAsync(axis.MovePointTag.Name, parsedValue).ConfigureAwait(false);
            }

            await _plcService.WriteAsync(axis.MoveToPointTag.Name, true).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            await ShowErrorMessageAsync($"Không thể chạy {axis.DisplayName} tới điểm", exception).ConfigureAwait(false);
        }
    }

    private bool CanApplyAxisSpeed(ManualAxisState? axis)
    {
        return CanIssueCommands && axis is not null;
    }

    private bool CanMoveAxisToPoint(ManualAxisState? axis)
    {
        return CanIssueCommands && axis is not null;
    }

    private async Task StartJogAsync(string? tagName)
    {
        if (!CanStartJog(tagName))
        {
            return;
        }

        _activeJogTagName = tagName;

        try
        {
            await _plcService.WriteAsync(tagName!, true).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
        catch (Exception exception)
        {
            _activeJogTagName = null;
            await ShowErrorMessageAsync($"Không thể kích hoạt Jog {tagName}", exception).ConfigureAwait(false);
        }
    }

    private bool CanStartJog(string? tagName)
    {
        return CanIssueCommands && !string.IsNullOrWhiteSpace(tagName);
    }

    private async Task StopJogAsync(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return;
        }

        try
        {
            await _plcService.WriteAsync(tagName, false).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await ShowErrorMessageAsync($"Không thể nhả lệnh Jog {tagName}", exception).ConfigureAwait(false);
        }
        finally
        {
            if (string.Equals(_activeJogTagName, tagName, StringComparison.OrdinalIgnoreCase))
            {
                _activeJogTagName = null;
            }

            await InvokeOnUiThreadAsync(() => SyncStatesFromCache(updateTimestamp: true));
        }
    }

    private bool CanStopJog(string? tagName)
    {
        return !string.IsNullOrWhiteSpace(tagName);
    }

    private bool IsAnyOriginBusy()
    {
        return OriginActions.Any(action => action.IsActive);
    }

    private bool AreAllOriginsDone()
    {
        return OriginActions.All(action => action.IsDone);
    }

    private bool TryResolveCylinderCommand(string tagName, out ManualCylinderState cylinder, out bool isPrimary)
    {
        foreach (var candidate in Cylinders)
        {
            if (string.Equals(candidate.PrimaryCommandTag.Name, tagName, StringComparison.OrdinalIgnoreCase))
            {
                cylinder = candidate;
                isPrimary = true;
                return true;
            }

            if (string.Equals(candidate.SecondaryCommandTag.Name, tagName, StringComparison.OrdinalIgnoreCase))
            {
                cylinder = candidate;
                isPrimary = false;
                return true;
            }
        }

        cylinder = null!;
        isPrimary = false;
        return false;
    }

    private void UpdateConnectionState(bool isConnected)
    {
        IsConnected = isConnected;
        ConnectionText = isConnected ? "PLC connected" : "PLC disconnected";
        RefreshCommandStates();
    }

    private void OnAxisPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs eventArgs)
    {
        if (sender is not ManualAxisState axis)
        {
            return;
        }

        if (eventArgs.PropertyName is nameof(ManualAxisState.ManualSpeedInput) or nameof(ManualAxisState.MovePointInput))
        {
            RefreshCommandStates();
        }
    }

    private void RefreshCommandStates()
    {
        RunOneShotCommand.NotifyCanExecuteChanged();
        ActivateBinaryOutputCommand.NotifyCanExecuteChanged();
        DeactivateBinaryOutputCommand.NotifyCanExecuteChanged();
        ApplyAxisSpeedCommand.NotifyCanExecuteChanged();
        WriteMovePointValueCommand.NotifyCanExecuteChanged();
        MoveAxisToPointCommand.NotifyCanExecuteChanged();
        StartJogCommand.NotifyCanExecuteChanged();
        StopJogCommand.NotifyCanExecuteChanged();
    }

    private async Task ShowErrorMessageAsync(string titleText, Exception exception)
    {
        await InvokeOnUiThreadAsync(async () =>
        {
            await _notificationDialog.ShowErrorAsync(
                titleText,
                $"{exception.Message}\n\nKiểm tra lại kết nối PLC và trạng thái liên động an toàn.").ConfigureAwait(false);
        });
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

    private static string FormatSingle(float value)
    {
        return value.ToString("0.##");
    }


}
