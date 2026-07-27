using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Configuration;
using Desktop.App.Configuration.Plc;
using Desktop.App.Data.Repositories;
using Desktop.App.Models.Runtime;
using Desktop.App.Models.Tray;
using Desktop.App.Models.Ui;
using Desktop.App.Services;
using Desktop.App.Services.Abstractions;
using Desktop.App.Services.Plc;
using Desktop.App.Session;

namespace Desktop.App.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject, IDisposable
{
    private readonly ISettingsService _settingsService;
    private readonly IPlcParameterSettingsService _plcParameterSettingsService;
    private readonly IPlcParameterSyncService _plcParameterSyncService;
    private readonly INotificationDialogService _notificationDialog;
    private readonly ITrayConfigRepository _trayConfigRepository;
    private static readonly string[] SupportedPlcTypes = ["DVP", "AS"];
    private AppOptions _loadedOptions = new();
    private bool _isApplyingLoadedValues;
    private bool _isApplyingParameterValues;
    private bool _isDisposed;
    private bool _isInitialized;

    public SettingsPageViewModel(
        ISettingsService settingsService,
        IPlcParameterSettingsService plcParameterSettingsService,
        IPlcParameterSyncService plcParameterSyncService,
        INotificationDialogService notificationDialog,
        ITrayConfigRepository trayConfigRepository)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _plcParameterSettingsService = plcParameterSettingsService ?? throw new ArgumentNullException(nameof(plcParameterSettingsService));
        _plcParameterSyncService = plcParameterSyncService ?? throw new ArgumentNullException(nameof(plcParameterSyncService));
        _notificationDialog = notificationDialog ?? throw new ArgumentNullException(nameof(notificationDialog));
        _trayConfigRepository = trayConfigRepository ?? throw new ArgumentNullException(nameof(trayConfigRepository));

        DataTrayCartParameters.CollectionChanged += OnParameterCollectionChanged;
        DataMachineParameters.CollectionChanged += OnParameterCollectionChanged;
        _plcParameterSyncService.SyncStatesChanged += OnSyncStatesChanged;
        AppSession.SessionChanged += OnSessionChanged;

        RefreshIsEditable();
    }

    [ObservableProperty]
    private string title = "Cài đặt hệ thống";

    [ObservableProperty]
    private string subtitle = "Quản lý cấu hình máy, tham số PLC và địa chỉ server được lưu cục bộ trên máy.";

    [ObservableProperty]
    private string machineName = string.Empty;

    [ObservableProperty]
    private string machineCode = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string plcHost = string.Empty;

    [ObservableProperty]
    private string plcPort = string.Empty;

    [ObservableProperty]
    private string plcId = string.Empty;

    [ObservableProperty]
    private string selectedPlcType = SupportedPlcTypes[0];

    [ObservableProperty]
    private string apiBaseUrl = string.Empty;

    [ObservableProperty]
    private string scanRateMs = string.Empty;


    [ObservableProperty]
    private bool isMachineSettingsTabSelected = true;

    [ObservableProperty]
    private bool isDataTrayCartTabSelected;

    [ObservableProperty]
    private bool isDataMachineTabSelected;


    [ObservableProperty]
    private bool isAgvSettingsTabSelected;

    [ObservableProperty]
    private bool isTraySettingsTabSelected;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isDirty;

    [ObservableProperty]
    private bool isEditable;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool virtualKeyboardEnabled = true;

    // === AGV ===
    [ObservableProperty]
    private string agvBaseUrl = string.Empty;


    [ObservableProperty]
    private bool agvAutoCallEnabled;

    [ObservableProperty]
    private string agvKe1AutoCallRemainingBelow = "5";

    [ObservableProperty]
    private string agvKe2AutoCallRemainingBelow = "5";

    // === TRAY TYPE CONFIG (chỉ 2 loại: Bé + Lớn) ===
    [ObservableProperty] private string smallTrayRows = "5";
    [ObservableProperty] private string smallTrayCols = "9";
    [ObservableProperty] private string smallTrayRowOffset = "55";
    [ObservableProperty] private string smallTrayColOffset = "55";

    [ObservableProperty] private string largeTrayRows = "4";
    [ObservableProperty] private string largeTrayCols = "8";
    [ObservableProperty] private string largeTrayRowOffset = "68";
    [ObservableProperty] private string largeTrayColOffset = "68";

    public ObservableCollection<EditablePlcParameterField> DataTrayCartParameters { get; } = [];

    public ObservableCollection<EditablePlcParameterField> DataMachineParameters { get; } = [];


    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

    public bool CanSave => !IsBusy && IsDirty && IsEditable;

    public bool CanReload => !IsBusy && IsDirty && IsEditable;

    public IReadOnlyList<string> PlcTypes => SupportedPlcTypes;

    public string ActiveMachineText =>
        string.IsNullOrWhiteSpace(MachineCode)
            ? "Chưa cấu hình"
            : $"{MachineName.Trim()} ({MachineCode.Trim()})";

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await LoadMachineSettingsAsync(showFeedback: false);
        await ReloadParameterGroupAsync(PlcParameterGroups.DataTrayCart);
        await ReloadParameterGroupAsync(PlcParameterGroups.DataMachine);
        await LoadTrayConfigsAsync();
    }

    partial void OnMachineNameChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnMachineCodeChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnDescriptionChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnPlcHostChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnPlcPortChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnApiBaseUrlChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnPlcIdChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnSelectedPlcTypeChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnScanRateMsChanged(string value)
    {
        OnFormValueChanged();
    }


    partial void OnIsBusyChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        ReloadCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDirtyChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        ReloadCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEditableChanged(bool value)
    {
        SaveCommand.NotifyCanExecuteChanged();
        ReloadCommand.NotifyCanExecuteChanged();
    }

    partial void OnValidationMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasValidationMessage));
    }

    partial void OnVirtualKeyboardEnabledChanged(bool value)
    {
        // Apply immediately so the keyboard reacts without Save
        VirtualKeyboardStateService.Instance.IsEnabled = value;

        // If the user turned the keyboard OFF, hide it immediately
        // so it doesn't stay floating on screen.
        if (!value)
        {
            VirtualKeyboardManager.Hide();
        }

        OnFormValueChanged();
    }


    partial void OnAgvBaseUrlChanged(string value)
    {
        OnFormValueChanged();
    }


    partial void OnAgvAutoCallEnabledChanged(bool value)
    {
        OnFormValueChanged();
    }

    partial void OnAgvKe1AutoCallRemainingBelowChanged(string value)
    {
        OnFormValueChanged();
    }

    partial void OnAgvKe2AutoCallRemainingBelowChanged(string value)
    {
        OnFormValueChanged();
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!TryBuildOptions(out var options, out var errorMessage))
        {
            ValidationMessage = errorMessage;
            return;
        }

        IsBusy = true;
        ValidationMessage = string.Empty;

        try
        {
            await _settingsService.SaveAsync(options);
            _loadedOptions = options.Clone();
            ApplyOptionsToForm(options);
            AppSettings.Update(options);
            UpdateDirtyState();
            await _notificationDialog.ShowSuccessAsync("Thành công", "Đã lưu cấu hình. Khởi động lại ứng dụng để áp dụng cho PLC/server.");
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể lưu cấu hình máy: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanReload))]
    private async Task ReloadAsync()
    {
        await LoadMachineSettingsAsync(showFeedback: true);
    }

    [RelayCommand]
    private void SelectMachineSettingsTab()
    {
        IsMachineSettingsTabSelected = true;
        IsDataTrayCartTabSelected = false;
        IsDataMachineTabSelected = false;
        IsAgvSettingsTabSelected = false;
        IsTraySettingsTabSelected = false;
    }

    [RelayCommand]
    private void SelectDataTrayCartTab()
    {
        IsMachineSettingsTabSelected = false;
        IsDataTrayCartTabSelected = true;
        IsDataMachineTabSelected = false;
        IsAgvSettingsTabSelected = false;
        IsTraySettingsTabSelected = false;
    }

    [RelayCommand]
    private void SelectDataMachineTab()
    {
        IsMachineSettingsTabSelected = false;
        IsDataTrayCartTabSelected = false;
        IsDataMachineTabSelected = true;
        IsAgvSettingsTabSelected = false;
        IsTraySettingsTabSelected = false;
    }

    [RelayCommand]
    private void SelectAgvSettingsTab()
    {
        IsMachineSettingsTabSelected = false;
        IsDataTrayCartTabSelected = false;
        IsDataMachineTabSelected = false;
        IsAgvSettingsTabSelected = true;
        IsTraySettingsTabSelected = false;
    }

    [RelayCommand]
    private void SelectTraySettingsTab()
    {
        IsMachineSettingsTabSelected = false;
        IsDataTrayCartTabSelected = false;
        IsDataMachineTabSelected = false;
        IsAgvSettingsTabSelected = false;
        IsTraySettingsTabSelected = true;
    }

    // --- Tray type auto-fill removed (no longer per-position) ---

    [RelayCommand]
    private async Task SaveTrayConfigAsync()
    {
        try
        {
            await SaveTrayTypeConfigAsync(TraySize.Small, SmallTrayRows, SmallTrayCols, SmallTrayRowOffset, SmallTrayColOffset);
            await SaveTrayTypeConfigAsync(TraySize.Large, LargeTrayRows, LargeTrayCols, LargeTrayRowOffset, LargeTrayColOffset);
            await _notificationDialog.ShowSuccessAsync("Thành công", "Đã lưu cấu hình Tray.");
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể lưu cấu hình Tray: {ex.Message}");
        }
    }

    private async Task SaveTrayTypeConfigAsync(TraySize size, string rowsStr, string colsStr, string rowOffsetStr, string colOffsetStr)
    {
        if (!int.TryParse(rowsStr, out var rows)) rows = size == TraySize.Large ? 4 : 5;
        if (!int.TryParse(colsStr, out var cols)) cols = size == TraySize.Large ? 8 : 9;
        if (!float.TryParse(rowOffsetStr, out var rowOffset)) rowOffset = 55f;
        if (!float.TryParse(colOffsetStr, out var colOffset)) colOffset = 55f;

        await _trayConfigRepository.SaveAsync(new TrayConfig
        {
            Size = size,
            Rows = rows,
            Columns = cols,
            RowOffset = rowOffset,
            ColOffset = colOffset
        });
    }

    private async Task LoadTrayConfigsAsync()
    {
        try
        {
            var configs = await _trayConfigRepository.GetAllAsync();
            foreach (var config in configs)
            {
                var r = config.Rows.ToString();
                var c = config.Columns.ToString();
                var ro = config.RowOffset.ToString(CultureInfo.InvariantCulture);
                var co = config.ColOffset.ToString(CultureInfo.InvariantCulture);

                if (config.Size == TraySize.Small)
                {
                    SmallTrayRows = r; SmallTrayCols = c;
                    SmallTrayRowOffset = ro; SmallTrayColOffset = co;
                }
                else if (config.Size == TraySize.Large)
                {
                    LargeTrayRows = r; LargeTrayCols = c;
                    LargeTrayRowOffset = ro; LargeTrayColOffset = co;
                }
            }
        }
        catch { /* Use defaults */ }
    }

    [RelayCommand]
    private async Task ReloadDataTrayCartAsync()
    {
        await ReloadParameterGroupAsync(PlcParameterGroups.DataTrayCart);
    }

    [RelayCommand]
    private async Task ReloadDataMachineAsync()
    {
        await ReloadParameterGroupAsync(PlcParameterGroups.DataMachine);
    }


    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        AppSession.SessionChanged -= OnSessionChanged;
        _plcParameterSyncService.SyncStatesChanged -= OnSyncStatesChanged;
        DataTrayCartParameters.CollectionChanged -= OnParameterCollectionChanged;
        DataMachineParameters.CollectionChanged -= OnParameterCollectionChanged;

        foreach (var field in DataTrayCartParameters)
        {
            field.PropertyChanged -= OnParameterFieldPropertyChanged;
        }

        foreach (var field in DataMachineParameters)
        {
            field.PropertyChanged -= OnParameterFieldPropertyChanged;
        }

    }

    private async Task LoadMachineSettingsAsync(bool showFeedback)
    {
        IsBusy = true;

        try
        {
            var options = await _settingsService.LoadAsync();
            _loadedOptions = options.Clone();
            ApplyOptionsToForm(options);
            UpdateDirtyState();
            ValidationMessage = string.Empty;
            if (showFeedback)
            {
                await _notificationDialog.ShowSuccessAsync("Thành công", "Đã tải lại cấu hình đã lưu.");
            }
        }
        catch (Exception exception)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể tải cấu hình máy: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadParameterGroupAsync(string groupName)
    {
        var fields = await _plcParameterSettingsService.LoadGroupAsync(groupName);
        ApplyParameterFields(groupName, fields);
    }

    private void ApplyOptionsToForm(AppOptions options)
    {
        _isApplyingLoadedValues = true;

        try
        {
            MachineName = options.MachineName;
            MachineCode = options.MachineCode;
            Description = options.Description;
            PlcHost = options.PlcHost;
            PlcPort = options.PlcPort.ToString(CultureInfo.InvariantCulture);
            PlcId = options.PlcSlaveId.ToString(CultureInfo.InvariantCulture);
            SelectedPlcType = NormalizePlcType(options.PlcConnectionMode);
            ApiBaseUrl = options.ApiBaseUrl;
            ScanRateMs = options.PollIntervalMs.ToString(CultureInfo.InvariantCulture);
            VirtualKeyboardEnabled = options.VirtualKeyboardEnabled;
            AgvBaseUrl = options.AgvBaseUrl;

            AgvAutoCallEnabled = options.AgvAutoCallEnabled;
            AgvKe1AutoCallRemainingBelow = options.AgvKe1AutoCallRemainingBelow.ToString(CultureInfo.InvariantCulture);
            AgvKe2AutoCallRemainingBelow = options.AgvKe2AutoCallRemainingBelow.ToString(CultureInfo.InvariantCulture);
        }
        finally
        {
            _isApplyingLoadedValues = false;
        }

        OnPropertyChanged(nameof(ActiveMachineText));
    }

    private void ApplyParameterFields(string groupName, IReadOnlyList<EditablePlcParameterField> fields)
    {
        _isApplyingParameterValues = true;

        try
        {
            var targetCollection = groupName switch
            {
                PlcParameterGroups.DataTrayCart => DataTrayCartParameters,
                PlcParameterGroups.DataMachine => DataMachineParameters,
                _ => throw new ArgumentOutOfRangeException(nameof(groupName))
            };
            targetCollection.Clear();

            foreach (var field in fields)
            {
                field.LastSavedValueText = field.ValueText;
                field.ValidationMessage = string.Empty;
                field.IsSyncedWithPlc = _plcParameterSyncService.IsTagSynced(field.TagName);
                targetCollection.Add(field);
            }
        }
        finally
        {
            _isApplyingParameterValues = false;
        }
    }

    private void OnFormValueChanged()
    {
        if (_isApplyingLoadedValues)
        {
            return;
        }

        ValidationMessage = string.Empty;
        UpdateDirtyState();
        OnPropertyChanged(nameof(ActiveMachineText));
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        PostToUiThread(RefreshIsEditable);
    }

    private void RefreshIsEditable()
    {
        IsEditable = AppSession.CanEditSettings;
    }

    private void UpdateDirtyState()
    {
        IsDirty =
            !string.Equals(MachineName.Trim(), _loadedOptions.MachineName, StringComparison.Ordinal)
            || !string.Equals(MachineCode.Trim(), _loadedOptions.MachineCode, StringComparison.Ordinal)
            || !string.Equals(Description.Trim(), _loadedOptions.Description, StringComparison.Ordinal)
            || !string.Equals(PlcHost.Trim(), _loadedOptions.PlcHost, StringComparison.Ordinal)
            || !string.Equals(ApiBaseUrl.Trim(), _loadedOptions.ApiBaseUrl, StringComparison.Ordinal)
            || !string.Equals(PlcPort.Trim(), _loadedOptions.PlcPort.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            || !string.Equals(PlcId.Trim(), _loadedOptions.PlcSlaveId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            || !string.Equals(SelectedPlcType.Trim(), NormalizePlcType(_loadedOptions.PlcConnectionMode), StringComparison.Ordinal)
            || !string.Equals(ScanRateMs.Trim(), _loadedOptions.PollIntervalMs.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            || VirtualKeyboardEnabled != _loadedOptions.VirtualKeyboardEnabled
            || !string.Equals(AgvBaseUrl.Trim(), _loadedOptions.AgvBaseUrl, StringComparison.Ordinal)

            || AgvAutoCallEnabled != _loadedOptions.AgvAutoCallEnabled
            || !string.Equals(AgvKe1AutoCallRemainingBelow.Trim(), _loadedOptions.AgvKe1AutoCallRemainingBelow.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            || !string.Equals(AgvKe2AutoCallRemainingBelow.Trim(), _loadedOptions.AgvKe2AutoCallRemainingBelow.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private bool TryBuildOptions(out AppOptions options, out string errorMessage)
    {
        var normalizedMachineName = MachineName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMachineName))
        {
            options = null!;
            errorMessage = "Tên máy là bắt buộc.";
            return false;
        }

        var normalizedMachineCode = MachineCode.Trim();
        if (string.IsNullOrWhiteSpace(normalizedMachineCode))
        {
            options = null!;
            errorMessage = "Machine Code là bắt buộc.";
            return false;
        }

        var normalizedPlcHost = PlcHost.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPlcHost))
        {
            options = null!;
            errorMessage = "IP PLC là bắt buộc.";
            return false;
        }

        if (!IsValidHost(normalizedPlcHost))
        {
            options = null!;
            errorMessage = "IP PLC phải là host hoặc địa chỉ IP hợp lệ.";
            return false;
        }

        if (!int.TryParse(PlcPort.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedPlcPort)
            || normalizedPlcPort is < 1 or > 65535)
        {
            options = null!;
            errorMessage = "Port PLC phải là số nguyên trong khoảng 1 đến 65535.";
            return false;
        }

        var normalizedPlcType = NormalizePlcType(SelectedPlcType);
        if (!SupportedPlcTypes.Contains(normalizedPlcType, StringComparer.Ordinal))
        {
            options = null!;
            errorMessage = "Type PLC phải là DVP hoặc AS.";
            return false;
        }

        if (!int.TryParse(PlcId.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedPlcSlaveId)
            || normalizedPlcSlaveId is < 1 or > 247)
        {
            options = null!;
            errorMessage = "PLC ID phải là số nguyên trong khoảng 1 đến 247.";
            return false;
        }

        if (!int.TryParse(ScanRateMs.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedPollIntervalMs)
            || normalizedPollIntervalMs is < 50 or > 5000)
        {
            options = null!;
            errorMessage = "Scan rate PLC (ms) phải là số nguyên trong khoảng 50 đến 5000.";
            return false;
        }

        var normalizedApiBaseUrl = ApiBaseUrl.Trim();
        if (string.IsNullOrWhiteSpace(normalizedApiBaseUrl))
        {
            options = null!;
            errorMessage = "UrlBase Server là bắt buộc.";
            return false;
        }

        if (!Uri.TryCreate(normalizedApiBaseUrl, UriKind.Absolute, out var apiBaseUri)
            || apiBaseUri.Scheme is not ("http" or "https"))
        {
            options = null!;
            errorMessage = "UrlBase Server phải là địa chỉ http/https hợp lệ.";
            return false;
        }

        options = _loadedOptions.Clone();
        options.MachineName = normalizedMachineName;
        options.MachineCode = normalizedMachineCode;
        options.Description = Description.Trim();
        options.PlcHost = normalizedPlcHost;
        options.PlcPort = normalizedPlcPort;
        options.PlcConnectionMode = normalizedPlcType;
        options.PlcSlaveId = normalizedPlcSlaveId;
        options.PollIntervalMs = normalizedPollIntervalMs;
        options.ApiBaseUrl = apiBaseUri.ToString().TrimEnd('/');
        options.VirtualKeyboardEnabled = VirtualKeyboardEnabled;


        // --- AGV ---
        if (!int.TryParse(AgvKe1AutoCallRemainingBelow.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedAgvKe1Threshold) || normalizedAgvKe1Threshold < 0)
        {
            normalizedAgvKe1Threshold = 0;
        }

        if (!int.TryParse(AgvKe2AutoCallRemainingBelow.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedAgvKe2Threshold) || normalizedAgvKe2Threshold < 0)
        {
            normalizedAgvKe2Threshold = 0;
        }

        options.AgvBaseUrl = AgvBaseUrl.Trim();

        options.AgvAutoCallEnabled = AgvAutoCallEnabled;
        options.AgvKe1AutoCallRemainingBelow = normalizedAgvKe1Threshold;
        options.AgvKe2AutoCallRemainingBelow = normalizedAgvKe2Threshold;

        errorMessage = string.Empty;
        return true;
    }

    private void OnParameterCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<EditablePlcParameterField>())
            {
                item.PropertyChanged -= OnParameterFieldPropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<EditablePlcParameterField>())
            {
                item.PropertyChanged += OnParameterFieldPropertyChanged;
            }
        }
    }

    private async void OnParameterFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isApplyingParameterValues
            || e.PropertyName != nameof(EditablePlcParameterField.ValueText)
            || sender is not EditablePlcParameterField field)
        {
            return;
        }

        await SaveParameterFieldAsync(field);
    }

    private async Task SaveParameterFieldAsync(EditablePlcParameterField field)
    {
        if (string.Equals(field.ValueText.Trim(), field.LastSavedValueText, StringComparison.Ordinal))
        {
            field.ValidationMessage = string.Empty;
            field.IsSyncedWithPlc = _plcParameterSyncService.IsTagSynced(field.TagName);
            return;
        }

        if (!PlcTagValueTextConverter.TryParse(field.Definition, field.ValueText, out var typedValue, out var errorMessage))
        {
            field.ValidationMessage = errorMessage;
            field.IsSyncedWithPlc = false;
            return;
        }

        try
        {
            field.ValidationMessage = string.Empty;
            await _plcParameterSyncService.SetDesiredValueAsync(field.TagName, typedValue);

            var normalizedValue = PlcTagValueTextConverter.Format(field.Definition, typedValue);
            _isApplyingParameterValues = true;
            field.ValueText = normalizedValue;
            _isApplyingParameterValues = false;
            field.LastSavedValueText = normalizedValue;
            field.IsSyncedWithPlc = _plcParameterSyncService.IsTagSynced(field.TagName);
        }
        catch (Exception exception)
        {
            _isApplyingParameterValues = false;
            field.ValidationMessage = $"Không thể lưu: {exception.Message}";
            field.IsSyncedWithPlc = false;
        }
    }

    private void OnSyncStatesChanged(object? sender, PlcParameterSyncChangedEventArgs e)
    {
        PostToUiThread(() =>
        {
            ApplySyncStates(DataTrayCartParameters, e.SyncStates);
            ApplySyncStates(DataMachineParameters, e.SyncStates);
        });
    }

    private static void ApplySyncStates(IEnumerable<EditablePlcParameterField> fields, IReadOnlyDictionary<string, bool> syncStates)
    {
        foreach (var field in fields)
        {
            if (syncStates.TryGetValue(field.TagName, out var isSynced))
            {
                field.IsSyncedWithPlc = isSynced;
            }
        }
    }

    private static void PostToUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _ = dispatcher.BeginInvoke(action);
    }


    private static bool IsValidHost(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains(' ')
            || value.Contains('/')
            || value.Contains('\\'))
        {
            return false;
        }

        return Uri.CheckHostName(value) != UriHostNameType.Unknown;
    }

    private static string NormalizePlcType(string? value)
    {
        if (string.Equals(value, "AS", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "TcpAS", StringComparison.OrdinalIgnoreCase))
        {
            return "AS";
        }

        return "DVP";
    }
}
