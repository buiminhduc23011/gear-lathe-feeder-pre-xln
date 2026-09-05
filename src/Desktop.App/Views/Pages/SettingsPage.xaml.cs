using System.Windows;
using System.Windows.Controls;
using Desktop.App.ViewModels.Pages;

namespace Desktop.App.Views.Pages;

public partial class SettingsPage : UserControl
{
    private readonly SettingsPageViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();

        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            return;

        var app = Application.Current as App
            ?? throw new InvalidOperationException("Desktop application context is not available.");

        _viewModel = new SettingsPageViewModel(
            app.SettingsService,
            app.PlcParameterSettingsService,
            app.PlcParameterSyncService,
            app.NotificationDialogService);
        DataContext = _viewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Dispose();
    }
}
