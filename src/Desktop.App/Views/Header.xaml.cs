using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Desktop.App.Views;

public partial class Header : UserControl
{
    private Window? _window;
    private bool _isSyncingWindowState;

    public Header()
    {
        InitializeComponent();
        DataContext = ((App)Application.Current).HeaderViewModel;

        Loaded += Header_Loaded;
        Unloaded += Header_Unloaded;
    }

    private void Header_Loaded(object sender, RoutedEventArgs e)
    {
        _window = Window.GetWindow(this);

        if (_window is null)
        {
            return;
        }

        _window.StateChanged += Window_StateChanged;
        SyncWindowState();
    }

    private void Header_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_window is null)
        {
            return;
        }

        _window.StateChanged -= Window_StateChanged;
        _window = null;
    }

    private void HeaderRoot_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_window is null)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            if (_window.WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(_window);
            }
            else
            {
                SystemCommands.MaximizeWindow(_window);
            }
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            // Only allow dragging when NOT maximized to avoid exceptions
            if (_window.WindowState != WindowState.Maximized)
            {
                _window.DragMove();
            }
        }
    }

    private void MinButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_window is not null)
        {
            SystemCommands.MinimizeWindow(_window);
        }
    }

    private void MaxToggle_OnChecked(object sender, RoutedEventArgs e)
    {
        if (_isSyncingWindowState)
        {
            return;
        }

        if (_window is not null)
        {
            SystemCommands.MaximizeWindow(_window);
        }
    }

    private void MaxToggle_OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (_isSyncingWindowState)
        {
            return;
        }

        if (_window is not null)
        {
            SystemCommands.RestoreWindow(_window);
        }
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_window is not null)
        {
            SystemCommands.CloseWindow(_window);
        }
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        SyncWindowState();
    }

    private void SyncWindowState()
    {
        if (_window is not null)
        {
            _isSyncingWindowState = true;
            try
            {
                MaxToggle.IsChecked = _window.WindowState == WindowState.Maximized;
            }
            finally
            {
                _isSyncingWindowState = false;
            }
        }
    }

    private void OpenWebsite_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://stivietnam.com/",
                UseShellExecute = true,
            });
        }
        catch
        {
            MessageBox.Show("Khong the mo website cong ty.", "Loi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UtilityMenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.ContextMenu is not ContextMenu contextMenu)
        {
            return;
        }

        contextMenu.PlacementTarget = button;
        contextMenu.IsOpen = true;
    }
}
