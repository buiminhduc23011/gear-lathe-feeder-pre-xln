using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Data.Repositories;
using Desktop.App.Models.Alarms;
using Desktop.App.Services.Abstractions;
using Desktop.App.Session;

namespace Desktop.App.ViewModels.Pages;

public partial class HistoryPageViewModel : ObservableObject, IDisposable
{
    private const int DefaultPageSize = 10;

    private readonly IAlarmHistoryRepository _alarmHistoryRepository;
    private readonly IAlarmMonitorService _alarmMonitorService;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationDialogService _notificationDialog;
    private readonly DispatcherTimer _durationTimer;
    private readonly DispatcherTimer _searchDebounceTimer;
    private bool _disposed;
    private bool _isInitialized;

    public HistoryPageViewModel(
        IAlarmHistoryRepository alarmHistoryRepository,
        IAlarmMonitorService alarmMonitorService,
        IAuditLogRepository auditLogRepository,
        INotificationDialogService notificationDialog)
    {
        _alarmHistoryRepository = alarmHistoryRepository;
        _alarmMonitorService = alarmMonitorService;
        _auditLogRepository = auditLogRepository;
        _notificationDialog = notificationDialog;

        StatusOptions =
        [
            new FilterOption<AlarmRecordStatus?>("Chọn trạng thái", null),
            new FilterOption<AlarmRecordStatus?>("Đang hoạt động", AlarmRecordStatus.Active),
            new FilterOption<AlarmRecordStatus?>("Đã kết thúc", AlarmRecordStatus.Resolved),
        ];

        TypeOptions =
        [
            new FilterOption<AlarmType?>("Chọn loại lỗi", null),
            new FilterOption<AlarmType?>("Alarm", AlarmType.Alarm),
            new FilterOption<AlarmType?>("Error", AlarmType.Error),
            new FilterOption<AlarmType?>("System", AlarmType.System),
        ];

        selectedStatusOption = StatusOptions[0];
        selectedTypeOption = TypeOptions[0];

        Records = new ObservableCollection<AlarmHistoryRowItem>();

        _alarmMonitorService.StateChanged += OnAlarmStateChanged;
        AppSession.SessionChanged += OnSessionChanged;
        _durationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _durationTimer.Tick += (_, _) => RefreshDurations();
        _durationTimer.Start();

        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500),
        };
        _searchDebounceTimer.Tick += async (_, _) =>
        {
            _searchDebounceTimer.Stop();
            PageIndex = 1;
            await LoadAsync();
        };
    }

    public ObservableCollection<AlarmHistoryRowItem> Records { get; }

    public IReadOnlyList<FilterOption<AlarmRecordStatus?>> StatusOptions { get; }

    public IReadOnlyList<FilterOption<AlarmType?>> TypeOptions { get; }

    [ObservableProperty]
    private string title = "Lịch sử lỗi";

    [ObservableProperty]
    private string subtitle = "Lịch sử cảnh báo và hướng dẫn xử lý sự cố.";

    [ObservableProperty]
    private string searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    [ObservableProperty]
    private FilterOption<AlarmRecordStatus?> selectedStatusOption = null!;

    [ObservableProperty]
    private FilterOption<AlarmType?> selectedTypeOption = null!;

    [ObservableProperty]
    private DateTime? fromDate;

    [ObservableProperty]
    private DateTime? toDate;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string activeCountText = "0";

    [ObservableProperty]
    private string stopMachineActiveCountText = "0";

    [ObservableProperty]
    private string todayCountText = "0";

    [ObservableProperty]
    private string dataSourceText = "SQLite runtime log";

    [ObservableProperty]
    private string emptyStateText = "";

    [ObservableProperty]
    private bool isAdmin = AppSession.IsAdmin;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int pageIndex = 1;

    [ObservableProperty]
    private int maxPageCount = 1;

    public string PageStatusText => $"Trang {PageIndex}/{MaxPageCount}";

    public bool CanGoPreviousPage => PageIndex > 1;

    public bool CanGoNextPage => PageIndex < MaxPageCount;

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

    public async Task InitializeAsync()
    {
        if (_isInitialized || _disposed)
        {
            return;
        }

        _isInitialized = true;
        await LoadAsync();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _alarmMonitorService.StateChanged -= OnAlarmStateChanged;
        AppSession.SessionChanged -= OnSessionChanged;
        _durationTimer.Stop();
        _searchDebounceTimer.Stop();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        IsAdmin = AppSession.IsAdmin;
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        _searchDebounceTimer.Stop();
        SelectedStatusOption = StatusOptions[0];
        SelectedTypeOption = TypeOptions[0];
        FromDate = null;
        ToDate = null;
        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task GoToFirstPageAsync()
    {
        if (PageIndex == 1)
        {
            return;
        }

        PageIndex = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPreviousPage)
        {
            return;
        }

        PageIndex--;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (!CanGoNextPage)
        {
            return;
        }

        PageIndex++;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task GoToLastPageAsync()
    {
        if (PageIndex == MaxPageCount)
        {
            return;
        }

        PageIndex = MaxPageCount;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteHistoryAsync()
    {
        if (!AppSession.IsAdmin) return;

        var filterParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(SearchText)) filterParts.Add($"từ khóa '{SearchText}'");
        if (SelectedStatusOption.Value is not null) filterParts.Add($"trạng thái '{SelectedStatusOption.Label}'");
        if (SelectedTypeOption.Value is not null) filterParts.Add($"loại '{SelectedTypeOption.Label}'");
        if (FromDate.HasValue) filterParts.Add($"từ {FromDate.Value:dd/MM/yyyy}");
        if (ToDate.HasValue) filterParts.Add($"đến {ToDate.Value:dd/MM/yyyy}");
        var filterDesc = filterParts.Count > 0 ? $" ({string.Join(", ", filterParts)})" : string.Empty;

        var confirmed = await _notificationDialog.ShowConfirmAsync(
            "Xác nhận xóa",
            $"Bạn có chắc chắn muốn xóa lịch sử lỗi{filterDesc}?\nHành động này không thể hoàn tác.");

        if (!confirmed) return;

        try
        {
            var query = new AlarmHistoryQuery
            {
                SearchTag = SearchText,
                Status = SelectedStatusOption.Value,
                Type = SelectedTypeOption.Value,
                FromLocalDate = FromDate,
                ToLocalDate = ToDate,
            };

            var deletedCount = await _alarmHistoryRepository.DeleteByFilterAsync(query);

            await _auditLogRepository.LogAsync(
                "DELETE_ALARM_HISTORY",
                AppSession.CurrentUserName ?? "unknown",
                $"Đã xóa {deletedCount} bản ghi{filterDesc}");

            PageIndex = 1;
            await LoadAsync();

            await _notificationDialog.ShowSuccessAsync("Thành công", $"Đã xóa {deletedCount} bản ghi lịch sử lỗi.");
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể xóa lịch sử lỗi.\n{ex.Message}");
        }
    }

    private void OnAlarmStateChanged(object? sender, AlarmStateChangedEventArgs e)
    {
        _ = InvokeOnUiThreadAsync(LoadAsync);
    }

    private async Task LoadAsync()
    {
        if (_disposed)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var query = new AlarmHistoryQuery
            {
                SearchTag = SearchText,
                Status = SelectedStatusOption.Value,
                Type = SelectedTypeOption.Value,
                FromLocalDate = FromDate,
                ToLocalDate = ToDate,
                PageNumber = PageIndex,
                PageSize = DefaultPageSize,
            };

            var page = await _alarmHistoryRepository.QueryAsync(query);
            var summary = await _alarmHistoryRepository.GetSummaryAsync(DateTime.Now);

            var maxPageCount = Math.Max(1, (int)Math.Ceiling((double)page.TotalCount / DefaultPageSize));
            if (PageIndex > maxPageCount)
            {
                PageIndex = maxPageCount;
                await LoadAsync();
                return;
            }

            TotalCount = page.TotalCount;
            MaxPageCount = maxPageCount;
            ApplyRecords(page.Items);
            ApplySummary(summary);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyRecords(IReadOnlyList<AlarmHistoryItem> items)
    {
        Records.Clear();

        var index = ((PageIndex - 1) * DefaultPageSize) + 1;
        foreach (var item in items)
        {
            Records.Add(AlarmHistoryRowItem.From(index++, item, DateTime.Now));
        }

        RefreshDurations();
    }

    private void ApplySummary(AlarmHistorySummary summary)
    {
        ActiveCountText = summary.ActiveCount.ToString();
        StopMachineActiveCountText = summary.StopMachineActiveCount.ToString();
        TodayCountText = summary.TodayCount.ToString();
    }

    private void RefreshDurations()
    {
        var now = DateTime.Now;
        foreach (var item in Records)
        {
            item.RefreshDuration(now);
        }
    }

    private static Task InvokeOnUiThreadAsync(Func<Task> action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            return action();
        }

        return dispatcher.InvokeAsync(action).Task.Unwrap();
    }
}

public sealed partial class AlarmHistoryRowItem : ObservableObject
{
    private DateTime _startedAtLocal;
    private DateTime? _endedAtLocal;
    private int? _resolvedDurationSeconds;

    private AlarmHistoryRowItem()
    {
    }

    public int Index { get; private init; }

    public string Tag { get; private init; } = string.Empty;

    public string Description { get; private init; } = string.Empty;

    public string Remedy { get; private init; } = string.Empty;

    public string TypeText { get; private init; } = string.Empty;

    public string StatusText { get; private init; } = string.Empty;

    public string StartedAtText { get; private init; } = string.Empty;

    public string EndedAtText { get; private init; } = "-";

    public bool IsActive { get; private init; }

    [ObservableProperty]
    private int durationSeconds;

    [ObservableProperty]
    private string durationText = "0s";

    public static AlarmHistoryRowItem From(int index, AlarmHistoryItem item, DateTime localNow)
    {
        var startedAtLocal = item.StartedAtUtc.ToLocalTime();
        var endedAtLocal = item.EndedAtUtc?.ToLocalTime();

        var row = new AlarmHistoryRowItem
        {
            Index = index,
            Tag = item.Address,
            Description = item.Description,
            Remedy = item.Remedy,
            TypeText = item.AlarmType.ToString(),
            StatusText = item.Status == AlarmRecordStatus.Active ? "Đang hoạt động" : "Đã kết thúc",
            StartedAtText = startedAtLocal.ToString("dd/MM/yyyy HH:mm:ss"),
            EndedAtText = endedAtLocal?.ToString("dd/MM/yyyy HH:mm:ss") ?? "-",
            IsActive = item.Status == AlarmRecordStatus.Active,
            _startedAtLocal = startedAtLocal,
            _endedAtLocal = endedAtLocal,
            _resolvedDurationSeconds = item.DurationSeconds,
        };

        row.RefreshDuration(localNow);
        return row;
    }

    public void RefreshDuration(DateTime nowLocal)
    {
        DurationSeconds = IsActive
            ? Math.Max(0, (int)(nowLocal - _startedAtLocal).TotalSeconds)
            : Math.Max(0, _resolvedDurationSeconds ?? (int)((_endedAtLocal ?? nowLocal) - _startedAtLocal).TotalSeconds);

        DurationText = FormatDuration(DurationSeconds);
    }

    private static string FormatDuration(int totalSeconds)
    {
        if (totalSeconds < 60)
        {
            return $"{totalSeconds}s";
        }

        var timeSpan = TimeSpan.FromSeconds(totalSeconds);
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes:D2}m";
        }

        return $"{timeSpan.Minutes}m {timeSpan.Seconds:D2}s";
    }
}

public sealed class FilterOption<T>
{
    public FilterOption(string label, T value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }

    public T Value { get; }

    public override string ToString()
    {
        return Label;
    }
}
