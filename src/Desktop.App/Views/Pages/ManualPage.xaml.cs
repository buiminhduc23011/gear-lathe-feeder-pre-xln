using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Desktop.App.Helpers;
using Desktop.App.Models.Ui;
using Desktop.App.ViewModels.Pages;

namespace Desktop.App.Views.Pages;

public partial class ManualPage : UserControl
{
    private readonly ManualPageViewModel _viewModel;

    public ManualPage()
    {
        InitializeComponent();

        var app = Application.Current as App
            ?? throw new InvalidOperationException("Desktop application context is not available.");

        _viewModel = new ManualPageViewModel(app.PlcService, app.PlcParameterSettingsService, app.NotificationDialogService);
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

    internal void SpeedInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is ManualAxisState axis)
        {
            _viewModel.ApplyAxisSpeedCommand.Execute(axis);
        }
    }

    internal void MovePointInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is ManualAxisState axis)
        {
            _viewModel.WriteMovePointValueCommand.Execute(axis);
        }
    }

    internal void NumericField_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            e.Handled = !NumericTextBoxInputHelper.IsProposedTextValid(textBox, e.Text);
        }
    }

    internal void NumericField_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        NumericTextBoxInputHelper.HandlePaste(sender, e);
    }
}
