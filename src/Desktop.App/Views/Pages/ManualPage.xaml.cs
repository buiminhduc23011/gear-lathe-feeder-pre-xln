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
        PreviewMouseDown += OnPreviewMouseDown;
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.FocusedElement is TextBox currentTextBox)
        {
            if (e.OriginalSource is DependencyObject dep)
            {
                var hitTextBox = FindVisualParent<TextBox>(dep);
                if (hitTextBox != currentTextBox)
                {
                    var scope = FocusManager.GetFocusScope(currentTextBox);
                    if (scope is not null)
                    {
                        FocusManager.SetFocusedElement(scope, null);
                    }
                    Keyboard.ClearFocus();
                }
            }
        }
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T parent)
            {
                return parent;
            }
            child = System.Windows.Media.VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        PreviewMouseDown -= OnPreviewMouseDown;
        _viewModel.Dispose();
    }

    internal async void SpeedInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is ManualAxisState axis)
        {
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            await _viewModel.ApplyAxisSpeedCommand.ExecuteAsync(axis);
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }
    }

    internal async void SpeedInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            e.Handled = true;
            if (sender is TextBox textBox && textBox.DataContext is ManualAxisState axis)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                await _viewModel.ApplyAxisSpeedCommand.ExecuteAsync(axis);
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                var scope = FocusManager.GetFocusScope(textBox);
                if (scope is not null)
                {
                    FocusManager.SetFocusedElement(scope, null);
                }
                Keyboard.ClearFocus();
            }
        }
    }

    internal async void MovePointInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is ManualAxisState axis)
        {
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            await _viewModel.WriteMovePointValueCommand.ExecuteAsync(axis);
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
        }
    }

    internal async void MovePointInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            e.Handled = true;
            if (sender is TextBox textBox && textBox.DataContext is ManualAxisState axis)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                await _viewModel.WriteMovePointValueCommand.ExecuteAsync(axis);
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                var scope = FocusManager.GetFocusScope(textBox);
                if (scope is not null)
                {
                    FocusManager.SetFocusedElement(scope, null);
                }
                Keyboard.ClearFocus();
            }
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
