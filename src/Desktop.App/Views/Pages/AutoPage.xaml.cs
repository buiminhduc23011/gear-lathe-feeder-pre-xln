using System.Windows;
using System.Windows.Controls;
using Desktop.App.Data.Repositories;
using Desktop.App.ViewModels.Pages;

namespace Desktop.App.Views.Pages;

public partial class AutoPage : UserControl
{
    private readonly AutoPageViewModel _viewModel;

    public AutoPage()
    {
        InitializeComponent();
        
        var app = Application.Current as App
            ?? throw new InvalidOperationException("Desktop application context is not available.");

        _viewModel = new AutoPageViewModel(
            app.AgvBackgroundService,
            app.PlcService,
            app.NotificationDialogService,
            app.LoginDialogService,
            app.TrayConfigRepository,
            app.ShelfOrderCacheRepository);

        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}
