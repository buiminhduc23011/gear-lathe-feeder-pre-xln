using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Desktop.App.Helpers;
using Desktop.App.Models.Ui;
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
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
        _viewModel.Dispose();
    }

    internal void ParameterValueInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && textBox.DataContext is EditablePlcParameterField field)
        {
            textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            _ = _viewModel.SaveParameterFieldCommand.ExecuteAsync(field);
        }
    }

    internal void ParameterValueInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            if (sender is TextBox textBox && textBox.DataContext is EditablePlcParameterField field)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                _ = _viewModel.SaveParameterFieldCommand.ExecuteAsync(field);
                var scope = FocusManager.GetFocusScope(textBox);
                if (scope is not null)
                {
                    FocusManager.SetFocusedElement(scope, null);
                }
                Keyboard.ClearFocus();
                e.Handled = true;
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
