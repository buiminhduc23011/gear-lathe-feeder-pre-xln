using System.Windows;
using System.Windows.Controls;
using Desktop.App.ViewModels.Pages;

namespace Desktop.App.Views.Pages;

public partial class GpioPage : UserControl
{
    private readonly GpioPageViewModel _viewModel;

    public GpioPage()
    {
        InitializeComponent();

        var app = Application.Current as App
            ?? throw new InvalidOperationException("Desktop application context is not available.");

        _viewModel = new GpioPageViewModel(app.PlcService);
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
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        _viewModel.Dispose();
    }
}
