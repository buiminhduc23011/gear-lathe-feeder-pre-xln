using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Configuration.Plc;
using Desktop.App.Data.Repositories;
using Desktop.App.Models.Agv;
using Desktop.App.Models.Tray;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Agv;

namespace Desktop.App.ViewModels.Pages;

public partial class AutoPageViewModel : ObservableObject, IDisposable
{
    private readonly INotificationDialogService _dialogService;
    private readonly ILoginDialogService _loginDialogService;
    private readonly ITrayConfigRepository _trayConfigRepository;
    private readonly IShelfOrderCacheRepository _shelfOrderCacheRepository;
    private readonly IPlcService _plcService;
    private bool _isInitialized;
    private bool _isInitializing;

    // Cached tray type configs from Settings (only 2: Small + Large)
    private TrayConfig _smallTrayConfig = TrayConfig.CreateSmallDefault();
    private TrayConfig _largeTrayConfig = TrayConfig.CreateLargeDefault();

    public AgvBackgroundService AgvService { get; }

    [ObservableProperty]
    private string title = "Bảng điều khiển Tự động";
    [ObservableProperty]
    private string subtitle = "Theo dõi chu trình sản xuất, trạng thái liên động và các bước vận hành tự động định hướng Robot và AGV.";

    // --- Tray grid properties: Kệ 1 ---
    [ObservableProperty] private int ke1Tray1Rows = 5;
    [ObservableProperty] private int ke1Tray1Cols = 9;
    [ObservableProperty] private int ke1Tray2Rows = 5;
    [ObservableProperty] private int ke1Tray2Cols = 9;

    public ObservableCollection<TraySlotState> Ke1Tray1Slots { get; } = [];
    public ObservableCollection<TraySlotState> Ke1Tray2Slots { get; } = [];

    // --- Tray grid properties: Kệ 2 ---
    [ObservableProperty] private int ke2Tray1Rows = 5;
    [ObservableProperty] private int ke2Tray1Cols = 9;
    [ObservableProperty] private int ke2Tray2Rows = 5;
    [ObservableProperty] private int ke2Tray2Cols = 9;

    public ObservableCollection<TraySlotState> Ke2Tray1Slots { get; } = [];
    public ObservableCollection<TraySlotState> Ke2Tray2Slots { get; } = [];

    // --- Order display collections ---
    public ObservableCollection<OrderDisplayItem> Ke1Orders { get; } = [];
    public ObservableCollection<OrderDisplayItem> Ke2Orders { get; } = [];

    // --- Dynamic tray labels ---
    [ObservableProperty] private string ke1TrayLabel1 = "TRAY 1";
    [ObservableProperty] private string ke1TrayLabel2 = "TRAY 2";
    [ObservableProperty] private string ke2TrayLabel1 = "TRAY 1";
    [ObservableProperty] private string ke2TrayLabel2 = "TRAY 2";

    // --- RUN / STOP toggle ---
    [ObservableProperty] private bool isRunning = true;

    public AutoPageViewModel(
        AgvBackgroundService agvService,
        IPlcService plcService,
        INotificationDialogService dialogService,
        ILoginDialogService loginDialogService,
        ITrayConfigRepository trayConfigRepository,
        IShelfOrderCacheRepository shelfOrderCacheRepository)
    {
        AgvService = agvService;
        _plcService = plcService;
        _dialogService = dialogService;
        _loginDialogService = loginDialogService;
        _trayConfigRepository = trayConfigRepository;
        _shelfOrderCacheRepository = shelfOrderCacheRepository;
        AgvService.StateChanged += OnAgvStateChanged;
        AgvService.RuntimeError += OnAgvRuntimeError;
    }

    [RelayCommand]
    private async Task ToggleRunStopAsync()
    {
        if (IsRunning)
        {
            AgvService.Stop();
            IsRunning = false;
            await SyncRunStopToPlcAsync(isRunning: false);
            Trace.WriteLine("[AutoPage] AgvBackgroundService STOPPED by user.");
        }
        else
        {
            AgvService.Start();
            IsRunning = true;
            await SyncRunStopToPlcAsync(isRunning: true);
            Trace.WriteLine("[AutoPage] AgvBackgroundService STARTED by user.");
        }
    }

    // --- Global Auto-Call Pause (controls both shelves) ---
    [ObservableProperty] private bool isGlobalAutoPaused;

    [RelayCommand]
    private void ToggleGlobalAutoPause()
    {
        IsGlobalAutoPaused = !IsGlobalAutoPaused;
        AgvService.Position1State.IsAutoCallPausedLocally = IsGlobalAutoPaused;
        Trace.WriteLine($"[AutoPage] Global auto-call pause: {(IsGlobalAutoPaused ? "TẮT" : "BẬT")}");
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized || _isInitializing)
        {
            return;
        }

        _isInitializing = true;
        try
        {
            SyncRunStopFromPlc();
            await LoadTrayTypeConfigsAsync();
            await LoadActiveDeclarationsFromServerAsync();
            // Do not resync current orders to PLC on startup (load to UI only)
            RebuildAllTraySlots();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AutoPageViewModel] CRITICAL STARTUP ERROR: {ex}");
            await _dialogService.ShowErrorAsync("Lỗi nạp hệ thống", "Có lỗi xảy ra khi nạp cache kệ, bạn có thể cần Xóa dữ liệu Kệ và load lại.");
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void SyncRunStopFromPlc()
    {
        var paused = _plcService.GetValue<bool>(PlcTagCatalog.DataAutos.PausedByPc.Name);
        IsRunning = !paused;
    }

    private async Task SyncRunStopToPlcAsync(bool isRunning)
    {
        await _plcService.WriteAsync(PlcTagCatalog.DataAutos.PausedByPc.Name, !isRunning);
    }

    private void OnAgvStateChanged(object? sender, EventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher?.BeginInvoke(RebuildAllTraySlots);
    }

    public void Dispose()
    {
        AgvService.StateChanged -= OnAgvStateChanged;
        AgvService.RuntimeError -= OnAgvRuntimeError;
    }

    private void OnAgvRuntimeError(object? sender, AgvRuntimeErrorEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher?.BeginInvoke(async () =>
        {
            if (e.Type == NotificationDialogType.Warning)
            {
                await _dialogService.ShowWarningAsync("Canh bao AGV", e.Message);
                return;
            }

            await _dialogService.ShowErrorAsync("Lỗi AGV", e.Message);
        });
    }

    [RelayCommand]
    private async Task CallAgvManualAsync(string type)
    {
        var position = type == "1" ? AgvPosition.Position1 : AgvPosition.Position2;
        var state = position == AgvPosition.Position1 ? AgvService.Position1State : AgvService.Position2State;
        if (state.HasActiveCommand) return;

        if (!state.IsServerSlotDeclared)
        {
            await _dialogService.ShowWarningAsync(
                "Không thể gọi AGV",
                $"Kệ {type} chưa có khai báo hàng trên server.\nVui lòng khai báo hàng trước khi gọi AGV.");
            return;
        }

        var confirm = await _dialogService.ShowConfirmAsync(
            "Gọi AGV thủ công",
            $"Bạn muốn GỌI AGV THỦ CÔNG cho Kệ {type}?");
        if (confirm)
        {
            await AgvService.CallManualAsync(position);
        }
    }

    [RelayCommand]
    private async Task CancelAgvManualAsync(string type)
    {
        var position = type == "1" ? AgvPosition.Position1 : AgvPosition.Position2;
        var state = position == AgvPosition.Position1 ? AgvService.Position1State : AgvService.Position2State;
        if (!state.IsAutoCallPausedLocally || !state.HasActiveCommand) return;

        var confirm = await _dialogService.ShowConfirmAsync("Hủy lệnh AGV", $"Bạn có chắc chắn hủy bỏ phiên theo dõi gọi AGV cho Kệ {type} không?");
        if (confirm)
        {
            await AgvService.CancelManualAsync(position);
        }
    }

    [RelayCommand]
    private async Task ClearShelfManualAsync(string type)
    {
        Trace.WriteLine($"[AutoPage] ClearShelfManualAsync called with type={type}");
        var position = type == "1" ? AgvPosition.Position1 : AgvPosition.Position2;

        try
        {
            string? username = null;

            // Nếu đã đăng nhập r thì k yêu cầu đăng nhập nữa
            if (Desktop.App.Session.AppSession.IsAuthenticated)
            {
                username = Desktop.App.Session.AppSession.CurrentUserName;
            }
            else
            {
                // Yêu cầu đăng nhập nếu chưa có session
                var loginResult = await _loginDialogService.ShowAsync();
                if (loginResult is null || !loginResult.Success)
                {
                    Trace.WriteLine("[AutoPage] ClearShelf cancelled: login failed or dismissed.");
                    return;
                }
                username = loginResult.Username;
                
                // Lưu session luôn sau khi login thành công tại đây nếu cần (có thể LoginDialogService đã làm việc này)
                // Desktop.App.Session.AppSession.SetSession(loginResult);
            }

            if (string.IsNullOrEmpty(username)) 
            {
                 Trace.WriteLine("[AutoPage] ClearShelf aborted: No username available.");
                 return;
            }

            var confirm = await _dialogService.ShowConfirmAsync(
                "Xóa dữ liệu kệ",
                $"Bạn có chắc chắn muốn XÓA TOÀN BỘ dữ liệu kệ {type}?\n(Order, trạng thái tray sẽ bị reset)");
            Trace.WriteLine($"[AutoPage] ClearShelf confirm result: {confirm}");
            if (confirm)
            {
                var cleared = await AgvService.ClearPositionAsync(position, sendClearEvent: true, clearedByUsername: username);
                if (cleared)
                {
                    RebuildAllTraySlots();
                    Trace.WriteLine($"[AutoPage] Shelf {type} cleared by '{username}' successfully.");
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Lỗi", $"Không thể xóa dữ liệu kệ {type} trên server. Vui lòng kiểm tra kết nối mạng hoặc trạng thái server.");
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AutoPage] ClearShelfManualAsync ERROR: {ex}");
        }
    }

    [RelayCommand]
    private async Task LoadManualAsync(string type)
    {
        var position = type == "1" ? AgvPosition.Position1 : AgvPosition.Position2;
        var result = await AgvService.LoadPendingManualAsync(position);

        switch (result.Status)
        {
            case ManualLoadResultStatus.Success:
                await _dialogService.ShowSuccessAsync("Thành công", result.Message);
                RebuildAllTraySlots();
                break;
            case ManualLoadResultStatus.NoPendingDeclaration:
                await _dialogService.ShowInfoAsync("Thông báo", result.Message);
                break;
            case ManualLoadResultStatus.PositionNotClear:
                await _dialogService.ShowWarningAsync("Không thể Load Manual", result.Message);
                break;
            case ManualLoadResultStatus.BlockedByInactiveModel:
                RebuildAllTraySlots();
                break;
            default:
                await _dialogService.ShowErrorAsync("Lỗi", result.Message);
                break;
        }
    }

    [RelayCommand]
    private void ToggleAutoCallPause(string type)
    {
        var state = type == "1" ? AgvService.Position1State : AgvService.Position2State;
        state.IsAutoCallPausedLocally = !state.IsAutoCallPausedLocally;
    }

    #region Config Loading

    /// <summary>Load tray type configs (Small + Large) from SQLite.</summary>
    private async Task LoadTrayTypeConfigsAsync()
    {
        try
        {
            var configs = await _trayConfigRepository.GetAllAsync();
            foreach (var config in configs)
            {
                if (config.Size == TraySize.Small) _smallTrayConfig = config;
                else if (config.Size == TraySize.Large) _largeTrayConfig = config;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AutoPageViewModel] Failed to load tray configs, using defaults: {ex}");
        }
    }

    private async Task LoadActiveDeclarationsFromServerAsync()
    {
        try
        {
            await LoadActiveDeclarationForPositionAsync(AgvPosition.Position1);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[AutoPageViewModel] Failed to load active declaration for position 1: {ex}");
        }
    }

    private async Task LoadActiveDeclarationForPositionAsync(AgvPosition position)
    {
        var activeLoad = await AgvService.FetchActiveLoadAsync(position);
        var state = position == AgvPosition.Position1 ? AgvService.Position1State : AgvService.Position2State;

        if (activeLoad != null)
        {
            var orders = JsonSerializer.Deserialize<List<AgvOrderData>>(activeLoad.OrdersJson) ?? [];
            state.ActiveDeclarationId = activeLoad.DeclarationId;
            state.Orders = orders;
            state.ShelfLayoutType = activeLoad.ShelfLayoutType;

            // Resolve current running sequence based on the first incomplete order on the server
            var currentOrder = orders
                .OrderBy(o => o.OrderSequence)
                .FirstOrDefault(o => !string.Equals(o.Status, "Completed", StringComparison.OrdinalIgnoreCase));

            state.CurrentOrderSequence = currentOrder?.OrderSequence ?? (orders.Count + 1);
            state.CurrentItemInOrder = 0;
            state.AwaitingPickReset = false;
            state.CompletionReported = false;
            state.CompletionAcknowledged = false;

            var orderQty = orders.Sum(o => Math.Max(0, o.Quantity));
            var ranQty = orders
                .Where(o => string.Equals(o.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                .Sum(o => Math.Max(0, o.Quantity));

            state.OrderQty = orderQty;
            state.RanQty = ranQty;
            state.RemainingQty = Math.Max(0, orderQty - ranQty);
            state.CanLoadManual = false;
        }
        else
        {
            // Clear local state if no active declaration on the server
            state.ClearShelfData();
            state.OrderQty = 0;
            state.RanQty = 0;
            state.RemainingQty = 0;
        }
    }

    #endregion

    #region Tray Slot Computation

    /// <summary>
    /// Resolve (rows, cols) for a tray position based on AGV layout type + tray type configs.
    /// </summary>
    private (int Rows, int Cols) GetTrayDimensions(TraySize traySize)
    {
        var config = traySize == TraySize.Large ? _largeTrayConfig : _smallTrayConfig;
        return (config.Rows, config.Columns);
    }

    /// <summary>Rebuild all 4 tray slot collections from current AGV position state + layout type.</summary>
    private void RebuildAllTraySlots()
    {
        RebuildPositionSlots(AgvService.Position1State,
            s => { Ke1Tray1Rows = s.t1r; Ke1Tray1Cols = s.t1c; Ke1Tray2Rows = s.t2r; Ke1Tray2Cols = s.t2c; },
            (l1, l2) => { Ke1TrayLabel1 = l1; Ke1TrayLabel2 = l2; },
            Ke1Tray1Slots, Ke1Tray2Slots, Ke1Orders);
    }

    private void RebuildPositionSlots(
        AgvPositionState positionState,
        Action<(int t1r, int t1c, int t2r, int t2c)> setDimensions,
        Action<string, string> setTrayLabels,
        ObservableCollection<TraySlotState> tray1Slots,
        ObservableCollection<TraySlotState> tray2Slots,
        ObservableCollection<OrderDisplayItem> orderItems)
    {
        // Resolve tray sizes from AGV ShelfLayoutType
        var layout = (ShelfLayoutType)positionState.ShelfLayoutType;
        var (tray1Size, tray2Size) = layout.GetTraySizes();
        var (tray1Rows, tray1Cols) = GetTrayDimensions(tray1Size);
        var (tray2Rows, tray2Cols) = GetTrayDimensions(tray2Size);

        // Update UI binding properties
        setDimensions((tray1Rows, tray1Cols, tray2Rows, tray2Cols));

        // Update display text on state
        positionState.ShelfLayoutDisplayText = layout.GetDisplayText();

        // Build tray labels with type + dimensions
        var tray1SizeText = tray1Size == TraySize.Large ? "Tray Lớn" : "Tray Nhỏ";
        var tray2SizeText = tray2Size == TraySize.Large ? "Tray Lớn" : "Tray Nhỏ";
        setTrayLabels(
            $"TRAY 1 · {tray1SizeText} ({tray1Rows}×{tray1Cols})",
            $"TRAY 2 · {tray2SizeText} ({tray2Rows}×{tray2Cols})");

        var tray1Total = tray1Rows * tray1Cols;
        var tray2Total = tray2Rows * tray2Cols;

        // Ensure slot collections have correct size
        EnsureSlotCount(tray1Slots, tray1Total, tray1Cols);
        EnsureSlotCount(tray2Slots, tray2Total, tray2Cols);

        var desiredTray1States = new (SlotStatus Status, string? OrderId, string? ModelName)[tray1Total];
        var desiredTray2States = new (SlotStatus Status, string? OrderId, string? ModelName)[tray2Total];
        var orders = positionState.Orders
            .OrderBy(order => order.OrderSequence)
            .ToList();
        var currentSeq = positionState.CurrentOrderSequence;
        var currentItem = positionState.CurrentItemInOrder;

        // Rebuild order display items without recreating the whole collection each tick.
        var desiredOrderItems = new List<OrderDisplayItem>(orders.Count);
        string? currentModelName = null;

        foreach (var order in orders)
        {
            var targetStates = order.TrayIndex == 2 ? desiredTray2States : desiredTray1States;
            var targetTotal = order.TrayIndex == 2 ? tray2Total : tray1Total;

            var orderSeq = order.OrderSequence;
            var isCompleted = currentSeq > 0 && orderSeq < currentSeq;
            var isRunning = currentSeq > 0 && orderSeq == currentSeq;

            // Track current model
            if (isRunning)
            {
                currentModelName = order.ModelName;
            }

            // Build order display item
            var trayTypeText = order.TrayType == 2 ? "Lớn" : order.TrayType == 1 ? "Nhỏ" : "—";
            var jigText = GetJigTypeText(order.JigType);

            string statusText;
            string statusBadgeKey;
            if (isCompleted)
            {
                statusText = "Hoàn thành";
                statusBadgeKey = "BadgeSuccess";
            }
            else if (isRunning)
            {
                statusText = "Đang chạy";
                statusBadgeKey = "BadgeInfo";
            }
            else
            {
                statusText = "Chờ";
                statusBadgeKey = "BadgeWarning";
            }

            desiredOrderItems.Add(new OrderDisplayItem
            {
                Sequence = orderSeq,
                OrderId = order.OrderId,
                ModelName = string.IsNullOrEmpty(order.ModelName) ? "—" : order.ModelName,
                Quantity = order.Quantity,
                TrayIndex = order.TrayIndex,
                StartPosition = order.StartPosition,
                TrayTypeName = trayTypeText,
                JigTypeName = jigText,
                StatusText = statusText,
                StatusBadgeKey = statusBadgeKey
            });

            // Fill tray slots — StartPosition is a 1-based ROW index, each order fills one row
            var targetCols = order.TrayIndex == 2 ? tray2Cols : tray1Cols;
            for (int i = 0; i < order.Quantity; i++)
            {
                var slotIndex = (order.StartPosition - 1) * targetCols + i;
                if (slotIndex < 0 || slotIndex >= targetTotal) continue;

                var nextStatus = SlotStatus.HasProduct;

                if (isCompleted)
                {
                    nextStatus = SlotStatus.Empty;
                }
                else if (isRunning)
                {
                    var itemIndex = i + 1;
                    if (positionState.CompletionAcknowledged || (currentItem > 0 && itemIndex < currentItem))
                        nextStatus = SlotStatus.Empty;
                    else if (currentItem > 0 && itemIndex == currentItem)
                        nextStatus = SlotStatus.Picking;
                    else
                        nextStatus = SlotStatus.HasProduct;
                }

                targetStates[slotIndex] = (nextStatus, order.OrderId, order.ModelName);
            }
        }

        SyncOrderDisplayItems(orderItems, desiredOrderItems);

        ApplyDesiredSlotStates(tray1Slots, desiredTray1States);
        ApplyDesiredSlotStates(tray2Slots, desiredTray2States);

        // Update current model name on state
        positionState.CurrentModelName = currentModelName ?? "—";
    }

    private static void ApplyDesiredSlotStates(
        ObservableCollection<TraySlotState> slots,
        (SlotStatus Status, string? OrderId, string? ModelName)[] desiredStates)
    {
        var limit = Math.Min(slots.Count, desiredStates.Length);
        for (var i = 0; i < limit; i++)
        {
            var slot = slots[i];
            var desired = desiredStates[i];

            if (slot.Status != desired.Status) slot.Status = desired.Status;
            if (slot.OrderId != desired.OrderId) slot.OrderId = desired.OrderId;
            if (slot.ModelName != desired.ModelName) slot.ModelName = desired.ModelName;
        }
    }

    internal static void SyncOrderDisplayItems(
        ObservableCollection<OrderDisplayItem> orderItems,
        IReadOnlyList<OrderDisplayItem> desiredItems)
    {
        var sharedCount = Math.Min(orderItems.Count, desiredItems.Count);
        for (var index = 0; index < sharedCount; index++)
        {
            ApplyOrderDisplayItem(orderItems[index], desiredItems[index]);
        }

        for (var index = sharedCount; index < desiredItems.Count; index++)
        {
            orderItems.Add(CloneOrderDisplayItem(desiredItems[index]));
        }

        while (orderItems.Count > desiredItems.Count)
        {
            orderItems.RemoveAt(orderItems.Count - 1);
        }
    }

    private static void ApplyOrderDisplayItem(OrderDisplayItem target, OrderDisplayItem source)
    {
        target.Sequence = source.Sequence;
        target.OrderId = source.OrderId;
        target.ModelName = source.ModelName;
        target.Quantity = source.Quantity;
        target.TrayIndex = source.TrayIndex;
        target.StartPosition = source.StartPosition;
        target.TrayTypeName = source.TrayTypeName;
        target.JigTypeName = source.JigTypeName;
        target.StatusText = source.StatusText;
        target.StatusBadgeKey = source.StatusBadgeKey;
    }

    private static OrderDisplayItem CloneOrderDisplayItem(OrderDisplayItem source)
    {
        return new OrderDisplayItem
        {
            Sequence = source.Sequence,
            OrderId = source.OrderId,
            ModelName = source.ModelName,
            Quantity = source.Quantity,
            TrayIndex = source.TrayIndex,
            StartPosition = source.StartPosition,
            TrayTypeName = source.TrayTypeName,
            JigTypeName = source.JigTypeName,
            StatusText = source.StatusText,
            StatusBadgeKey = source.StatusBadgeKey
        };
    }

    private static void EnsureSlotCount(ObservableCollection<TraySlotState> slots, int total, int columns)
    {
        if (slots.Count == total) return;

        slots.Clear();
        for (int i = 0; i < total; i++)
        {
            slots.Add(new TraySlotState
            {
                Position = i + 1,
                Row = i / columns,
                Column = i % columns,
                Status = SlotStatus.Empty
            });
        }
    }

    private static string GetJigTypeText(int jigType)
    {
        var option = Array.Find(JigTypeOptions.Items, item => item.Value == jigType);
        return option?.Label ?? "—";
    }

    #endregion
}
