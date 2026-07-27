using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Desktop.App.ViewModels.Pages;

namespace Desktop.App.Views.Pages;

public partial class ReportPage : UserControl
{
    private readonly ReportPageViewModel? _viewModel;

    public ReportPage()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        var app = Application.Current as App
            ?? throw new InvalidOperationException("Desktop application context is not available.");

        _viewModel = new ReportPageViewModel(
            app.AgvCallHistoryRepository,
            app.AuditLogRepository,
            app.NotificationDialogService,
            app.ProductionLifecycleReportApiService,
            app.PlcService);
        DataContext = _viewModel;

        Loaded += async (_, _) =>
        {
            await _viewModel.AgvHistory.LoadDataCommand.ExecuteAsync(null);
            await _viewModel.ProductionLifecycle.LoadDataCommand.ExecuteAsync(null);
        };
        Unloaded += (_, _) => _viewModel.Dispose();
    }
}
