using System;
using System.Windows;
using System.Windows.Controls;
using Desktop.App.ViewModels.Pages;

namespace Desktop.App.Views.Pages;

public partial class AgvHistoryPage : UserControl
{
    private readonly AgvHistoryPageViewModel _viewModel;

    public AgvHistoryPage()
    {
        InitializeComponent();
        
        var app = Application.Current as App
            ?? throw new InvalidOperationException("Desktop application context is not available.");

        _viewModel = new AgvHistoryPageViewModel(
            app.AgvCallHistoryRepository,
            app.AuditLogRepository,
            app.NotificationDialogService);
        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
