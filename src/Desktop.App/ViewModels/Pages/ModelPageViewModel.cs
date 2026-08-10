using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Models.Runtime;
using Desktop.App.Models.Ui;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Api;
using Desktop.App.Session;
using Shared.Models.ModelProfiles;

namespace Desktop.App.ViewModels.Pages;

public partial class ModelPageViewModel : ObservableObject, IDisposable
{
    private readonly IModelProfileApiClient _apiClient;
    private readonly IPlcService _plcService;
    private readonly IPlcParameterSettingsService _plcParameterSettingsService;
    private readonly INotificationDialogService _notificationDialog;
    private readonly object _queueLock = new();
    private readonly Dictionary<string, AxisLimitProfile> _axisLimitsByKey = new(StringComparer.OrdinalIgnoreCase);
    private bool _isDisposed;
    private bool _isInitialized;
    private bool _isInitializing;
    private bool _isApplyingFieldValues;
    private bool _isSyncQueued;
    private string? _activeJogTagName;
    private int? _resolvedMachineId;



    // Tab state (control panel)
    [ObservableProperty] private bool _isAxisTabSelected = true;
    [ObservableProperty] private bool _isCylinderTabSelected;

    // Model list
    public ObservableCollection<ModelProfileDto> Models { get; } = [];
    public ICollectionView ModelsView { get; }

    [ObservableProperty] private ModelProfileDto? _selectedModel;
    [ObservableProperty] private string _modelFilterText = string.Empty;
    [ObservableProperty] private string _modelNameInput = string.Empty;
    [ObservableProperty] private string _itemTypeInput = string.Empty;
    [ObservableProperty] private string _machiningProgramInput = string.Empty;
    [ObservableProperty] private string _spare1Input = string.Empty;
    [ObservableProperty] private string _spare2Input = string.Empty;
    [ObservableProperty] private string _outerShaftDiameterInput = string.Empty;
    [ObservableProperty] private string _diameterOp1Input = string.Empty;
    [ObservableProperty] private string _diameterOp2Input = string.Empty;
    [ObservableProperty] private string _inputBlankDiameterInput = string.Empty;
    [ObservableProperty] private string _op2ChuckSleeveDepthInput = string.Empty;
    [ObservableProperty] private string _trayUsageInput = string.Empty;
    [ObservableProperty] private int? _trayTypeInput;
    [ObservableProperty] private int? _orderInputInput = 1;
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private bool _isCreatingNew;
    [ObservableProperty] private bool _isLoading;

    public IReadOnlyList<TrayTypeOption> TrayTypeOptions { get; } =
    [
        new(null, "Chưa chọn"),
        new(0, "Nhỏ"),
        new(1, "To")
    ];

    public IReadOnlyList<SelectOption> OrderInputOptions { get; } =
    [
        new(0, "Không nhập"),
        new(1, "Nhập")
    ];

    // Fields
    public ObservableCollection<ModelFieldValue> RobotFields { get; } = [];

    // Jog / Axis
    public ManualAxisState AxisX { get; }
    public ManualAxisState AxisY { get; }
    public ManualAxisState AxisZ { get; }
    public IReadOnlyList<ManualAxisState> Axes { get; }

    // Cylinders
    public IReadOnlyList<ManualCylinderState> Cylinders { get; }
    public ManualCylinderState ToolClampCylinder { get; }
    public ManualCylinderState RotateCylinder { get; }
    public ManualCylinderState ClampCart1Cylinder { get; }
    public ManualCylinderState ClampCart2Cylinder { get; }

    // Connection
    [ObservableProperty] private bool _isConnected;

    // Session
    [ObservableProperty] private bool _canSaveModel;

    //public bool CanIssueCommands => IsConnected;
    public bool CanIssueCommands => true;
    public bool HasModelSelected => SelectedModel is not null || IsCreatingNew;

    /// <summary>True when the currently selected model is enabled (allowed to run in Auto mode).</summary>
    public bool IsSelectedModelEnabled => SelectedModel?.IsEnabled ?? false;

    // Page metadata
    [ObservableProperty] private string _title = "C\u00e0i \u0111\u1eb7t Model";
    [ObservableProperty] private string _subtitle = "Qu\u1ea3n l\u00fd th\u00f4ng s\u1ed1 t\u1ecda \u0111\u1ed9 Robot v\u00e0 Line theo t\u1eebng m\u00e3 s\u1ea3n ph\u1ea9m";

    public ModelPageViewModel(
        IModelProfileApiClient apiClient,
        IPlcService plcService,
        IPlcParameterSettingsService plcParameterSettingsService,
        INotificationDialogService notificationDialog)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _plcService = plcService ?? throw new ArgumentNullException(nameof(plcService));
        _plcParameterSettingsService = plcParameterSettingsService ?? throw new ArgumentNullException(nameof(plcParameterSettingsService));
        _notificationDialog = notificationDialog ?? throw new ArgumentNullException(nameof(notificationDialog));

        ModelsView = CollectionViewSource.GetDefaultView(Models);
        ModelsView.Filter = FilterModel;

        AxisX = new ManualAxisState(
            "axis_x", "Trục X", "X -", "X +",
            PlcTagCatalog.Manual.MoveXBackward, PlcTagCatalog.Manual.MoveXForward,
            PlcTagCatalog.Manual.HomeX, PlcTagCatalog.Manual.MoveXToPoint,
            PlcTagCatalog.Manual.ManualSpeedX, PlcTagCatalog.Manual.MovePointX,
            PlcTagCatalog.Manual.CurrentPositionX, PlcTagCatalog.Manual.IsHomingX,
            PlcTagCatalog.Manual.IsHomedX, PlcTagCatalog.Alarms.XLimitNegative,
            PlcTagCatalog.Alarms.XLimitPositive, PlcTagCatalog.Outputs.Y0_09AxisXOn);

        AxisY = new ManualAxisState(
            "axis_y", "Trục Y", "Y -", "Y +",
            PlcTagCatalog.Manual.MoveYLeft, PlcTagCatalog.Manual.MoveYRight,
            PlcTagCatalog.Manual.HomeY, PlcTagCatalog.Manual.MoveYToPoint,
            PlcTagCatalog.Manual.ManualSpeedY, PlcTagCatalog.Manual.MovePointY,
            PlcTagCatalog.Manual.CurrentPositionY, PlcTagCatalog.Manual.IsHomingY,
            PlcTagCatalog.Manual.IsHomedY, PlcTagCatalog.Alarms.YLimitNegative,
            PlcTagCatalog.Alarms.YLimitPositive, PlcTagCatalog.Outputs.Y0_10AxisYOn);

        AxisZ = new ManualAxisState(
            "axis_z", "Trục Z", "Z -", "Z +",
            PlcTagCatalog.Manual.MoveZDown, PlcTagCatalog.Manual.MoveZUp,
            PlcTagCatalog.Manual.HomeZ, PlcTagCatalog.Manual.MoveZToPoint,
            PlcTagCatalog.Manual.ManualSpeedZ, PlcTagCatalog.Manual.MovePointZ,
            PlcTagCatalog.Manual.CurrentPositionZ, PlcTagCatalog.Manual.IsHomingZ,
            PlcTagCatalog.Manual.IsHomedZ, PlcTagCatalog.Alarms.ZLimitNegative,
            PlcTagCatalog.Alarms.ZLimitPositive, PlcTagCatalog.Outputs.Y0_11AxisZOn);

        Axes = [AxisX, AxisY, AxisZ];
        foreach (var axis in Axes)
        {
            axis.PropertyChanged += OnAxisPropertyChanged;
        }

        ToolClampCylinder = new ManualCylinderState(
            "tool_clamp", "K\u1eb9p tay tool", string.Empty, "K\u1eb9p", "M\u1edf", "K\u1eb9p", "M\u1edf",
            PlcTagCatalog.Manual.ToolClampIn, PlcTagCatalog.Manual.ToolClampOut,
            PlcTagCatalog.Manual.ToolClosedSignal, PlcTagCatalog.Manual.ToolOpenedSignal);

        RotateCylinder = new ManualCylinderState(
            "rotate", "Xy lanh xoay", string.Empty, "0\u00b0", "90\u00b0", "0\u00b0", "90\u00b0",
            PlcTagCatalog.Manual.ToolRotate0, PlcTagCatalog.Manual.ToolRotate90,
            PlcTagCatalog.Manual.RotatedTo0Signal, PlcTagCatalog.Manual.RotatedTo90Signal);

        ClampCart1Cylinder = new ManualCylinderState(
            "cart_1", "Xe h\u00e0ng 1", string.Empty, "K\u1eb9p", "M\u1edf", "K\u1eb9p", "M\u1edf",
            PlcTagCatalog.Manual.ClampCart1, PlcTagCatalog.Manual.UnclampCart1,
            PlcTagCatalog.Manual.Cart1ClosedSignal, PlcTagCatalog.Manual.Cart1OpenedSignal);

        ClampCart2Cylinder = new ManualCylinderState(
            "cart_2", "Xe h\u00e0ng 2", string.Empty, "K\u1eb9p", "M\u1edf", "K\u1eb9p", "M\u1edf",
            PlcTagCatalog.Manual.ClampCart2, PlcTagCatalog.Manual.UnclampCart2,
            PlcTagCatalog.Manual.Cart2ClosedSignal, PlcTagCatalog.Manual.Cart2OpenedSignal);

        Cylinders = [ToolClampCylinder, RotateCylinder, ClampCart1Cylinder, ClampCart2Cylinder];

        InitFieldCollections();
        foreach (var field in RobotFields)
        {
            field.PropertyChanged += OnRobotFieldPropertyChanged;
        }

        _plcService.ConnectionChanged += OnConnectionChanged;
        _plcService.DataUpdated += OnDataUpdated;
        AppSession.SessionChanged += OnSessionChanged;
        RefreshCanSaveModel();
    }

    private void InitFieldCollections()
    {
        foreach (var def in ModelFieldCatalog.RobotFields)
        {
            RobotFields.Add(new ModelFieldValue(def, isReadOnly: false));
        }
    }

    // ══ Lifecycle ══

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
                    SyncAxesFromCache();
                    UpdateConnectionState(_plcService.IsConnected);
                }
                finally
                {
                    _isInitializing = false;
                }
            });

        await LoadModelsAsync(CancellationToken.None);
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
        AppSession.SessionChanged -= OnSessionChanged;
        foreach (var axis in Axes)
        {
            axis.PropertyChanged -= OnAxisPropertyChanged;
        }

        foreach (var field in RobotFields)
        {
            field.PropertyChanged -= OnRobotFieldPropertyChanged;
        }

        if (!string.IsNullOrWhiteSpace(_activeJogTagName))
        {
            _ = StopJogAsync(_activeJogTagName);
        }
    }

    private async Task LoadAxisLimitsAsync()
    {
        var fields = await _plcParameterSettingsService.LoadGroupAsync(PlcParameterGroups.DataTrayCart).ConfigureAwait(false);
        var fieldLookup = fields.ToDictionary(field => field.TagName, StringComparer.OrdinalIgnoreCase);

        ApplyAxisLimit("axis_x", AxisX, CreateAxisLimitProfile(
            fieldLookup,
            PlcTagCatalog.DataTrayCart.AxisXSpeedLimit.Name,
            PlcTagCatalog.DataTrayCart.AxisXNegativeLimit.Name,
            PlcTagCatalog.DataTrayCart.AxisXPositiveLimit.Name));
        ApplyAxisLimit("axis_z", AxisZ, CreateAxisLimitProfile(
            fieldLookup,
            PlcTagCatalog.DataTrayCart.AxisZSpeedLimit.Name,
            PlcTagCatalog.DataTrayCart.AxisZNegativeLimit.Name,
            PlcTagCatalog.DataTrayCart.AxisZPositiveLimit.Name));

        ValidateRobotFields();
    }

    // ══ Session ══

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        PostToUiThread(RefreshCanSaveModel);
    }

    private void RefreshCanSaveModel()
    {
        CanSaveModel = AppSession.CanSaveModel;
        SaveModelCommand.NotifyCanExecuteChanged();
        DeleteModelCommand.NotifyCanExecuteChanged();
    }

    // ══ PLC events ══

    private void OnConnectionChanged(object? sender, bool connected)
    {
        PostToUiThread(() =>
        {
            UpdateConnectionState(connected);
            SyncAxesFromCache();
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

        SyncAxesFromCache();
    }

    private void SyncAxesFromCache()
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

        RefreshCommandStates();
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

    private void UpdateConnectionState(bool connected)
    {
        IsConnected = connected;
        OnPropertyChanged(nameof(CanIssueCommands));
        RefreshCommandStates();
    }

    // ══ Tab commands (control panel) ══

    [RelayCommand]
    private void SelectAxisTab()
    {
        IsAxisTabSelected = true;
        IsCylinderTabSelected = false;
    }

    [RelayCommand]
    private void SelectCylinderTab()
    {
        IsAxisTabSelected = false;
        IsCylinderTabSelected = true;
    }

    // ══ Model list commands ══

    [RelayCommand]
    private async Task RefreshModelsAsync(CancellationToken cancellationToken)
    {
        await LoadModelsAsync(cancellationToken);
    }

    [RelayCommand]
    private void SelectModel(ModelProfileDto? model)
    {
        if (model is null)
        {
            return;
        }

        SelectedModel = model;
        IsCreatingNew = false;
        ModelNameInput = model.ModelName;
        PopulateMetadata(model);
        PopulateFields(model);
        IsDirty = false;
        OnPropertyChanged(nameof(HasModelSelected));
    }

    [RelayCommand]
    private void CreateNewModel()
    {
        SelectedModel = null;
        IsCreatingNew = true;
        ModelNameInput = string.Empty;
        ClearMetadata();
        ClearFields();
        IsDirty = true;
        OnPropertyChanged(nameof(HasModelSelected));
    }

    [RelayCommand]
    private void DiscardChanges()
    {
        if (IsCreatingNew)
        {
            IsCreatingNew = false;
            SelectedModel = null;
            ClearFields();
            OnPropertyChanged(nameof(HasModelSelected));
        }
        else if (SelectedModel is not null)
        {
            PopulateMetadata(SelectedModel);
            PopulateFields(SelectedModel);
            ModelNameInput = SelectedModel.ModelName;
        }

        IsDirty = false;
    }

    [RelayCommand(CanExecute = nameof(CanSaveModel))]
    private async Task SaveModelAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ModelNameInput))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Ten model khong duoc de trong.");
            return;
        }

        ValidateRobotFields();
        var firstInvalidField = RobotFields.FirstOrDefault(field => field.HasValidationMessage);
        if (firstInvalidField is not null)
        {
            await _notificationDialog.ShowErrorAsync("Loi", firstInvalidField.ValidationMessage);
            return;
        }

        var machineId = await EnsureMachineIdAsync(cancellationToken);
        if (machineId is null)
        {
            return;
        }

        if (!TryReadOptionalInt(MachiningProgramInput, out var machiningProgram))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Chuong trinh gia cong phai la so nguyen.");
            return;
        }

        if (!TryReadOptionalDecimal(OuterShaftDiameterInput, out var outerShaftDiameter))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Duong kinh ngoai truc phai la so.");
            return;
        }

        if (!TryReadOptionalFloat(DiameterOp1Input, out var diameterOp1))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Duong kinh Op1 phai la so.");
            return;
        }

        if (!TryReadOptionalFloat(DiameterOp2Input, out var diameterOp2))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Duong kinh Op2 phai la so.");
            return;
        }

        if (!TryReadOptionalFloat(InputBlankDiameterInput, out var inputBlankDiameter))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Duong kinh phoi dau vao phai la so.");
            return;
        }

        if (!TryReadOptionalFloat(Op2ChuckSleeveDepthInput, out var op2ChuckSleeveDepth))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "Chieu sau bac mam cap OP2 phai la so.");
            return;
        }

        if (!TryReadOptionalInt(TrayUsageInput, out var trayUsage))
        {
            await _notificationDialog.ShowErrorAsync("Loi", "TRAY su dung phai la so nguyen.");
            return;
        }

        IsLoading = true;

        try
        {
            var request = new SaveModelProfileRequest
            {
                ModelName = ModelNameInput.Trim(),
                ItemType = TrimToNull(ItemTypeInput),
                MachiningProgram = machiningProgram,
                Spare1 = TrimToNull(Spare1Input),
                Spare2 = TrimToNull(Spare2Input),
                OuterShaftDiameter = outerShaftDiameter,
                DiameterOp1 = diameterOp1,
                DiameterOp2 = diameterOp2,
                InputBlankDiameter = inputBlankDiameter,
                Op2ChuckSleeveDepth = op2ChuckSleeveDepth,
                TrayUsage = trayUsage,
                TrayType = TrayTypeInput,
                OrderInput = OrderInputInput,
                RobotData = CollectFieldData(RobotFields),
                Line1Data = [],
                Line2Data = []
            };

            ModelProfileDto saved;

            if (IsCreatingNew)
            {
                saved = await _apiClient.CreateAsync(machineId.Value, request, cancellationToken);
            }
            else
            {
                saved = await _apiClient.UpdateAsync(machineId.Value, SelectedModel!.Id, request, cancellationToken);
            }

            await LoadModelsAsync(cancellationToken);

            var match = Models.FirstOrDefault(m => m.Id == saved.Id);
            if (match is not null)
            {
                SelectModel(match);
            }

            IsDirty = false;
            await _notificationDialog.ShowSuccessAsync("Thành Công", "Đã Lưu Thành Công.");
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không Thể Lưu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveModel))]
    private async Task DeleteModelAsync(CancellationToken cancellationToken)
    {
        if (SelectedModel is null)
        {
            return;
        }

        var machineId = await EnsureMachineIdAsync(cancellationToken);
        if (machineId is null)
        {
            return;
        }

        IsLoading = true;

        try
        {
            await _apiClient.DeleteAsync(machineId.Value, SelectedModel.Id, cancellationToken);
            await LoadModelsAsync(cancellationToken);
            SelectedModel = null;
            IsCreatingNew = false;
            ClearMetadata();
            ClearFields();
            OnPropertyChanged(nameof(HasModelSelected));
            await _notificationDialog.ShowSuccessAsync("Thành công", "Đã Xóa Model.");
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không Thể Xóa: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ══ Capture position from PLC ══

    [RelayCommand]
    private void CapturePosition(string? fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return;
        }

        var field = RobotFields.FirstOrDefault(f => f.Key == fieldKey);
        if (field is null)
        {
            return;
        }

        float position;

        if (fieldKey.EndsWith("X", StringComparison.Ordinal))
        {
            position = ReadSingle(PlcTagCatalog.Manual.CurrentPositionX.Name);
        }
        else if (fieldKey.EndsWith("Y", StringComparison.Ordinal))
        {
            position = ReadSingle(PlcTagCatalog.Manual.CurrentPositionY.Name);
        }
        else if (fieldKey.EndsWith("Z", StringComparison.Ordinal))
        {
            position = ReadSingle(PlcTagCatalog.Manual.CurrentPositionZ.Name);
        }
        else
        {
            return;
        }

        field.ValueText = ManualNumeric.Format(position);
        ValidateRobotField(field);
        IsDirty = true;
    }

    // ══ Jog commands ══

    [RelayCommand(CanExecute = nameof(CanStartJog))]
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
            await InvokeOnUiThreadAsync(() => SyncAxesFromCache());
        }
        catch (Exception ex)
        {
            _activeJogTagName = null;
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể jog: {ex.Message}");
        }
    }

    private bool CanStartJog(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName) || !CanIssueCommands || !string.IsNullOrWhiteSpace(_activeJogTagName))
        {
            return false;
        }

        var axis = Axes.FirstOrDefault(a =>
            string.Equals(a.NegativeJogTag.Name, tagName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(a.PositiveJogTag.Name, tagName, StringComparison.OrdinalIgnoreCase));

        if (axis is null)
        {
            return false;
        }

        return string.Equals(axis.NegativeJogTag.Name, tagName, StringComparison.OrdinalIgnoreCase)
            ? axis.CanJogNegative
            : axis.CanJogPositive;
    }

    [RelayCommand(CanExecute = nameof(CanStopJog))]
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
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể dừng jog: {ex.Message}");
        }
        finally
        {
            await InvokeOnUiThreadAsync(() =>
            {
                if (string.Equals(_activeJogTagName, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    _activeJogTagName = null;
                }

                SyncAxesFromCache();
                RefreshCommandStates();
            });
        }
    }

    private bool CanStopJog(string? tagName)
    {
        return !string.IsNullOrWhiteSpace(tagName)
            && string.Equals(_activeJogTagName, tagName, StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand(CanExecute = nameof(CanApplyAxisSpeed))]
    private async Task ApplyAxisSpeedAsync(ManualAxisState? axis)
    {
        if (axis is null)
        {
            return;
        }

        if (!axis.TryGetValidatedManualSpeed(out var speedValue, out _))
        {
            return;
        }

        try
        {
            await _plcService.WriteAsync(axis.ManualSpeedTag.Name, speedValue).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() =>
            {
                axis.MarkManualSpeedApplied(speedValue);
                SyncAxesFromCache();
            });
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể ghi tốc độ cho {axis.DisplayName}: {ex.Message}");
        }
    }

    private bool CanApplyAxisSpeed(ManualAxisState? axis)
    {
        return axis is not null
            && CanIssueCommands
            && !axis.HasManualSpeedValidationMessage;
    }

    [RelayCommand(CanExecute = nameof(CanWriteMovePointValue))]
    private async Task WriteMovePointValueAsync(ManualAxisState? axis)
    {
        if (axis is null || !CanWriteMovePointValue(axis))
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
                SyncAxesFromCache();
            });
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể ghi điểm chạy cho {axis.DisplayName}: {ex.Message}");
        }
    }

    private bool CanWriteMovePointValue(ManualAxisState? axis)
    {
        return axis is not null
            && CanIssueCommands
            && !axis.HasMovePointValidationMessage;
    }

    [RelayCommand(CanExecute = nameof(CanMoveAxisToPoint))]
    private async Task MoveAxisToPointAsync(ManualAxisState? axis)
    {
        if (axis is null)
        {
            return;
        }

        if (!axis.TryGetValidatedMovePoint(out var movePointValue, out _))
        {
            await _notificationDialog.ShowErrorAsync("Loi", $"Gia tri diem chay cua {axis.DisplayName} khong hop le.");
            return;
        }

        try
        {
            await _plcService.WriteAsync(axis.MovePointTag.Name, movePointValue).ConfigureAwait(false);
            await _plcService.WriteAsync(axis.MoveToPointTag.Name, true).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() =>
            {
                axis.MarkMovePointApplied(movePointValue);
                SyncAxesFromCache();
            });
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể chạy tới điểm cho {axis.DisplayName}: {ex.Message}");
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

    [RelayCommand(CanExecute = nameof(CanIssueCommands))]
    private async Task RunOneShotAsync(string? tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName) || !CanIssueCommands)
        {
            return;
        }

        try
        {
            // Mutual exclusion: turn off the opposite cylinder command first
            if (TryResolveCylinderCommand(tagName, out var cylinder, out var isPrimary))
            {
                var oppositeTag = isPrimary
                    ? cylinder.SecondaryCommandTag.Name
                    : cylinder.PrimaryCommandTag.Name;

                await _plcService.WriteAsync(oppositeTag, false).ConfigureAwait(false);
            }

            await _plcService.WriteAsync(tagName, true).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(SyncAxesFromCache);
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Loi", $"Loi lenh: {ex.Message}");
        }
    }

    // ══ Partial hooks ══

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanIssueCommands));
        RefreshCommandStates();
    }

    partial void OnSelectedModelChanged(ModelProfileDto? value)
    {
        OnPropertyChanged(nameof(HasModelSelected));
        OnPropertyChanged(nameof(IsSelectedModelEnabled));
        if (value is not null && !IsCreatingNew)
        {
            ModelNameInput = value.ModelName;
            PopulateMetadata(value);
            PopulateFields(value);
            IsDirty = false;
        }
    }

    partial void OnIsCreatingNewChanged(bool value)
    {
        OnPropertyChanged(nameof(HasModelSelected));
    }

    partial void OnModelFilterTextChanged(string value)
    {
        ModelsView.Refresh();
    }

    partial void OnModelNameInputChanged(string value)
    {
        if (!IsCreatingNew && SelectedModel is not null)
        {
            IsDirty = !string.Equals(value, SelectedModel.ModelName, StringComparison.Ordinal);
        }
    }

    partial void OnItemTypeInputChanged(string value) => MarkMetadataDirty();

    partial void OnMachiningProgramInputChanged(string value) => MarkMetadataDirty();

    partial void OnSpare1InputChanged(string value) => MarkMetadataDirty();

    partial void OnSpare2InputChanged(string value) => MarkMetadataDirty();

    partial void OnOuterShaftDiameterInputChanged(string value) => MarkMetadataDirty();

    partial void OnDiameterOp1InputChanged(string value) => MarkMetadataDirty();

    partial void OnDiameterOp2InputChanged(string value) => MarkMetadataDirty();

    partial void OnInputBlankDiameterInputChanged(string value) => MarkMetadataDirty();

    partial void OnOp2ChuckSleeveDepthInputChanged(string value) => MarkMetadataDirty();

    partial void OnTrayUsageInputChanged(string value) => MarkMetadataDirty();

    partial void OnTrayTypeInputChanged(int? value) => MarkMetadataDirty();

    partial void OnOrderInputInputChanged(int? value) => MarkMetadataDirty();

    // ══ Helpers ══

    private async Task LoadModelsAsync(CancellationToken cancellationToken)
    {
        var machineId = await EnsureMachineIdAsync(cancellationToken);
        if (machineId is null)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var list = await _apiClient.GetAllAsync(machineId.Value, cancellationToken);

            await InvokeOnUiThreadAsync(() =>
            {
                Models.Clear();
                foreach (var m in list)
                {
                    Models.Add(m);
                }

                ModelsView.Refresh();
            });
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể tải danh sách model: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<int?> EnsureMachineIdAsync(CancellationToken cancellationToken)
    {
        if (_resolvedMachineId.HasValue)
        {
            return _resolvedMachineId.Value;
        }

        try
        {
            var machineCode = AppSettings.Current.MachineCode;
            _resolvedMachineId = await _apiClient.ResolveMachineIdAsync(machineCode, cancellationToken);
            return _resolvedMachineId.Value;
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể xác định máy '{AppSettings.Current.MachineCode}': {ex.Message}");
            return null;
        }
    }

    private bool FilterModel(object item)
    {
        if (item is not ModelProfileDto model)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(ModelFilterText))
        {
            return true;
        }

        return model.ModelName.Contains(ModelFilterText.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void PopulateMetadata(ModelProfileDto model)
    {
        _isApplyingFieldValues = true;

        try
        {
            ItemTypeInput = model.ItemType ?? string.Empty;
            MachiningProgramInput = model.MachiningProgram?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            Spare1Input = model.Spare1 ?? string.Empty;
            Spare2Input = model.Spare2 ?? string.Empty;
            OuterShaftDiameterInput = model.OuterShaftDiameter?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            DiameterOp1Input = model.DiameterOp1?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            DiameterOp2Input = model.DiameterOp2?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            InputBlankDiameterInput = model.InputBlankDiameter?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            Op2ChuckSleeveDepthInput = model.Op2ChuckSleeveDepth?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            TrayUsageInput = model.TrayUsage?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            TrayTypeInput = model.TrayType;
            OrderInputInput = model.OrderInput ?? 1;
        }
        finally
        {
            _isApplyingFieldValues = false;
        }
    }

    private void ClearMetadata()
    {
        _isApplyingFieldValues = true;

        try
        {
            ItemTypeInput = string.Empty;
            MachiningProgramInput = string.Empty;
            Spare1Input = string.Empty;
            Spare2Input = string.Empty;
            OuterShaftDiameterInput = string.Empty;
            DiameterOp1Input = string.Empty;
            DiameterOp2Input = string.Empty;
            InputBlankDiameterInput = string.Empty;
            Op2ChuckSleeveDepthInput = string.Empty;
            TrayUsageInput = string.Empty;
            TrayTypeInput = null;
            OrderInputInput = 1;
        }
        finally
        {
            _isApplyingFieldValues = false;
        }
    }

    private void MarkMetadataDirty()
    {
        if (_isApplyingFieldValues)
        {
            return;
        }

        IsDirty = true;
    }

    private void PopulateFields(ModelProfileDto model)
    {
        _isApplyingFieldValues = true;

        try
        {
            PopulateFieldCollection(RobotFields, model.RobotData);
            InputBlankDiameterInput = model.InputBlankDiameter?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var inputDiameterField = RobotFields.FirstOrDefault(field => field.Key == "inputBlankDiameter");
            if (inputDiameterField is not null)
            {
                inputDiameterField.ValueText = InputBlankDiameterInput;
            }
            Op2ChuckSleeveDepthInput = model.Op2ChuckSleeveDepth?.ToString("G29", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var op2Field = RobotFields.FirstOrDefault(field => field.Key == "op2ChuckSleeveDepth");
            if (op2Field is not null)
            {
                op2Field.ValueText = Op2ChuckSleeveDepthInput;
            }
        }
        finally
        {
            _isApplyingFieldValues = false;
        }

        ValidateRobotFields();
    }

    private static void PopulateFieldCollection(
        ObservableCollection<ModelFieldValue> fields,
        Dictionary<string, object?> data)
    {
        foreach (var field in fields)
        {
            if (data.TryGetValue(field.Key, out var value) && value is not null)
            {
                field.ValueText = value.ToString() ?? string.Empty;
            }
            else
            {
                field.ValueText = string.Empty;
            }
        }
    }

    private void ClearFields()
    {
        _isApplyingFieldValues = true;

        try
        {
            foreach (var f in RobotFields) f.ValueText = string.Empty;
        }
        finally
        {
            _isApplyingFieldValues = false;
        }

        ValidateRobotFields();
    }

    private static Dictionary<string, object?> CollectFieldData(ObservableCollection<ModelFieldValue> fields)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var field in fields)
        {
            if (field.Key is "inputBlankDiameter" or "op2ChuckSleeveDepth") continue;
            if (string.IsNullOrWhiteSpace(field.ValueText)) continue;

            if (double.TryParse(field.ValueText, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
            {
                dict[field.Key] = d;
            }
            else
            {
                dict[field.Key] = field.ValueText;
            }
        }

        return dict;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool TryReadOptionalInt(string value, out int? parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        if (int.TryParse(value.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            parsed = result;
            return true;
        }

        parsed = null;
        return false;
    }

    private static bool TryReadOptionalDecimal(string value, out decimal? parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        if (decimal.TryParse(value.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var result)
            || decimal.TryParse(value.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), out result))
        {
            parsed = result;
            return true;
        }

        parsed = null;
        return false;
    }

    private static bool TryReadOptionalFloat(string value, out float? parsed)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        if (float.TryParse(value.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var result)
            || float.TryParse(value.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), out result))
        {
            parsed = result;
            return true;
        }

        parsed = null;
        return false;
    }

    private void RefreshCommandStates()
    {
        StartJogCommand.NotifyCanExecuteChanged();
        StopJogCommand.NotifyCanExecuteChanged();
        ApplyAxisSpeedCommand.NotifyCanExecuteChanged();
        WriteMovePointValueCommand.NotifyCanExecuteChanged();
        MoveAxisToPointCommand.NotifyCanExecuteChanged();
        RunOneShotCommand.NotifyCanExecuteChanged();

    }

    private void OnAxisPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is ManualAxisState)
        {
            RefreshCommandStates();
        }
    }

    private void OnRobotFieldPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not ModelFieldValue field || e.PropertyName != nameof(ModelFieldValue.ValueText))
        {
            return;
        }

        ValidateRobotField(field);
        if (field.Key == "inputBlankDiameter" && !string.Equals(InputBlankDiameterInput, field.ValueText, StringComparison.Ordinal))
        {
            InputBlankDiameterInput = field.ValueText;
        }
        else if (field.Key == "op2ChuckSleeveDepth" && !string.Equals(Op2ChuckSleeveDepthInput, field.ValueText, StringComparison.Ordinal))
        {
            Op2ChuckSleeveDepthInput = field.ValueText;
        }
        if (_isApplyingFieldValues) return;
        IsDirty = true;
    }

    private void ApplyAxisLimit(string axisKey, ManualAxisState axis, AxisLimitProfile profile)
    {
        _axisLimitsByKey[axisKey] = profile;
        axis.ApplyLimitProfile(profile);
    }

    private void ValidateRobotFields()
    {
        foreach (var field in RobotFields) ValidateRobotField(field);
    }

    private void ValidateRobotField(ModelFieldValue field)
    {
        if (field.IsSelectField || !field.IsCoordinateField || string.IsNullOrWhiteSpace(field.ValueText))
        {
            field.ValidationMessage = string.Empty;
            return;
        }

        if (!ManualNumeric.TryParse(field.ValueText, out var value))
        {
            field.ValidationMessage = $"{field.Label}: gia tri khong hop le.";
            return;
        }

        var axisKey = ResolveAxisKey(field.Key);
        if (axisKey is null || !_axisLimitsByKey.TryGetValue(axisKey, out var profile))
        {
            field.ValidationMessage = string.Empty;
            return;
        }

        field.ValidationMessage = profile.TryValidatePosition(value, out _)
            ? string.Empty
            : $"{field.Label}: gia tri phai nam trong {profile.DescribePositionRange()}.";
    }

    private static string? ResolveAxisKey(string fieldKey)
    {
        if (fieldKey.EndsWith("X", StringComparison.Ordinal)) return "axis_x";
        if (fieldKey.EndsWith("Y", StringComparison.Ordinal)) return "axis_y";
        if (fieldKey.EndsWith("Z", StringComparison.Ordinal)) return "axis_z";
        return null;
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

    private bool ReadBool(string tagName) => _plcService.GetValue(tagName, false);

    private float ReadSingle(string tagName)
    {
        var value = _plcService.GetValue<object?>(tagName, null);
        return value switch
        {
            float f => f,
            double d => (float)d,
            int i => i,
            short s => s,
            long l => l,
            _ => 0f
        };
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
}

public sealed record TrayTypeOption(int? Value, string Label);
