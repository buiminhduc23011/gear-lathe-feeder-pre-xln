using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktop.App.Data.Repositories;
using Desktop.App.Services.Abstractions;

namespace Desktop.App.ViewModels.Pages;

public partial class ReportPageViewModel : ObservableObject, IDisposable
{
    private bool _isSyncingTabSelection;

    [ObservableProperty]
    private string title = "Báo cáo sản xuất";

    [ObservableProperty]
    private string subtitle = "Tổng hợp sản lượng, chất lượng và tiến độ ca vận hành để phục vụ giao ca và phân tích.";

    [ObservableProperty]
    private bool isAgvTabSelected;

    [ObservableProperty]
    private bool isProductionTabSelected = true;

    public bool IsAgvTabVisible => IsAgvTabSelected;

    public bool IsProductionTabVisible => IsProductionTabSelected;

    public AgvHistoryPageViewModel AgvHistory { get; }
    public ProductionLifecycleReportPageViewModel ProductionLifecycle { get; }

    public ReportPageViewModel(
        IAgvCallHistoryRepository agvCallHistoryRepository,
        IAuditLogRepository auditLogRepository,
        INotificationDialogService notificationDialogService,
        IProductionLifecycleReportApiService productionLifecycleReportApiService,
        IPlcService plcService)
    {
        AgvHistory = new AgvHistoryPageViewModel(agvCallHistoryRepository, auditLogRepository, notificationDialogService);
        ProductionLifecycle = new ProductionLifecycleReportPageViewModel(productionLifecycleReportApiService, plcService);
    }

    partial void OnIsAgvTabSelectedChanged(bool value)
    {
        if (_isSyncingTabSelection)
        {
            OnPropertyChanged(nameof(IsAgvTabVisible));
            OnPropertyChanged(nameof(IsProductionTabVisible));
            return;
        }

        _isSyncingTabSelection = true;
        IsProductionTabSelected = !value;
        _isSyncingTabSelection = false;

        OnPropertyChanged(nameof(IsAgvTabVisible));
        OnPropertyChanged(nameof(IsProductionTabVisible));
    }

    partial void OnIsProductionTabSelectedChanged(bool value)
    {
        if (_isSyncingTabSelection)
        {
            OnPropertyChanged(nameof(IsAgvTabVisible));
            OnPropertyChanged(nameof(IsProductionTabVisible));
            return;
        }

        _isSyncingTabSelection = true;
        IsAgvTabSelected = !value;
        _isSyncingTabSelection = false;

        OnPropertyChanged(nameof(IsAgvTabVisible));
        OnPropertyChanged(nameof(IsProductionTabVisible));
    }

    [RelayCommand]
    private void SelectAgvTab()
    {
        IsAgvTabSelected = true;
    }

    [RelayCommand]
    private void SelectProductionTab()
    {
        IsProductionTabSelected = true;
    }

    public void Dispose()
    {
        AgvHistory.Dispose();
        ProductionLifecycle.Dispose();
    }
}
