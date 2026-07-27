using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Threading;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Data.Repositories;
using Desktop.App.Models.Agv;
using Desktop.App.Models.Tray;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Api;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Plc;
using Desktop.App.Services.Robot;
using Desktop.App.Session;

namespace Desktop.App.Services.Agv;

public class AgvPositionState : INotifyPropertyChanged
{
    public static readonly TimeSpan AutoCallRetryCooldown = TimeSpan.FromSeconds(30);

    private AgvTransferStatus _apiStatus;
    private AgvCallStatus _localStatus;
    private bool _isPlcReady; // MachineReadyForSwap
    private bool _isBusy;
    private int _orderQty;
    private int _ranQty;
    private int _remainingQty;
    private string _statusText = "Sẵn sàng";
    private string _statusBadgeKey = "BadgeInfo";
    private bool _isServerSlotDeclared = true;
    private string _serverSlotWarningText = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AgvPosition Position { get; }
    public AgvCallRecord? ActiveRecord { get; set; }
    public DateTime? NextAutoCallAllowedAtUtc { get; set; }

    public AgvPositionState(AgvPosition position)
    {
        Position = position;
    }

    /// <summary>true nếu slot tương ứng trên server đã có khai báo hàng. false = chưa khai báo → khóa gọi AGV.</summary>
    public bool IsServerSlotDeclared
    {
        get => _isServerSlotDeclared;
        set { if (_isServerSlotDeclared != value) { _isServerSlotDeclared = value; OnPropertyChanged(nameof(IsServerSlotDeclared)); } }
    }

    /// <summary>Text cảnh báo khi slot chưa khai báo hàng trên server.</summary>
    public string ServerSlotWarningText
    {
        get => _serverSlotWarningText;
        set { if (_serverSlotWarningText != value) { _serverSlotWarningText = value; OnPropertyChanged(nameof(ServerSlotWarningText)); } }
    }

    public AgvTransferStatus ApiStatus
    {
        get => _apiStatus;
        set { if (_apiStatus != value) { _apiStatus = value; OnPropertyChanged(nameof(ApiStatus)); } }
    }

    public AgvCallStatus LocalStatus
    {
        get => _localStatus;
        set { if (_localStatus != value) { _localStatus = value; OnPropertyChanged(nameof(LocalStatus)); } }
    }

    public bool IsPlcReady
    {
        get => _isPlcReady;
        set { if (_isPlcReady != value) { _isPlcReady = value; OnPropertyChanged(nameof(IsPlcReady)); } }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); } }
    }

    public int OrderQty
    {
        get => _orderQty;
        set
        {
            if (_orderQty != value)
            {
                _orderQty = value;
                OnPropertyChanged(nameof(OrderQty));
                OnPropertyChanged(nameof(CurrentPickingQty));
            }
        }
    }

    public int RanQty
    {
        get => _ranQty;
        set
        {
            if (_ranQty != value)
            {
                _ranQty = value;
                OnPropertyChanged(nameof(RanQty));
                OnPropertyChanged(nameof(CurrentPickingQty));
            }
        }
    }

    public int RemainingQty
    {
        get => _remainingQty;
        set { if (_remainingQty != value) { _remainingQty = value; OnPropertyChanged(nameof(RemainingQty)); } }
    }

    public string StatusText
    {
        get => _statusText;
        set { if (_statusText != value) { _statusText = value; OnPropertyChanged(nameof(StatusText)); } }
    }

    public string StatusBadgeKey
    {
        get => _statusBadgeKey;
        set { if (_statusBadgeKey != value) { _statusBadgeKey = value; OnPropertyChanged(nameof(StatusBadgeKey)); } }
    }

    // --- Order/Tray tracking ---
    private List<AgvOrderData> _orders = [];
    private int _currentOrderSequence;
    private int _currentItemInOrder;
    private int _shelfLayoutType;
    private int? _activeDeclarationId;
    private bool _awaitingPickReset;
    private bool _completionReported;
    private bool _completionAcknowledged;
    private bool _wasLoadingRequestedByPlc;

    /// <summary>Cờ đánh dấu PLC đã yêu cầu ghi lại dữ liệu (Rising Edge).</summary>
    public bool WasLoadingRequestedByPlc
    {
        get => _wasLoadingRequestedByPlc;
        set { if (_wasLoadingRequestedByPlc != value) { _wasLoadingRequestedByPlc = value; OnPropertyChanged(nameof(WasLoadingRequestedByPlc)); } }
    }

    /// <summary>Loại bố trí tray do AGV gửi: 1=2 nhỏ, 2=2 lớn, 3=nhỏ dưới lớn trên, 4=lớn dưới nhỏ trên.</summary>
    public int ShelfLayoutType
    {
        get => _shelfLayoutType;
        set { if (_shelfLayoutType != value) { _shelfLayoutType = value; OnPropertyChanged(nameof(ShelfLayoutType)); } }
    }

    /// <summary>Danh sách orders trên kệ hiện đang load trên máy.</summary>
    public List<AgvOrderData> Orders
    {
        get => _orders;
        set
        {
            _orders = value;
            OnPropertyChanged(nameof(Orders));
            OnPropertyChanged(nameof(CurrentPickingQty));
        }
    }

    /// <summary>Thứ tự order đang chạy (1-indexed). 0 = chưa/đã xong hết.</summary>
    public int CurrentOrderSequence
    {
        get => _currentOrderSequence;
        set
        {
            if (_currentOrderSequence != value)
            {
                _currentOrderSequence = value;
                OnPropertyChanged(nameof(CurrentOrderSequence));
                OnPropertyChanged(nameof(CurrentPickingQty));
            }
        }
    }

    /// <summary>Số thứ tự con hàng đang gắp trong order hiện tại (1-indexed). 0 = chưa bắt đầu.</summary>
    public int CurrentItemInOrder
    {
        get => _currentItemInOrder;
        set
        {
            if (_currentItemInOrder != value)
            {
                _currentItemInOrder = value;
                OnPropertyChanged(nameof(CurrentItemInOrder));
                OnPropertyChanged(nameof(CurrentPickingQty));
            }
        }
    }

    /// <summary>
    /// Chỉ số con hàng đang gắp trên toàn kệ.
    /// Ví dụ order 1 đã xong 45 con, order 2 đang gắp con thứ 8 => hiển thị 53.
    /// </summary>
    public int CurrentPickingQty
    {
        get
        {
            if (CurrentItemInOrder <= 0)
            {
                return RanQty;
            }

            if (Orders.Count == 0 || CurrentOrderSequence <= 0)
            {
                return Math.Max(RanQty, CurrentItemInOrder);
            }

            var completedBeforeCurrentOrder = Orders
                .Where(order => order.OrderSequence < CurrentOrderSequence)
                .Sum(order => Math.Max(0, order.Quantity));

            return Math.Max(RanQty, completedBeforeCurrentOrder + CurrentItemInOrder);
        }
    }

    public int? ActiveDeclarationId
    {
        get => _activeDeclarationId;
        set { if (_activeDeclarationId != value) { _activeDeclarationId = value; OnPropertyChanged(nameof(ActiveDeclarationId)); } }
    }

    public bool AwaitingPickReset
    {
        get => _awaitingPickReset;
        set { if (_awaitingPickReset != value) { _awaitingPickReset = value; OnPropertyChanged(nameof(AwaitingPickReset)); } }
    }

    public bool CompletionReported
    {
        get => _completionReported;
        set { if (_completionReported != value) { _completionReported = value; OnPropertyChanged(nameof(CompletionReported)); } }
    }

    public bool CompletionAcknowledged
    {
        get => _completionAcknowledged;
        set { if (_completionAcknowledged != value) { _completionAcknowledged = value; OnPropertyChanged(nameof(CompletionAcknowledged)); } }
    }

    // --- Display / AGV Call Flow ---
    private string _currentModelName = "—";
    private string _shelfLayoutDisplayText = "Chưa xác định";
    private bool _isManualCall;
    private bool _isAutoCallPausedLocally;
    private double _autoCallProgressPercent;
    private string _autoCallProgressText = "—";
    private bool _hasActiveCommand;
    private bool _canCallManual = true;
    private bool _canCancel;
    private bool _canLoadManual = true;

    /// <summary>Tên model đang chạy (từ order hiện tại).</summary>
    public string CurrentModelName
    {
        get => _currentModelName;
        set { if (_currentModelName != value) { _currentModelName = value; OnPropertyChanged(nameof(CurrentModelName)); } }
    }

    /// <summary>Text mô tả bố trí kệ: "2 Tray Nhỏ", "Nhỏ + Lớn", v.v.</summary>
    public string ShelfLayoutDisplayText
    {
        get => _shelfLayoutDisplayText;
        set { if (_shelfLayoutDisplayText != value) { _shelfLayoutDisplayText = value; OnPropertyChanged(nameof(ShelfLayoutDisplayText)); } }
    }

    /// <summary>true nếu lệnh đang chạy là manual call.</summary>
    public bool IsManualCall
    {
        get => _isManualCall;
        set { if (_isManualCall != value) { _isManualCall = value; OnPropertyChanged(nameof(IsManualCall)); } }
    }

    /// <summary>Toggle tạm tắt auto-call riêng cho kệ này (runtime only, không lưu DB).</summary>
    public bool IsAutoCallPausedLocally
    {
        get => _isAutoCallPausedLocally;
        set
        {
            if (_isAutoCallPausedLocally != value)
            {
                _isAutoCallPausedLocally = value;
                OnPropertyChanged(nameof(IsAutoCallPausedLocally));
                // Recalculate dependent properties
                OnPropertyChanged(nameof(CanCancel));
            }
        }
    }

    /// <summary>0-100: tiến độ đến ngưỡng auto-call.</summary>
    public double AutoCallProgressPercent
    {
        get => _autoCallProgressPercent;
        set { if (Math.Abs(_autoCallProgressPercent - value) > 0.01) { _autoCallProgressPercent = value; OnPropertyChanged(nameof(AutoCallProgressPercent)); } }
    }

    /// <summary>Text mô tả: "Còn 12/35 → Tự động gọi khi ≤5".</summary>
    public string AutoCallProgressText
    {
        get => _autoCallProgressText;
        set { if (_autoCallProgressText != value) { _autoCallProgressText = value; OnPropertyChanged(nameof(AutoCallProgressText)); } }
    }

    /// <summary>true nếu có lệnh AGV đang chạy (LocalStatus != Pending && != Failed).</summary>
    public bool HasActiveCommand
    {
        get => _hasActiveCommand;
        set
        {
            if (_hasActiveCommand != value)
            {
                _hasActiveCommand = value;
                OnPropertyChanged(nameof(HasActiveCommand));
                // Recalculate dependent properties
                CanCallManual = !value;
                CanCancel = value && IsAutoCallPausedLocally;
            }
        }
    }

    /// <summary>true nếu chưa có lệnh active nào → có thể gọi manual.</summary>
    public bool CanCallManual
    {
        get => _canCallManual;
        set { if (_canCallManual != value) { _canCallManual = value; OnPropertyChanged(nameof(CanCallManual)); } }
    }

    /// <summary>true nếu IsAutoCallPausedLocally == true và đang có lệnh active.</summary>
    public bool CanCancel
    {
        get => _canCancel;
        set { if (_canCancel != value) { _canCancel = value; OnPropertyChanged(nameof(CanCancel)); } }
    }

    public bool CanLoadManual
    {
        get => _canLoadManual;
        set { if (_canLoadManual != value) { _canLoadManual = value; OnPropertyChanged(nameof(CanLoadManual)); } }
    }

    /// <summary>Clear all shelf/order data — called before loading new shelf info.</summary>
    public void ClearShelfData()
    {
        Orders = [];
        CurrentOrderSequence = 0;
        CurrentItemInOrder = 0;
        ShelfLayoutType = 0;
        ActiveDeclarationId = null;
        AwaitingPickReset = false;
        CompletionReported = false;
        CompletionAcknowledged = false;
        CurrentModelName = "—";
        ShelfLayoutDisplayText = "Chưa xác định";
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class AgvBackgroundService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IAgvCallHistoryRepository _repository;
    private readonly IPlcService _plcService;
    private readonly object _apiServiceSync = new();
    private readonly List<AgvTransferApiService> _retiredApiServices = [];
    private AgvTransferApiService _apiService;
    private readonly DispatcherTimer _timer;
    private int _activeApiOperations;
    private bool _isDisposed;
    private bool _isRunningTick;
    private readonly HttpClient _serverApiClient;
    private readonly IShelfOrderCacheRepository _shelfOrderCacheRepository;
    private readonly IRobotCurrentOrderService _currentOrderService;
    private readonly AgvDeclarationProgressTransition _declarationProgressTransition;
    private DateTime _lastRuntimeErrorNotifiedAtUtc = DateTime.MinValue;
    private string? _lastRuntimeErrorMessage;
    private DateTime _lastRuntimeWarningNotifiedAtUtc = DateTime.MinValue;
    private string? _lastRuntimeWarningMessage;
    private DateTime _lastEligibilityCheckUtc = DateTime.MinValue;
    private static readonly TimeSpan EligibilityCheckInterval = TimeSpan.FromSeconds(10);

    public AgvPositionState Position1State { get; } = new(AgvPosition.Position1);
    public AgvPositionState Position2State { get; } = new(AgvPosition.Position2);

    public event EventHandler? StateChanged;
    public event EventHandler<AgvRuntimeErrorEventArgs>? RuntimeError;

    public AgvBackgroundService(
        AgvTransferApiService apiService, 
        IAgvCallHistoryRepository repository, 
        IPlcService plcService,
        IShelfOrderCacheRepository shelfOrderCacheRepository,
        ITrayConfigRepository trayConfigRepository)
        : this(apiService, repository, plcService, shelfOrderCacheRepository, new HttpClient { Timeout = TimeSpan.FromSeconds(10) }, trayConfigRepository)
    {
    }

    internal AgvBackgroundService(
        AgvTransferApiService apiService,
        IAgvCallHistoryRepository repository,
        IPlcService plcService,
        IShelfOrderCacheRepository shelfOrderCacheRepository,
        HttpClient serverApiClient,
        ITrayConfigRepository trayConfigRepository)
    {
        _apiService = apiService;
        _repository = repository;
        _plcService = plcService;
        _shelfOrderCacheRepository = shelfOrderCacheRepository;
        _serverApiClient = serverApiClient;
        var trayPlacementResolver = new RobotTrayPlacementResolver(trayConfigRepository);
        var jigTypeResolver = new RobotJigTypeResolver();
        var currentOrderParameterWriter = new RobotCurrentOrderParameterWriter(plcService, trayPlacementResolver, jigTypeResolver);
        _currentOrderService = new RobotCurrentOrderService(plcService, currentOrderParameterWriter);
        _declarationProgressTransition = new AgvDeclarationProgressTransition(_currentOrderService);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    internal Task ProcessPositionForTestAsync(AgvPosition position)
    {
        return position == AgvPosition.Position1
            ? ProcessPositionAsync(Position1State, PlcTagCatalog.Agv.MachineReadyForSwapLine1, PlcTagCatalog.DataAutos.OrderLine1Quantity, PlcTagCatalog.DataAutos.RanQuantityOrderLine1, PlcTagCatalog.DataAutos.OrderLine1CurrentPickIndex, PlcTagCatalog.DataAutos.OrderLine1IsLoading)
            : ProcessPositionAsync(Position2State, PlcTagCatalog.Agv.MachineReadyForSwapLine2, PlcTagCatalog.DataAutos.OrderLine2Quantity, PlcTagCatalog.DataAutos.RanQuantityOrderLine2, PlcTagCatalog.DataAutos.OrderLine2CurrentPickIndex, PlcTagCatalog.DataAutos.OrderLine2IsLoading);
    }

    public void Reconfigure(AppOptions options)
    {
        var newApiService = new AgvTransferApiService(options);
        bool shouldRestart = _timer.IsEnabled;
        _timer.Stop();

        lock (_apiServiceSync)
        {
            if (_isDisposed)
            {
                newApiService.Dispose();
                return;
            }

            _retiredApiServices.Add(_apiService);
            _apiService = newApiService;
        }

        TryDisposeRetiredApiServices();

        if (shouldRestart)
        {
            _timer.Start();
        }
    }

    public void AttachModelProfileApiClient(IModelProfileApiClient modelProfileApiClient)
    {
        _currentOrderService.AttachModelProfileApiClient(modelProfileApiClient);
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        if (_isRunningTick || _isDisposed) return;
        _isRunningTick = true;

        try
        {
            await ProcessPositionAsync(Position1State, PlcTagCatalog.Agv.MachineReadyForSwapLine1, PlcTagCatalog.DataAutos.OrderLine1Quantity, PlcTagCatalog.DataAutos.RanQuantityOrderLine1, PlcTagCatalog.DataAutos.OrderLine1CurrentPickIndex, PlcTagCatalog.DataAutos.OrderLine1IsLoading);
            await ProcessPositionAsync(Position2State, PlcTagCatalog.Agv.MachineReadyForSwapLine2, PlcTagCatalog.DataAutos.OrderLine2Quantity, PlcTagCatalog.DataAutos.RanQuantityOrderLine2, PlcTagCatalog.DataAutos.OrderLine2CurrentPickIndex, PlcTagCatalog.DataAutos.OrderLine2IsLoading);

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AgvBackgroundService] Timer tick error: {ex}");
            NotifyRuntimeError($"Lỗi xử lý AGV tự động: {ex.Message}", ex);
        }
        finally
        {
            _isRunningTick = false;
        }
    }

    #pragma warning disable CS0162
    private async Task ProcessPositionAsync(
        AgvPositionState state,
        PlcTagDefinition readyTag,
        PlcTagDefinition orderQtyTag,
        PlcTagDefinition ranQtyTag,
        PlcTagDefinition currentPickIndexTag,
        PlcTagDefinition isLoadingTag)
    {
        // 0. Proactively handle PLC request to rewrite parameters (Handshake: PLC sets IsLoading = True)
        await HandlePlcLoadingRequestAsync(state, isLoadingTag);

        // 1. Read PLC current-order progress, then project it to shelf-level quantities.
        var currentOrderQtyFromPlc = _plcService.GetValue<int>(orderQtyTag.Name);
        var currentRanQtyFromPlc = _plcService.GetValue<int>(ranQtyTag.Name);
        var currentPickIndexFromPlc = _plcService.GetValue<int>(currentPickIndexTag.Name);
        UpdateShelfQuantities(state, currentOrderQtyFromPlc, currentRanQtyFromPlc, currentPickIndexFromPlc);

        // 2. Read PLC Ready Flag (Machine ready for swap)
        state.IsPlcReady = _plcService.GetValue<bool>(readyTag.Name);
        var orderCompletedTag = state.Position == AgvPosition.Position1
            ? PlcTagCatalog.DataAutos.CurrentOrderCompleted
            : PlcTagCatalog.DataAutos.CurrentOrderCompletedLine2;
        var isOrderCompleted = _plcService.GetValue<bool>(orderCompletedTag.Name);
        var orderSequenceBeforeTransition = state.CurrentOrderSequence;
        var orderBeforeTransition = _declarationProgressTransition.ResolveCurrentOrder(state);
        AgvDeclarationProgressTransitionResult transitionResult;
        try
        {
            transitionResult = await _declarationProgressTransition.HandleAsync(
                state,
                isOrderCompleted,
                (id, slot, ev, order) => PostMachineEventAsync(id, slot, ev, order),
                SaveStateCacheAsync);
        }
        catch (InactiveModelProfileException ex)
        {
            ReportInactiveModelWriteBlocked(state, ex);
            return;
        }
        if (transitionResult == AgvDeclarationProgressTransitionResult.ShelfCompleted)
        {
            await _currentOrderService.SetShelfOrdersCompletedAsync(state.Position, true);
            await ClearPositionAsync(state.Position, sendClearEvent: false, setClearRequested: false);
        }
        else
        {
            UpdateShelfQuantities(
                state,
                currentOrderQtyFromPlc,
                state.CurrentOrderSequence == orderSequenceBeforeTransition ? currentRanQtyFromPlc : 0,
                state.CurrentOrderSequence == orderSequenceBeforeTransition ? currentPickIndexFromPlc : 0);
            if (state.ActiveDeclarationId.HasValue && state.Orders.Count > 0)
            {
                await SaveStateCacheAsync(state);
            }
        }

        // 2a. Periodic eligibility check — proactively detect undeclared slots
        if (transitionResult == AgvDeclarationProgressTransitionResult.OrderCompletionPostFailed)
        {
            ReportOrderCompletedRetryWarning(state, orderBeforeTransition);
            return;
        }

        await CheckAndUpdateSlotDeclarationStatusAsync(state);

        // 2b. Update computed state flags
        state.HasActiveCommand = state.LocalStatus != AgvCallStatus.Pending
                              && state.LocalStatus != AgvCallStatus.Failed;
        UpdateManualLoadAvailability(state);
        UpdateAutoCallProgress(state);

        // 3. Auto-Call Evaluation (only if idle and enabled)
        if (state.LocalStatus == AgvCallStatus.Pending || state.LocalStatus == AgvCallStatus.Failed)
        {
            // Block auto-call when server slot is not declared
            if (!state.IsServerSlotDeclared)
            {
                UpdateBadgeDisplay(state);
            }
            else
            {
                bool isAutoCallEnabled = AppSettings.Current.AgvAutoCallEnabled;
                int threshold = GetAutoCallThreshold(state);
                bool autoCallCooldownElapsed = !state.NextAutoCallAllowedAtUtc.HasValue || DateTime.UtcNow >= state.NextAutoCallAllowedAtUtc.Value;

                if (isAutoCallEnabled
                    && !state.IsAutoCallPausedLocally
                    && !state.HasActiveCommand
                    && state.OrderQty > 0
                    && state.RemainingQty <= threshold
                    && autoCallCooldownElapsed)
                {
                    // Trigger auto call
                    await CallAgvAsync(state, isAutoCall: true);
                }
                else
                {
                    UpdateBadgeDisplay(state);
                }
            }
        }
        else if (state.LocalStatus == AgvCallStatus.InProgress
                 || state.LocalStatus == AgvCallStatus.Calling
                 || state.LocalStatus == AgvCallStatus.AwaitingCompletion)
        {
            // 4. Update Status if currently active
            if (state.ActiveRecord != null)
            {
                var result = await UseApiServiceAsync(apiService => apiService.CheckStatusAsync(state.Position));
                state.ApiStatus = result.Status;

                if (result.Status == AgvTransferStatus.Ready)
                {
                    // AGV is ready — clear shelf info completely before swap

                    // Write Request to PLC
                    await _currentOrderService.MarkSwapRequestedAsync(state.Position);
                    if (state.LocalStatus == AgvCallStatus.AwaitingCompletion)
                    {
                        state.StatusText = "Đã xác nhận, đang chờ AGV hoàn tất";
                    }
                    else
                    {
                        state.StatusText = "AGV đã đến lấy hàng";
                    }

                    // The next step is Confirmation.
                    // Depending on the PLC logic, either we confirm immediately or wait for manual push.
                    // If auto-call, we can confirm immediately given the machine is ready.
                    if (state.LocalStatus != AgvCallStatus.AwaitingCompletion && state.IsPlcReady)
                    {
                        await ConfirmAgvAsync(state);
                    }
                }
                else if (result.Status == AgvTransferStatus.Completed)
                {
                    await CompleteTransferAsync(state, result.DeclarationId);
                    UpdateBadgeDisplay(state);
                    return;

                    if (!result.DeclarationId.HasValue)
                    {
                        const string message = "AGV trả Completed nhưng thiếu mã khai báo";
                        state.StatusText = message;
                        Trace.WriteLine($"[AgvBackgroundService] Position {state.Position} returned Completed without declarationId.");
                        await FinishCommandAsync(state, message, AgvCallStatus.Failed);
                    }
                    else
                    {
                        var machineLoad = await FetchMachineLoadAsync(result.DeclarationId.Value);
                        if (machineLoad == null)
                        {
                            var message = $"Không tải được thông tin kệ cho bản khai báo #{result.DeclarationId.Value}.";
                            state.StatusText = message;
                            Trace.WriteLine($"[AgvBackgroundService] Failed to fetch machine load for declaration {result.DeclarationId.Value}.");
                            await FinishCommandAsync(state, message, AgvCallStatus.Failed);
                        }
                        else if (machineLoad.MachineSlotIndex != (int)state.Position)
                        {
                            var message = $"Bản khai báo #{result.DeclarationId.Value} thuộc kệ {machineLoad.MachineSlotIndex} thay vì kệ {(int)state.Position}.";
                            state.StatusText = message;
                            Trace.WriteLine($"[AgvBackgroundService] {message}");
                            await FinishCommandAsync(state, message, AgvCallStatus.Failed);
                        }
                        else
                        {
                            await LoadDeclarationIntoPositionAsync(state.Position, machineLoad);
                            await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "Loaded");
                            await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "InProduction");
                            await FinishCommandAsync(state, "Hoàn thành nhiệm vụ", AgvCallStatus.Completed);
                        }
                    }
                }
                else
                {
                    state.StatusText = "Đang chạy (HasCommand)";
                }
            }
            UpdateBadgeDisplay(state);
        }
    }
    #pragma warning restore CS0162

    private async Task HandlePlcLoadingRequestAsync(AgvPositionState state, PlcTagDefinition isLoadingTag)
    {
        bool isLoadingRequest = _plcService.GetValue<bool>(isLoadingTag.Name);
        if (isLoadingRequest)
        {
            if (!state.WasLoadingRequestedByPlc)
            {
                state.WasLoadingRequestedByPlc = true;
                Trace.WriteLine($"[AgvBackgroundService] Kệ {(int)state.Position}: PLC yêu cầu nạp lại dữ liệu (IsLoading = True).");
                
                var currentOrder = _declarationProgressTransition.ResolveCurrentOrder(state);
                if (currentOrder != null)
                {
                    var shelfProductCount = state.Orders.Sum(o => Math.Max(0, o.Quantity));
                    var loaded = await TryLoadCurrentOrderToPlcAsync(
                        state,
                        state.ShelfLayoutType,
                        shelfProductCount,
                        state.Orders.Count,
                        currentOrder);
                    if (!loaded)
                    {
                        state.WasLoadingRequestedByPlc = false;
                    }
                }
            }
        }
        else
        {
            state.WasLoadingRequestedByPlc = false;
        }
    }

    private async Task<bool> TryLoadCurrentOrderToPlcAsync(
        AgvPositionState state,
        int shelfLayoutType,
        int shelfProductCount,
        int shelfOrderCount,
        AgvOrderData currentOrder)
    {
        try
        {
            await _currentOrderService.LoadCurrentOrderAsync(
                state.Position,
                shelfProductCount,
                shelfOrderCount,
                shelfLayoutType,
                currentOrder);
            return true;
        }
        catch (InactiveModelProfileException ex)
        {
            ReportInactiveModelWriteBlocked(state, ex, currentOrder);
            return false;
        }
    }

    private static void UpdateShelfQuantities(
        AgvPositionState state,
        int currentOrderQtyFromPlc,
        int currentPickedCountFromPlc,
        int currentPickIndexFromPlc)
    {
        var currentOrderPickedQty = CalculateCurrentOrderPickedQty(state, currentOrderQtyFromPlc, currentPickedCountFromPlc);
        state.CurrentItemInOrder = CalculateCurrentPickDisplayIndex(state, currentOrderQtyFromPlc, currentPickIndexFromPlc);

        if (state.Orders.Count == 0)
        {
            state.OrderQty = Math.Max(0, currentOrderQtyFromPlc);
            state.RanQty = Math.Clamp(currentOrderPickedQty, 0, state.OrderQty);
            state.RemainingQty = Math.Max(0, state.OrderQty - state.RanQty);
            return;
        }

        var shelfProductCount = state.Orders.Sum(order => Math.Max(0, order.Quantity));
        var shelfPickedCount = CalculateShelfPickedQty(state, currentOrderPickedQty);

        state.OrderQty = shelfProductCount;
        state.RanQty = Math.Clamp(shelfPickedCount, 0, shelfProductCount);
        state.RemainingQty = Math.Max(0, shelfProductCount - state.RanQty);
    }

    private static int CalculateCurrentOrderPickedQty(AgvPositionState state, int currentOrderQtyFromPlc, int currentPickedCountFromPlc)
    {
        var currentOrderQty = state.Orders
            .FirstOrDefault(order => order.OrderSequence == state.CurrentOrderSequence)
            ?.Quantity ?? currentOrderQtyFromPlc;
        currentOrderQty = Math.Max(0, currentOrderQty);

        if (state.CompletionAcknowledged)
        {
            return currentOrderQty;
        }

        var pickedQty = Math.Max(0, currentPickedCountFromPlc);
        return Math.Clamp(pickedQty, 0, currentOrderQty);
    }

    private static int CalculateCurrentPickDisplayIndex(AgvPositionState state, int currentOrderQtyFromPlc, int currentPickIndexFromPlc)
    {
        var currentOrderQty = state.Orders
            .FirstOrDefault(order => order.OrderSequence == state.CurrentOrderSequence)
            ?.Quantity ?? currentOrderQtyFromPlc;
        currentOrderQty = Math.Max(0, currentOrderQty);

        if (state.CompletionAcknowledged || currentOrderQty == 0)
        {
            return 0;
        }

        var currentPickIndex = Math.Max(0, currentPickIndexFromPlc);
        return Math.Clamp(currentPickIndex, 0, currentOrderQty);
    }

    private static int CalculateShelfPickedQty(AgvPositionState state, int currentOrderPickedQty)
    {
        if (state.Orders.Count == 0 || state.CurrentOrderSequence <= 0)
        {
            return currentOrderPickedQty;
        }

        var pickedCount = 0;
        foreach (var order in state.Orders.OrderBy(order => order.OrderSequence))
        {
            var orderQty = Math.Max(0, order.Quantity);
            if (order.OrderSequence < state.CurrentOrderSequence)
            {
                pickedCount += orderQty;
                continue;
            }

            if (order.OrderSequence == state.CurrentOrderSequence)
            {
                pickedCount += Math.Clamp(currentOrderPickedQty, 0, orderQty);
            }

            break;
        }

        return pickedCount;
    }

    private static int GetAutoCallThreshold(AgvPositionState state)
    {
        return state.Position == AgvPosition.Position1
            ? AppSettings.Current.AgvKe1AutoCallRemainingBelow
            : AppSettings.Current.AgvKe2AutoCallRemainingBelow;
    }

    /// <summary>Tính progress bar auto-call dựa trên số lượng còn lại trên toàn bộ kệ.</summary>
    private void UpdateAutoCallProgress(AgvPositionState state)
    {
        int threshold = GetAutoCallThreshold(state);

        if (state.OrderQty <= 0 || state.HasActiveCommand)
        {
            state.AutoCallProgressPercent = 0;
            state.AutoCallProgressText = state.HasActiveCommand ? "Đang có lệnh AGV" : "—";
            return;
        }

        if (state.IsAutoCallPausedLocally)
        {
            UpdateAutoCallProgressValues(state, threshold);
            state.AutoCallProgressText = $"⏸ Tạm tắt · Còn {state.RemainingQty}/{state.OrderQty} trên kệ (ngưỡng ≤{threshold})";
            return;
        }

        UpdateAutoCallProgressValues(state, threshold);
        state.AutoCallProgressText = state.RemainingQty <= threshold
            ? $"Đã đạt ngưỡng! Còn {state.RemainingQty}/{state.OrderQty} trên kệ"
            : $"Còn {state.RemainingQty}/{state.OrderQty} trên kệ → Tự động gọi khi ≤{threshold}";
    }

    private static void UpdateAutoCallProgressValues(AgvPositionState state, int threshold)
    {
        int totalRange = state.OrderQty - threshold;
        if (totalRange <= 0)
        {
            state.AutoCallProgressPercent = 100;
            return;
        }

        int totalConsumed = state.OrderQty - state.RemainingQty;
        state.AutoCallProgressPercent = Math.Clamp((double)totalConsumed / totalRange * 100, 0, 100);
    }

    public async Task CallManualAsync(AgvPosition position)
    {
        var state = position == AgvPosition.Position1 ? Position1State : Position2State;
        if (state.HasActiveCommand) return; // Guard: 1 lệnh / 1 kệ
        await CallAgvAsync(state, isAutoCall: false);
    }

    public async Task<ManualLoadResult> LoadPendingManualAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        var state = GetPositionState(position);
        var keLabel = $"Kệ {(int)position}";

        // Guard: block manual load if server slot is not declared
        if (!state.IsServerSlotDeclared)
        {
            var warningMsg = $"{keLabel}: Chưa có khai báo hàng trên server. Không thể Load Manual.";
            state.StatusText = warningMsg;
            NotifyRuntimeWarning($"Cảnh báo AGV: {warningMsg}");
            UpdateBadgeDisplay(state);
            return new ManualLoadResult
            {
                Status = ManualLoadResultStatus.Failed,
                Message = warningMsg
            };
        }

        UpdateManualLoadAvailability(state);
        if (!state.CanLoadManual)
        {
            var message = $"{keLabel} đang có dữ liệu hoặc lệnh AGV. Hãy Xóa kệ trước khi Load Manual.";
            state.StatusText = message;
            UpdateBadgeDisplay(state);
            return new ManualLoadResult
            {
                Status = ManualLoadResultStatus.PositionNotClear,
                Message = message
            };
        }

        try
        {
            var machineLoad = await FetchPendingManualLoadAsync(position, cancellationToken);
            if (machineLoad is null)
            {
                var message = $"Không có kệ manual chờ cho {keLabel}.";
                state.StatusText = message;
                UpdateBadgeDisplay(state);
                return new ManualLoadResult
                {
                    Status = ManualLoadResultStatus.NoPendingDeclaration,
                    Message = message
                };
            }

            if (machineLoad.MachineSlotIndex != (int)position)
            {
                var message = $"Bản khai báo #{machineLoad.DeclarationId} thuộc kệ {machineLoad.MachineSlotIndex} thay vì kệ {(int)position}.";
                state.StatusText = message;
                UpdateBadgeDisplay(state);
                return new ManualLoadResult
                {
                    Status = ManualLoadResultStatus.Failed,
                    Message = message,
                    DeclarationId = machineLoad.DeclarationId
                };
            }

            var loaded = await LoadDeclarationIntoPositionAsync(position, machineLoad);
            if (!loaded)
            {
                return new ManualLoadResult
                {
                    Status = ManualLoadResultStatus.BlockedByInactiveModel,
                    Message = state.StatusText,
                    DeclarationId = machineLoad.DeclarationId
                };
            }

            await _currentOrderService.MarkSwapCompletedAsync(position);
            await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "Loaded");
            await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "InProduction");

            var successMessage = $"Đã load manual cho {keLabel}.";
            state.StatusText = successMessage;
            UpdateManualLoadAvailability(state);
            UpdateBadgeDisplay(state);

            return new ManualLoadResult
            {
                Status = ManualLoadResultStatus.Success,
                Message = successMessage,
                DeclarationId = machineLoad.DeclarationId
            };
        }
        catch (Exception ex)
        {
            var message = $"Không thể load manual cho {keLabel}: {ex.Message}";
            Trace.WriteLine($"[AgvBackgroundService] {message}");
            state.StatusText = message;
            UpdateBadgeDisplay(state);
            return new ManualLoadResult
            {
                Status = ManualLoadResultStatus.Failed,
                Message = message
            };
        }
    }

    private async Task CallAgvAsync(AgvPositionState state, bool isAutoCall)
    {
        if (state.IsBusy || state.HasActiveCommand) return;

        // Guard: block if server slot is not declared
        if (!state.IsServerSlotDeclared)
        {
            var keLabel = $"Kệ {(int)state.Position}";
            var warningMsg = $"{keLabel}: Chưa có khai báo hàng trên server. Không thể gọi AGV.";
            state.StatusText = warningMsg;
            state.LocalStatus = AgvCallStatus.Failed;
            NotifyRuntimeWarning($"Cảnh báo AGV: {warningMsg}");
            UpdateBadgeDisplay(state);
            return;
        }

        state.IsBusy = true;
        state.IsManualCall = !isAutoCall;
        state.LocalStatus = AgvCallStatus.Calling;
        try
        {
        state.StatusText = "Đang gọi AGV...";
        UpdateBadgeDisplay(state);

        var eligibility = await CheckAgvCallEligibilityAsync(state.Position);
        if (eligibility is not null && !eligibility.HasActiveDeclaration)
        {
            var blockedMessage = BuildAgvCallBlockedMessage(eligibility);
            var blockedRecord = new AgvCallRecord
            {
                Position = state.Position,
                Status = AgvCallStatus.Failed,
                IsAutoCall = isAutoCall,
                CreatedAtUtc = DateTime.UtcNow,
                StartedAtUtc = DateTime.UtcNow,
                RemainingQty = state.RemainingQty,
                Note = blockedMessage
            };

            await _repository.SaveAsync(blockedRecord);
            state.LocalStatus = AgvCallStatus.Failed;
            state.NextAutoCallAllowedAtUtc = DateTime.UtcNow.Add(AgvPositionState.AutoCallRetryCooldown);
            state.StatusText = blockedMessage;
            NotifyRuntimeWarning($"Canh bao AGV: {blockedMessage}");
            return;
        }

        var record = new AgvCallRecord
        {
            Position = state.Position,
            Status = AgvCallStatus.InProgress,
            IsAutoCall = isAutoCall,
            CreatedAtUtc = DateTime.UtcNow,
            StartedAtUtc = DateTime.UtcNow,
            RemainingQty = state.RemainingQty
        };

        var caller = await UseApiServiceAsync(apiService => apiService.CreateCommandAsync(state.Position));
        if (caller.Success)
        {
            await _repository.SaveAsync(record);
            state.ActiveRecord = record;
            state.LocalStatus = AgvCallStatus.InProgress;
            state.NextAutoCallAllowedAtUtc = null;
            state.StatusText = "Đã nhận lệnh";
        }
        else
        {
            record.Status = AgvCallStatus.Failed;
            record.Note = caller.Message;
            await _repository.SaveAsync(record);
            state.LocalStatus = AgvCallStatus.Failed;
            state.NextAutoCallAllowedAtUtc = DateTime.UtcNow.Add(AgvPositionState.AutoCallRetryCooldown);
            state.StatusText = $"Lỗi: {caller.Message}";
        }

        }
        finally
        {
            state.IsBusy = false;
            UpdateBadgeDisplay(state);
        }
    }

    public async Task ConfirmManualAsync(AgvPosition position)
    {
        var state = position == AgvPosition.Position1 ? Position1State : Position2State;
        await ConfirmAgvAsync(state);
    }

    #pragma warning disable CS0162
    private async Task ConfirmAgvAsync(AgvPositionState state)
    {
        if (state.IsBusy || state.ActiveRecord == null) return;
        state.IsBusy = true;
        try
        {

        var result = await UseApiServiceAsync(apiService => apiService.ConfirmCommandAsync(state.Position));
        if (result.Success)
        {
            state.LocalStatus = AgvCallStatus.AwaitingCompletion;
            state.StatusText = "Đã xác nhận, đang chờ AGV hoàn tất";
            state.ActiveRecord.Status = AgvCallStatus.AwaitingCompletion;
            state.ActiveRecord.Note = state.StatusText;
            await _repository.UpdateAsync(state.ActiveRecord);
            return;

            // Re-check status to get declarationId, then pull shelf details from Server.Api
            var statusResult = await UseApiServiceAsync(apiService => apiService.CheckStatusAsync(state.Position));
            if (statusResult.Status == AgvTransferStatus.Completed)
            {
                if (!statusResult.DeclarationId.HasValue)
                {
                    state.StatusText = "AGV trả Completed nhưng thiếu mã khai báo";
                    state.ActiveRecord.Note = "AGV trả status = 3 nhưng thiếu declarationId.";
                    await _repository.UpdateAsync(state.ActiveRecord);
                    return;
                }

                var machineLoad = await FetchMachineLoadAsync(statusResult.DeclarationId.Value);
                if (machineLoad == null)
                {
                    state.StatusText = $"Không tải được thông tin kệ cho bản khai báo #{statusResult.DeclarationId.Value}.";
                    state.ActiveRecord.Note = state.StatusText;
                    await _repository.UpdateAsync(state.ActiveRecord);
                    return;
                }

                if (machineLoad.MachineSlotIndex != (int)state.Position)
                {
                    state.StatusText = $"Bản khai báo #{statusResult.DeclarationId.Value} thuộc kệ {machineLoad.MachineSlotIndex} thay vì kệ {(int)state.Position}.";
                    state.ActiveRecord.Note = state.StatusText;
                    await _repository.UpdateAsync(state.ActiveRecord);
                    return;
                }

                await LoadDeclarationIntoPositionAsync(state.Position, machineLoad);
                await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "Loaded");
                await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "InProduction");
            }

            await FinishCommandAsync(state, "Xác nhận thành công", AgvCallStatus.Completed);
        }
        else
        {
            state.StatusText = $"Xác nhận lỗi: {result.Message}";
            state.ActiveRecord.Note = result.Message;
            await _repository.UpdateAsync(state.ActiveRecord);
        }

        }
        finally
        {
            state.IsBusy = false;
            UpdateBadgeDisplay(state);
        }
    }
    #pragma warning restore CS0162

    private async Task CompleteTransferAsync(AgvPositionState state, int? declarationId)
    {
        if (!declarationId.HasValue)
        {
            const string message = "AGV trả Completed nhưng thiếu mã khai báo";
            state.StatusText = message;
            Trace.WriteLine($"[AgvBackgroundService] Position {state.Position} returned Completed without declarationId.");
            await FinishCommandAsync(state, message, AgvCallStatus.Failed);
            return;
        }

        var machineLoad = await FetchMachineLoadAsync(declarationId.Value);
        if (machineLoad == null)
        {
            var message = $"Không tải được thông tin kệ cho bản khai báo #{declarationId.Value}.";
            state.StatusText = message;
            Trace.WriteLine($"[AgvBackgroundService] Failed to fetch machine load for declaration {declarationId.Value}.");
            await FinishCommandAsync(state, message, AgvCallStatus.Failed);
            return;
        }

        if (machineLoad.MachineSlotIndex != (int)state.Position)
        {
            var message = $"Bản khai báo #{declarationId.Value} thuộc kệ {machineLoad.MachineSlotIndex} thay vì kệ {(int)state.Position}.";
            state.StatusText = message;
            Trace.WriteLine($"[AgvBackgroundService] {message}");
            await FinishCommandAsync(state, message, AgvCallStatus.Failed);
            return;
        }

        if (state.ActiveDeclarationId.HasValue || state.Orders.Count > 0)
        {
            await ClearPositionAsync(state.Position, sendClearEvent: true);
        }

        if (!await LoadDeclarationIntoPositionAsync(state.Position, machineLoad))
        {
            return;
        }

        await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "Loaded");
        await PostMachineEventAsync(machineLoad.DeclarationId, machineLoad.MachineSlotIndex, "InProduction");

        await _currentOrderService.MarkSwapCompletedAsync(state.Position);

        await FinishCommandAsync(state, "Hoàn thành nhiệm vụ", AgvCallStatus.Completed);
    }

    private async Task<bool> ProcessTrayInfoAsync(AgvPosition position, int declarationId, int shelfLayoutType, IReadOnlyList<AgvOrderData> orders)
    {
        if (orders.Count == 0) return false;

        var state = position == AgvPosition.Position1 ? Position1State : Position2State;
        var sortedOrders = orders.OrderBy(order => order.OrderSequence).ToList();
        var currentOrder = sortedOrders.FirstOrDefault();
        if (currentOrder is null)
        {
            return false;
        }

        var shelfProductCount = sortedOrders.Sum(order => Math.Max(0, order.Quantity));
        var loaded = await TryLoadCurrentOrderToPlcAsync(
            state,
            shelfLayoutType,
            shelfProductCount,
            sortedOrders.Count,
            currentOrder);
        if (!loaded)
        {
            return false;
        }

        state.ActiveDeclarationId = declarationId;
        state.ShelfLayoutType = shelfLayoutType;
        state.Orders = sortedOrders;
        state.CurrentOrderSequence = state.Orders.Count > 0 ? state.Orders[0].OrderSequence : 0;
        state.CurrentItemInOrder = 0;
        state.AwaitingPickReset = false;
        state.CompletionReported = false;
        state.CompletionAcknowledged = false;

        await SaveStateCacheAsync(state);
        return true;
    }

    public async Task<bool> ClearPositionAsync(
        AgvPosition position,
        bool sendClearEvent = true,
        string? clearedByUsername = null,
        bool setClearRequested = true)
    {
        var state = position == AgvPosition.Position1 ? Position1State : Position2State;
        var declarationId = state.ActiveDeclarationId;

        if (sendClearEvent && declarationId.HasValue)
        {
            var success = await PostMachineEventAsync(declarationId.Value, (int)position, "Cleared", actorUsername: clearedByUsername);
            if (!success)
            {
                Trace.WriteLine($"[AgvBackgroundService] ClearPositionAsync: Failed to post Cleared event to server for declaration {declarationId.Value}. Aborting clear.");
                return false;
            }
        }

        await _currentOrderService.ClearCurrentOrderAsync(position, setClearRequested, clearCompletedBit: setClearRequested);
        state.ClearShelfData();
        state.OrderQty = 0;
        state.RanQty = 0;
        state.RemainingQty = 0;

        await _shelfOrderCacheRepository.DeleteAsync((int)position);

        UpdateManualLoadAvailability(state);
        return true;
    }

    internal async Task<AgvCallEligibilityInfo?> CheckAgvCallEligibilityAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        var options = AppSettings.Current;
        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) || string.IsNullOrWhiteSpace(options.MachineCode))
        {
            return null;
        }

        var url =
            $"{options.ApiBaseUrl.TrimEnd('/')}/api/shelf-declarations/agv-call-eligibility?machineCode={Uri.EscapeDataString(options.MachineCode)}&machineSlotIndex={(int)position}";
        try
        {
            return await _serverApiClient.GetFromJsonAsync<AgvCallEligibilityInfo>(url, JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AgvBackgroundService] AGV call eligibility check failed for position {(int)position}: {ex.Message}");
            return null;
        }
    }

    internal async Task<MachineLoadInfo?> FetchMachineLoadAsync(int declarationId, CancellationToken cancellationToken = default)
    {
        var options = AppSettings.Current;
        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) || string.IsNullOrWhiteSpace(options.MachineCode))
        {
            return null;
        }

        var url = $"{options.ApiBaseUrl.TrimEnd('/')}/api/shelf-declarations/{declarationId}/machine-load?machineCode={Uri.EscapeDataString(options.MachineCode)}";
        try
        {
            return await _serverApiClient.GetFromJsonAsync<MachineLoadInfo>(url, JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AgvBackgroundService] Fetch machine load failed for declaration {declarationId}: {ex.Message}");
            return null;
        }
    }

    public async Task<MachineLoadInfo?> FetchActiveLoadAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        var options = AppSettings.Current;
        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) || string.IsNullOrWhiteSpace(options.MachineCode))
        {
            return null;
        }

        var url = $"{options.ApiBaseUrl.TrimEnd('/')}/api/shelf-declarations/active?machineCode={Uri.EscapeDataString(options.MachineCode)}&machineSlotIndex={(int)position}";
        try
        {
            return await _serverApiClient.GetFromJsonAsync<MachineLoadInfo>(url, JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AgvBackgroundService] Fetch active load failed for position {(int)position}: {ex.Message}");
            return null;
        }
    }

    internal async Task<MachineLoadInfo?> FetchPendingManualLoadAsync(AgvPosition position, CancellationToken cancellationToken = default)
    {
        var options = AppSettings.Current;
        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) || string.IsNullOrWhiteSpace(options.MachineCode))
        {
            return null;
        }

        var url = $"{options.ApiBaseUrl.TrimEnd('/')}/api/shelf-declarations/manual-load/pending?machineCode={Uri.EscapeDataString(options.MachineCode)}&machineSlotIndex={(int)position}";
        try
        {
            return await _serverApiClient.GetFromJsonAsync<MachineLoadInfo>(url, JsonOptions, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    internal static List<AgvOrderData> DeserializeOrders(string ordersJson)
    {
        return JsonSerializer.Deserialize<List<AgvOrderData>>(ordersJson, JsonOptions) ?? [];
    }

    internal static AgvPosition GetPositionForMachineSlot(int machineSlotIndex)
    {
        return machineSlotIndex == 2 ? AgvPosition.Position2 : AgvPosition.Position1;
    }

    private bool IsPositionClear(AgvPositionState state)
    {
        return !state.ActiveDeclarationId.HasValue
               && state.Orders.Count == 0
               && state.OrderQty == 0
               && !state.HasActiveCommand;
    }

    internal async Task<bool> LoadDeclarationIntoPositionAsync(AgvPosition position, MachineLoadInfo declaration)
    {
        var orders = DeserializeOrders(declaration.OrdersJson);

        if (orders.Count == 0)
        {
            Trace.WriteLine($"[AgvBackgroundService] Skip load request #{declaration.DeclarationId} because it has no orders.");
            return false;
        }

        var state = position == AgvPosition.Position1 ? Position1State : Position2State;
        state.ClearShelfData();

        if (!await ProcessTrayInfoAsync(position, declaration.DeclarationId, declaration.ShelfLayoutType, orders))
        {
            UpdateManualLoadAvailability(state);
            return false;
        }
        state.StatusText = "Đã nạp kệ xuống máy";
        UpdateManualLoadAvailability(state);
        return true;
    }

    public async Task ResyncCurrentOrdersToPlcAsync()
    {
        await ResyncPositionCurrentOrderAsync(Position1State);
        await ResyncPositionCurrentOrderAsync(Position2State);
    }

    private async Task ResyncPositionCurrentOrderAsync(AgvPositionState state)
    {
        if (!state.ActiveDeclarationId.HasValue || state.Orders.Count == 0)
        {
            return;
        }

        var currentOrder = _declarationProgressTransition.ResolveCurrentOrder(state);
        if (currentOrder is null)
        {
            return;
        }

        var shelfProductCount = state.Orders.Sum(order => Math.Max(0, order.Quantity));
        if (!await TryLoadCurrentOrderToPlcAsync(
                state,
                state.ShelfLayoutType,
                shelfProductCount,
                state.Orders.Count,
                currentOrder))
        {
            return;
        }

        if (state.CompletionAcknowledged)
        {
            await _currentOrderService.SetProductionResultAcknowledgedAsync(state.Position, true);
        }
    }

    private async Task SaveStateCacheAsync(AgvPositionState state)
    {
        await _shelfOrderCacheRepository.SaveAsync(new ShelfOrderCache
        {
            KeIndex = (int)state.Position,
            DeclarationId = state.ActiveDeclarationId,
            CurrentOrderSequence = state.CurrentOrderSequence,
            TotalOrders = state.Orders.Count,
            ShelfLayoutType = state.ShelfLayoutType,
            CurrentItemInOrder = state.CurrentItemInOrder,
            AwaitingPickReset = state.AwaitingPickReset,
            CompletionReported = state.CompletionReported,
            CompletionAcknowledged = state.CompletionAcknowledged,
            OrderQtySnapshot = state.OrderQty,
            RanQtySnapshot = state.RanQty,
            OrdersJson = JsonSerializer.Serialize(state.Orders)
        });
    }

    private async Task<bool> PostMachineEventAsync(
        int declarationId,
        int machineSlotIndex,
        string eventType,
        AgvOrderData? order = null,
        string? actorUsername = null)
    {
        var options = AppSettings.Current;
        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) || string.IsNullOrWhiteSpace(options.MachineCode))
        {
            return false;
        }

        var url = $"{options.ApiBaseUrl.TrimEnd('/')}/api/shelf-declarations/{declarationId}/machine-events";
        try
        {
            var response = await _serverApiClient.PostAsJsonAsync(url, new
            {
                machineCode = options.MachineCode,
                machineSlotIndex,
                eventType,
                orderSequence = order?.OrderSequence,
                orderId = order?.OrderId,
                actorUsername
            });

            if (!response.IsSuccessStatusCode)
            {
                Trace.WriteLine($"[AgvBackgroundService] Machine event '{eventType}' failed for declaration {declarationId}: {(int)response.StatusCode}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AgvBackgroundService] Machine event '{eventType}' failed for declaration {declarationId}: {ex.Message}");
            return false;
        }
    }

    private AgvPositionState GetPositionState(AgvPosition position)
    {
        return position == AgvPosition.Position1 ? Position1State : Position2State;
    }

    public async Task CancelManualAsync(AgvPosition position)
    {
        var state = position == AgvPosition.Position1 ? Position1State : Position2State;
        if (!state.IsAutoCallPausedLocally) return; // Guard: chỉ hủy khi đã pause auto
        if (state.ActiveRecord != null)
        {
            await FinishCommandAsync(state, "Đã hủy bởi người dùng.", AgvCallStatus.Cancelled);
        }
    }

    private async Task FinishCommandAsync(AgvPositionState state, string note, AgvCallStatus newStatus)
    {
        var record = state.ActiveRecord;
        if (record != null)
        {
            record.Status = newStatus;
            record.CompletedAtUtc = DateTime.UtcNow;
            record.Note = note;
            await _repository.UpdateAsync(record);
        }
        state.ActiveRecord = null;
        state.LocalStatus = newStatus == AgvCallStatus.Failed ? AgvCallStatus.Failed : AgvCallStatus.Pending;
        state.StatusText = note;
        UpdateManualLoadAvailability(state);
        UpdateBadgeDisplay(state);
    }

    private void UpdateManualLoadAvailability(AgvPositionState state)
    {
        // Manual load is allowed only when the shelf is clear, auto-call is locally paused, AND server slot is declared.
        state.CanLoadManual = IsPositionClear(state) && state.IsAutoCallPausedLocally && state.IsServerSlotDeclared;
    }

    /// <summary>
    /// Periodically checks the server for slot declaration status and updates state.
    /// Runs at most once per EligibilityCheckInterval to avoid flooding the server.
    /// </summary>
    private async Task CheckAndUpdateSlotDeclarationStatusAsync(AgvPositionState state)
    {
        // Only check when idle (no active AGV command)
        if (state.HasActiveCommand) return;

        var now = DateTime.UtcNow;
        if (now - _lastEligibilityCheckUtc < EligibilityCheckInterval) return;
        _lastEligibilityCheckUtc = now;

        try
        {
            var eligibility1 = await CheckAgvCallEligibilityAsync(AgvPosition.Position1);
            UpdateSlotDeclarationFromEligibility(Position1State, eligibility1);

            var eligibility2 = await CheckAgvCallEligibilityAsync(AgvPosition.Position2);
            UpdateSlotDeclarationFromEligibility(Position2State, eligibility2);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AgvBackgroundService] Periodic eligibility check failed: {ex.Message}");
        }
    }

    private void UpdateSlotDeclarationFromEligibility(AgvPositionState state, AgvCallEligibilityInfo? eligibility)
    {
        if (eligibility is null)
        {
            // Cannot reach server — keep current state, don't lock
            return;
        }

        var wasDeclared = state.IsServerSlotDeclared;
        state.IsServerSlotDeclared = eligibility.HasActiveDeclaration;

        if (!eligibility.HasActiveDeclaration)
        {
            var warningText = BuildAgvCallBlockedMessage(eligibility);
            state.ServerSlotWarningText = $"⚠ {warningText}";

            // Notify warning only on transition from declared → undeclared
            if (wasDeclared)
            {
                NotifyRuntimeWarning($"Cảnh báo AGV: Kệ {(int)state.Position} - {warningText}");
            }
        }
        else
        {
            state.ServerSlotWarningText = string.Empty;
        }
    }

    private void UpdateBadgeDisplay(AgvPositionState state)
    {
        switch (state.LocalStatus)
        {
            case AgvCallStatus.InProgress:
            case AgvCallStatus.Calling:
            case AgvCallStatus.AwaitingCompletion:
                state.StatusBadgeKey = "BadgeInfo";
                break;
            case AgvCallStatus.Failed:
                state.StatusBadgeKey = "BadgeDanger";
                break;
            case AgvCallStatus.Pending:
            default:
                if (state.IsPlcReady)
                    state.StatusBadgeKey = "BadgeSuccess";
                else if (state.OrderQty > 0 && state.RemainingQty <= GetAutoCallThreshold(state))
                    state.StatusBadgeKey = "BadgeWarning";
                else
                    state.StatusBadgeKey = "BadgeInfo";
                break;
        }
        
        if (state.LocalStatus == AgvCallStatus.Pending)
        {
            state.StatusText = "Sẵn sàng";
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _timer.Stop();

        lock (_apiServiceSync)
        {
            _retiredApiServices.Add(_apiService);
        }

        TryDisposeRetiredApiServices();
    }

    private async Task<T> UseApiServiceAsync<T>(Func<AgvTransferApiService, Task<T>> operation)
    {
        AgvTransferApiService apiService;

        lock (_apiServiceSync)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(AgvBackgroundService));

            _activeApiOperations++;
            apiService = _apiService;
        }

        try
        {
            return await operation(apiService);
        }
        finally
        {
            CompleteApiOperation();
        }
    }

    private void CompleteApiOperation()
    {
        lock (_apiServiceSync)
        {
            _activeApiOperations--;
        }

        TryDisposeRetiredApiServices();
    }

    private void TryDisposeRetiredApiServices()
    {
        AgvTransferApiService[] servicesToDispose = [];

        lock (_apiServiceSync)
        {
            if (_activeApiOperations > 0)
            {
                return;
            }

            if (_retiredApiServices.Count == 0)
            {
                return;
            }

            servicesToDispose = _retiredApiServices.ToArray();
            _retiredApiServices.Clear();
        }

        foreach (var service in servicesToDispose)
        {
            service.Dispose();
        }
    }

    private void NotifyRuntimeError(string message, Exception exception)
    {
        NotifyRuntimeNotification(NotificationDialogType.Error, message, exception, TimeSpan.FromSeconds(5));
    }

    private void ReportOrderCompletedRetryWarning(AgvPositionState state, AgvOrderData? order)
    {
        var orderText = order is null
            ? "order hiện tại"
            : $"order #{order.OrderSequence} ({order.OrderId})";
        var message =
            $"Kệ {(int)state.Position}: Ghi nhận hoàn thành {orderText} lên Server.Api chưa thành công, tiếp tục retry.";

        state.StatusText = message;
        state.StatusBadgeKey = "BadgeWarning";
        NotifyRuntimeWarning($"Cảnh báo AGV: {message}", dedupeWindow: TimeSpan.Zero);
    }

    private void ReportInactiveModelWriteBlocked(
        AgvPositionState state,
        InactiveModelProfileException exception,
        AgvOrderData? order = null)
    {
        var orderText = order is null
            ? "order hien tai"
            : $"order #{order.OrderSequence} ({order.OrderId})";
        var message =
            $"Ke {(int)state.Position}: Model '{exception.ModelName}' dang Deactive, chua ghi {orderText} xuong PLC. Vui long Active model roi thu lai.";

        state.StatusText = message;
        state.StatusBadgeKey = "BadgeWarning";
        NotifyRuntimeWarning($"Canh bao AGV: {message}");
    }

    private void NotifyRuntimeWarning(string message, TimeSpan? dedupeWindow = null)
    {
        NotifyRuntimeNotification(
            NotificationDialogType.Warning,
            message,
            new InvalidOperationException(message),
            dedupeWindow ?? TimeSpan.FromSeconds(20));
    }

    private void NotifyRuntimeNotification(NotificationDialogType type, string message, Exception exception, TimeSpan dedupeWindow)
    {
        var now = DateTime.UtcNow;
        var isWarning = type == NotificationDialogType.Warning;
        var lastMessage = isWarning ? _lastRuntimeWarningMessage : _lastRuntimeErrorMessage;
        var lastNotifiedAt = isWarning ? _lastRuntimeWarningNotifiedAtUtc : _lastRuntimeErrorNotifiedAtUtc;
        var isDuplicate = string.Equals(lastMessage, message, StringComparison.Ordinal);
        if (isDuplicate && now - lastNotifiedAt < dedupeWindow)
        {
            return;
        }

        if (isWarning)
        {
            _lastRuntimeWarningMessage = message;
            _lastRuntimeWarningNotifiedAtUtc = now;
        }
        else
        {
            _lastRuntimeErrorMessage = message;
            _lastRuntimeErrorNotifiedAtUtc = now;
        }

        RuntimeError?.Invoke(this, new AgvRuntimeErrorEventArgs(type, message, exception));
    }

    private static string BuildAgvCallBlockedMessage(AgvCallEligibilityInfo eligibility)
    {
        var machineDisplay = string.IsNullOrWhiteSpace(eligibility.MachineName)
            ? eligibility.MachineCode
            : $"{eligibility.MachineName} ({eligibility.MachineCode})";

        var stagingSlot = eligibility.StagingSlotIndex?.ToString() ?? "?";
        var reasonMessage = eligibility.ReasonCode switch
        {
            "NoDeclaration" => "chưa có khai báo hàng trên server.",
            "NoDeclaredOrders" => "bản khai báo chưa có order hợp lệ.",
            "MachineNotFound" => "không tìm thấy cấu hình máy trên server.",
            _ => string.IsNullOrWhiteSpace(eligibility.ReasonMessage)
                ? "không đủ điều kiện gọi AGV."
                : eligibility.ReasonMessage
        };

        return $"Máy {machineDisplay}, kệ máy {eligibility.MachineSlotIndex} (staging slot {stagingSlot}): {reasonMessage}";
    }
}

public sealed class AgvRuntimeErrorEventArgs : EventArgs
{
    public AgvRuntimeErrorEventArgs(NotificationDialogType type, string message, Exception exception)
    {
        Type = type;
        Message = message;
        Exception = exception;
    }

    public NotificationDialogType Type { get; }

    public string Message { get; }
    public Exception Exception { get; }
}
