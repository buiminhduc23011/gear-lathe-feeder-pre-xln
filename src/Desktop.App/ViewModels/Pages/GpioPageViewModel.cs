using System.Collections.ObjectModel;
using System.Globalization;
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

public partial class GpioPageViewModel : ObservableObject, IDisposable
{
    private const int DefaultPageSize = 10;

    private readonly Dictionary<string, GpioIoItem> _itemsByTagName;
    private readonly List<PlcTagDisplayItem> _allTags;
    private readonly List<PlcTagDisplayItem> _allTagsLine1;
    private readonly List<PlcTagDisplayItem> _allTagsLine2;
    private readonly object _pendingSnapshotLock = new();
    private readonly IPlcService _plcService;
    private readonly IPlcService? _plcServiceLine1;
    private readonly IPlcService? _plcServiceLine2;
    private readonly Dictionary<string, PlcTagDisplayItem> _tagsByTagName;
    private readonly Dictionary<string, PlcTagDisplayItem> _tagsByTagNameLine1;
    private readonly Dictionary<string, PlcTagDisplayItem> _tagsByTagNameLine2;
    private readonly DispatcherTimer _searchDebounceTimer;
    private bool _isDisposed;
    private bool _isInitialized;
    private bool _isInitializing;
    private bool _isSnapshotApplyQueued;
    private IReadOnlyDictionary<string, object?>? _pendingSnapshot;
    private readonly int _pageSize = DefaultPageSize;

    public GpioPageViewModel(IPlcService plcService, IPlcService? plcServiceLine1 = null, IPlcService? plcServiceLine2 = null)
    {
        _plcService = plcService;
        _plcServiceLine1 = plcServiceLine1;
        _plcServiceLine2 = plcServiceLine2;

        Inputs = CreateItems(typeof(PlcTagCatalog.Inputs));
        Outputs = CreateItems(typeof(PlcTagCatalog.Outputs));
        _itemsByTagName = Inputs.Concat(Outputs).ToDictionary(item => item.TagName, StringComparer.OrdinalIgnoreCase);

        _allTags = CreateTagItems(PlcTagCatalog.All, _plcService);
        _tagsByTagName = _allTags.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        _allTagsLine1 = CreateTagItems(PlcTagCatalog.AllLine1, _plcServiceLine1);
        _tagsByTagNameLine1 = _allTagsLine1.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        _allTagsLine2 = CreateTagItems(PlcTagCatalog.AllLine2, _plcServiceLine2);
        _tagsByTagNameLine2 = _allTagsLine2.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        Tags = new ObservableCollection<PlcTagDisplayItem>();

        SyncStatesFromCache(markTagRowsAsSynchronized: false);
        ApplyFilter();
        UpdateConnectionState(_plcService.IsConnected);

        _plcService.ConnectionChanged += OnConnectionChanged;
        _plcService.DataUpdated += OnDataUpdated;

        if (_plcServiceLine1 is not null)
        {
            _plcServiceLine1.ConnectionChanged += OnLine1ConnectionChanged;
            _plcServiceLine1.DataUpdated += OnLine1DataUpdated;
        }

        if (_plcServiceLine2 is not null)
        {
            _plcServiceLine2.ConnectionChanged += OnLine2ConnectionChanged;
            _plcServiceLine2.DataUpdated += OnLine2DataUpdated;
        }

        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };
        _searchDebounceTimer.Tick += (_, _) =>
        {
            _searchDebounceTimer.Stop();
            PageIndex = 1;
            ApplyFilter();
        };
    }

    public ObservableCollection<GpioIoItem> Inputs { get; }

    public ObservableCollection<GpioIoItem> Outputs { get; }

    [ObservableProperty]
    private ObservableCollection<PlcTagDisplayItem> tags = new();

    public string InputSummary => $"{Inputs.Count} inputs";

    public string OutputSummary => $"{Outputs.Count} outputs";

    public string ConnectionBadgeText => IsConnected ? "ONLINE" : "OFFLINE";

    public string VisibleTagSummary => $"{TotalCount} tags";

    public string PageStatusText => $"Trang {PageIndex}/{MaxPageCount}";

    public bool CanGoPreviousPage => PageIndex > 1;

    public bool CanGoNextPage => PageIndex < MaxPageCount;

    [ObservableProperty]
    private bool isInputTabSelected = true;

    [ObservableProperty]
    private bool isOutputTabSelected;

    [ObservableProperty]
    private bool isDataPlcTabSelected;

    [ObservableProperty]
    private bool isLine1TabSelected;

    [ObservableProperty]
    private bool isLine2TabSelected;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionText = "PLC disconnected";

    [ObservableProperty]
    private string lastUpdatedText = "Waiting for PLC data";

    [ObservableProperty]
    private string filterText = string.Empty;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int pageIndex = 1;

    [ObservableProperty]
    private int maxPageCount = 1;

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ConnectionBadgeText));
    }

    partial void OnTotalCountChanged(int value)
    {
        OnPropertyChanged(nameof(VisibleTagSummary));
    }

    partial void OnPageIndexChanged(int value)
    {
        OnPropertyChanged(nameof(PageStatusText));
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
    }

    partial void OnMaxPageCountChanged(int value)
    {
        OnPropertyChanged(nameof(PageStatusText));
        OnPropertyChanged(nameof(CanGoPreviousPage));
        OnPropertyChanged(nameof(CanGoNextPage));
    }

    partial void OnFilterTextChanged(string value)
    {
        if (_isInitializing)
        {
            return;
        }

        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    public async Task InitializeAsync()
    {
        if (_isDisposed || _isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _isInitializing = true;
        IReadOnlyDictionary<string, object?>? initialSnapshot = null;

        try
        {
            var robotSnapshotTask = LoadInitialSnapshotAsync(_plcService);
            var line1SnapshotTask = LoadInitialSnapshotAsync(_plcServiceLine1);
            var line2SnapshotTask = LoadInitialSnapshotAsync(_plcServiceLine2);

            await Task.WhenAll(robotSnapshotTask, line1SnapshotTask, line2SnapshotTask).ConfigureAwait(false);

            initialSnapshot = robotSnapshotTask.Result;

            await InvokeOnUiThreadAsync(
                () =>
                {
                    ApplyLineSnapshotFromCacheOrSnapshot(_allTagsLine1, _plcServiceLine1, line1SnapshotTask.Result);
                    ApplyLineSnapshotFromCacheOrSnapshot(_allTagsLine2, _plcServiceLine2, line2SnapshotTask.Result);
                });
        }
        catch
        {
        }

        await InvokeOnUiThreadAsync(
            () =>
            {
                try
                {
                    if (initialSnapshot is { Count: > 0 })
                    {
                        ApplyInitialSnapshot(initialSnapshot);
                    }
                    else
                    {
                        SyncStatesFromCache(markTagRowsAsSynchronized: true);
                        ApplyFilter();
                    }

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

        if (_plcServiceLine1 is not null)
        {
            _plcServiceLine1.ConnectionChanged -= OnLine1ConnectionChanged;
            _plcServiceLine1.DataUpdated -= OnLine1DataUpdated;
        }

        if (_plcServiceLine2 is not null)
        {
            _plcServiceLine2.ConnectionChanged -= OnLine2ConnectionChanged;
            _plcServiceLine2.DataUpdated -= OnLine2DataUpdated;
        }

        _searchDebounceTimer.Stop();
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        PostToUiThread(() => UpdateConnectionState(isConnected));
    }

    private void OnDataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_isDisposed || _isInitializing || e.Snapshot.Count == 0)
        {
            return;
        }

        lock (_pendingSnapshotLock)
        {
            _pendingSnapshot = e.Snapshot;

            if (_isSnapshotApplyQueued)
            {
                return;
            }

            _isSnapshotApplyQueued = true;
        }

        PostToUiThread(ProcessPendingSnapshots);
    }

    private void OnLine1DataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_isDisposed || e.Snapshot.Count == 0)
        {
            return;
        }

        PostToUiThread(() => ApplyLineSnapshot(e.Snapshot, _tagsByTagNameLine1));
    }

    private void OnLine2DataUpdated(object? sender, PlcDataChangedEventArgs e)
    {
        if (_isDisposed || e.Snapshot.Count == 0)
        {
            return;
        }

        PostToUiThread(() => ApplyLineSnapshot(e.Snapshot, _tagsByTagNameLine2));
    }

    private void OnLine1ConnectionChanged(object? sender, bool isConnected)
    {
        HandleLineConnectionChanged(_plcServiceLine1, _allTagsLine1, isConnected);
    }

    private void OnLine2ConnectionChanged(object? sender, bool isConnected)
    {
        HandleLineConnectionChanged(_plcServiceLine2, _allTagsLine2, isConnected);
    }

    private void ApplyLineSnapshot(IReadOnlyDictionary<string, object?> snapshot, Dictionary<string, PlcTagDisplayItem> tagsByName)
    {
        var updatedAt = DateTime.Now;

        foreach (var entry in snapshot)
        {
            if (tagsByName.TryGetValue(entry.Key, out var tagItem))
            {
                UpdateTagValue(tagItem, entry.Value, updatedAt, updateTimestampWhenMissing: true);
            }
        }
    }

    private void ApplyLineSnapshotFromCacheOrSnapshot(
        IEnumerable<PlcTagDisplayItem> tags,
        IPlcService? service,
        IReadOnlyDictionary<string, object?>? snapshot)
    {
        var isReadable = snapshot is not null;
        var updatedAt = isReadable ? DateTime.Now : (DateTime?)null;

        foreach (var item in tags)
        {
            var value = snapshot is not null && snapshot.TryGetValue(item.Name, out var snapshotValue)
                ? snapshotValue
                : service?.GetValue<object?>(item.Name, null);

            UpdateTagValue(item, value, updatedAt, updateTimestampWhenMissing: isReadable);
        }
    }

    private ObservableCollection<GpioIoItem> CreateItems(Type tagGroupType)
    {
        return new ObservableCollection<GpioIoItem>(
            tagGroupType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(field => field.FieldType == typeof(PlcTagDefinition))
                .Select(field => (PlcTagDefinition?)field.GetValue(null))
                .OfType<PlcTagDefinition>()
                .OrderBy(tag => tag.Address, StringComparer.OrdinalIgnoreCase)
                .Select(
                    tag => new GpioIoItem
                    {
                        TagName = tag.Name,
                        Address = tag.Address,
                        Name = tag.Description,
                        State = _plcService.GetValue(tag.Name, false),
                    }));
    }

    private List<PlcTagDisplayItem> CreateTagItems(IReadOnlyList<PlcTagDefinition> catalog, IPlcService? service)
    {
        return catalog
            .Select(
                (tag, index) => new PlcTagDisplayItem
                {
                    Index = index + 1,
                    Name = tag.Name,
                    Address = tag.Address,
                    DataType = tag.DataType.ToString(),
                    Value = FormatValue(service?.GetValue<object?>(tag.Name, null)),
                    Description = tag.Description,
                })
            .ToList();
    }

    private void ApplyInitialSnapshot(IReadOnlyDictionary<string, object?> snapshot)
    {
        var updatedAt = DateTime.Now;

        foreach (var item in _itemsByTagName.Values)
        {
            if (snapshot.TryGetValue(item.TagName, out var value))
            {
                item.State = value is bool boolValue && boolValue;
            }
            else
            {
                item.State = _plcService.GetValue(item.TagName, false);
            }
        }

        foreach (var item in _allTags)
        {
            var value = snapshot.TryGetValue(item.Name, out var snapshotValue)
                ? snapshotValue
                : _plcService.GetValue<object?>(item.Name, null);

            UpdateTagValue(item, value, updatedAt, updateTimestampWhenMissing: true);
        }

        LastUpdatedText = $"Last updated {updatedAt:HH:mm:ss}";
        ApplyFilter();
    }

    private void ApplySnapshot(IReadOnlyDictionary<string, object?> snapshot)
    {
        var updatedAt = DateTime.Now;

        foreach (var entry in snapshot)
        {
            if (_itemsByTagName.TryGetValue(entry.Key, out var item))
            {
                item.State = entry.Value is bool boolValue && boolValue;
            }

            if (_tagsByTagName.TryGetValue(entry.Key, out var tagItem))
            {
                UpdateTagValue(tagItem, entry.Value, updatedAt, updateTimestampWhenMissing: true);
            }
        }

        LastUpdatedText = $"Last updated {updatedAt:HH:mm:ss}";
    }

    private void ProcessPendingSnapshots()
    {
        while (true)
        {
            IReadOnlyDictionary<string, object?>? snapshot;

            lock (_pendingSnapshotLock)
            {
                snapshot = _pendingSnapshot;
                _pendingSnapshot = null;

                if (snapshot is null)
                {
                    _isSnapshotApplyQueued = false;
                    return;
                }
            }

            ApplySnapshot(snapshot);
        }
    }

    private void SyncStatesFromCache(bool markTagRowsAsSynchronized)
    {
        foreach (var item in _itemsByTagName.Values)
        {
            item.State = _plcService.GetValue(item.TagName, false);
        }

        SyncTagRowsFromCache(markTagRowsAsSynchronized);

        if (_plcService.IsConnected)
        {
            LastUpdatedText = $"Last updated {DateTime.Now:HH:mm:ss}";
        }
    }

    private void UpdateConnectionState(bool connected)
    {
        IsConnected = connected;
        ConnectionText = connected ? "PLC connected" : "PLC disconnected";

        if (!connected)
        {
            foreach (var item in _allTags)
            {
                item.Status = "NG";
            }
        }
    }

    private void HandleLineConnectionChanged(IPlcService? service, IEnumerable<PlcTagDisplayItem> tags, bool isConnected)
    {
        if (_isDisposed)
        {
            return;
        }

        if (!isConnected)
        {
            PostToUiThread(() =>
            {
                foreach (var item in tags)
                {
                    item.Status = "NG";
                }
            });

            return;
        }

        _ = RefreshLineSnapshotAsync(service, tags);
    }

    private void SyncTagRowsFromCache(bool markTagRowsAsSynchronized)
    {
        var updatedAt = markTagRowsAsSynchronized ? DateTime.Now : (DateTime?)null;

        foreach (var item in _allTags)
        {
            UpdateTagValue(
                item,
                _plcService.GetValue<object?>(item.Name, null),
                updatedAt,
                updateTimestampWhenMissing: markTagRowsAsSynchronized);
        }
    }

    private void ApplyFilter()
    {
        var filteredTags = GetFilteredTags().ToList();
        TotalCount = filteredTags.Count;
        MaxPageCount = Math.Max(1, (int)Math.Ceiling((double)TotalCount / _pageSize));

        if (PageIndex < 1)
        {
            PageIndex = 1;
        }
        else if (PageIndex > MaxPageCount)
        {
            PageIndex = MaxPageCount;
        }

        Tags = new ObservableCollection<PlcTagDisplayItem>(
            filteredTags
                .Skip((PageIndex - 1) * _pageSize)
                .Take(_pageSize));
    }

    private IEnumerable<PlcTagDisplayItem> GetFilteredTags()
    {
        var source = IsLine1TabSelected ? _allTagsLine1
            : IsLine2TabSelected ? _allTagsLine2
            : _allTags;

        if (string.IsNullOrWhiteSpace(FilterText))
        {
            return source;
        }

        var filter = FilterText.Trim();
        return source.Where(
            item =>
                item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.Address.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || item.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    private static void UpdateTagValue(PlcTagDisplayItem item, object? value, DateTime? updatedAt, bool updateTimestampWhenMissing)
    {
        var formattedValue = FormatValue(value);
        var valueChanged = !string.Equals(item.Value, formattedValue, StringComparison.Ordinal);

        if (valueChanged)
        {
            item.Value = formattedValue;
        }

        if (updatedAt.HasValue && (valueChanged || (updateTimestampWhenMissing && item.LastUpdatedTime is null)))
        {
            item.LastUpdatedTime = updatedAt.Value;
            item.Status = "OK";
        }
    }

    [RelayCommand]
    private void GoToFirstPage()
    {
        if (PageIndex == 1)
        {
            return;
        }

        PageIndex = 1;
        ApplyFilter();
    }

    [RelayCommand]
    private void GoToPreviousPage()
    {
        if (!CanGoPreviousPage)
        {
            return;
        }

        PageIndex--;
        ApplyFilter();
    }

    [RelayCommand]
    private void GoToNextPage()
    {
        if (!CanGoNextPage)
        {
            return;
        }

        PageIndex++;
        ApplyFilter();
    }

    [RelayCommand]
    private void GoToLastPage()
    {
        if (PageIndex == MaxPageCount)
        {
            return;
        }

        PageIndex = MaxPageCount;
        ApplyFilter();
    }

    [RelayCommand]
    private void SelectInputTab()
    {
        SetActiveTab(input: true, output: false, dataPlc: false, line1: false, line2: false);
    }

    [RelayCommand]
    private void SelectOutputTab()
    {
        SetActiveTab(input: false, output: true, dataPlc: false, line1: false, line2: false);
    }

    [RelayCommand]
    private void SelectDataPlcTab()
    {
        SetActiveTab(input: false, output: false, dataPlc: true, line1: false, line2: false);
    }

    [RelayCommand]
    private void SelectLine1Tab()
    {
        SetActiveTab(input: false, output: false, dataPlc: false, line1: true, line2: false);
    }

    [RelayCommand]
    private void SelectLine2Tab()
    {
        SetActiveTab(input: false, output: false, dataPlc: false, line1: false, line2: true);
    }

    private void SetActiveTab(bool input, bool output, bool dataPlc, bool line1, bool line2)
    {
        IsInputTabSelected = input;
        IsOutputTabSelected = output;
        IsDataPlcTabSelected = dataPlc;
        IsLine1TabSelected = line1;
        IsLine2TabSelected = line2;
        PageIndex = 1;
        ApplyFilter();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string stringValue => stringValue,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }

    private async Task RefreshLineSnapshotAsync(IPlcService? service, IEnumerable<PlcTagDisplayItem> tags)
    {
        if (_isDisposed || service is null)
        {
            return;
        }

        var snapshot = await LoadInitialSnapshotAsync(service).ConfigureAwait(false);
        if (_isDisposed)
        {
            return;
        }

        await InvokeOnUiThreadAsync(() => ApplyLineSnapshotFromCacheOrSnapshot(tags, service, snapshot));
    }

    private static async Task<IReadOnlyDictionary<string, object?>?> LoadInitialSnapshotAsync(IPlcService? service)
    {
        if (service is null)
        {
            return null;
        }

        try
        {
            await service.ConnectAsync().ConfigureAwait(false);

            if (service.IsConnected)
            {
                return await service.ReadAllAsync().ConfigureAwait(false);
            }
        }
        catch
        {
        }

        return null;
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
