using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Data.Repositories;
using Desktop.App.Models.Agv;
using Desktop.App.Services.Abstractions;
using Desktop.App.Session;

namespace Desktop.App.ViewModels.Pages;

public partial class AgvHistoryPageViewModel : ObservableObject, IDisposable
{
    private const int DefaultPageSize = 10;

    private readonly IAgvCallHistoryRepository _repository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly INotificationDialogService _notificationDialog;
    private bool _disposed;

    public ObservableCollection<AgvCallRecord> Items { get; } = [];

    [ObservableProperty]
    private DateTime? selectedDate = DateTime.Today;

    [ObservableProperty]
    private int selectedPositionIndex;

    [ObservableProperty]
    private bool isBusy;

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

    public AgvHistoryPageViewModel(
        IAgvCallHistoryRepository repository,
        IAuditLogRepository auditLogRepository,
        INotificationDialogService notificationDialog)
    {
        _repository = repository;
        _auditLogRepository = auditLogRepository;
        _notificationDialog = notificationDialog;

        AppSession.SessionChanged += OnSessionChanged;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        AppSession.SessionChanged -= OnSessionChanged;
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        IsAdmin = AppSession.IsAdmin;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var positionFilter = GetPositionFilter();

            var page = await _repository.GetPagedAsync(positionFilter, SelectedDate, PageIndex, DefaultPageSize);

            var maxPageCount = Math.Max(1, (int)Math.Ceiling((double)page.TotalCount / DefaultPageSize));
            if (PageIndex > maxPageCount)
            {
                PageIndex = maxPageCount;
                await LoadDataAsync();
                return;
            }

            TotalCount = page.TotalCount;
            MaxPageCount = maxPageCount;

            Items.Clear();
            foreach (var record in page.Items)
            {
                Items.Add(record);
            }
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể tải lịch sử AGV.\n{ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        PageIndex = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoToFirstPageAsync()
    {
        if (PageIndex == 1) return;
        PageIndex = 1;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (!CanGoPreviousPage) return;
        PageIndex--;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (!CanGoNextPage) return;
        PageIndex++;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task GoToLastPageAsync()
    {
        if (PageIndex == MaxPageCount) return;
        PageIndex = MaxPageCount;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteHistoryAsync()
    {
        if (!AppSession.IsAdmin) return;

        var positionFilter = GetPositionFilter();
        var filterDesc = BuildFilterDescription(positionFilter, SelectedDate);

        var confirmed = await _notificationDialog.ShowConfirmAsync(
            "Xác nhận xóa",
            $"Bạn có chắc chắn muốn xóa lịch sử gọi AGV{filterDesc}?\nHành động này không thể hoàn tác.");

        if (!confirmed) return;

        try
        {
            var deletedCount = await _repository.DeleteByFilterAsync(positionFilter, SelectedDate);

            await _auditLogRepository.LogAsync(
                "DELETE_AGV_HISTORY",
                AppSession.CurrentUserName ?? "unknown",
                $"Đã xóa {deletedCount} bản ghi{filterDesc}");

            PageIndex = 1;
            await LoadDataAsync();

            await _notificationDialog.ShowSuccessAsync("Thành công", $"Đã xóa {deletedCount} bản ghi lịch sử AGV.");
        }
        catch (Exception ex)
        {
            await _notificationDialog.ShowErrorAsync("Lỗi", $"Không thể xóa lịch sử AGV.\n{ex.Message}");
        }
    }

    private AgvPosition? GetPositionFilter()
    {
        return SelectedPositionIndex switch
        {
            1 => AgvPosition.Position1,
            2 => AgvPosition.Position2,
            _ => null
        };
    }

    private static string BuildFilterDescription(AgvPosition? position, DateTime? date)
    {
        var parts = new List<string>();

        if (position.HasValue)
        {
            parts.Add(position.Value == AgvPosition.Position1 ? "Kệ 1" : "Kệ 2");
        }

        if (date.HasValue)
        {
            parts.Add($"ngày {date.Value:dd/MM/yyyy}");
        }

        return parts.Count > 0 ? $" ({string.Join(", ", parts)})" : string.Empty;
    }
}
