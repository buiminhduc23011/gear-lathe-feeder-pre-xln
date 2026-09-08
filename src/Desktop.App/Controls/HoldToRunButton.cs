using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Desktop.App.Controls;

public sealed class HoldToRunButton : Button
{
    public static readonly DependencyProperty PressCommandProperty =
        DependencyProperty.Register(nameof(PressCommand), typeof(ICommand), typeof(HoldToRunButton), new PropertyMetadata(null));

    public static readonly DependencyProperty PressCommandParameterProperty =
        DependencyProperty.Register(nameof(PressCommandParameter), typeof(object), typeof(HoldToRunButton), new PropertyMetadata(null));

    public static readonly DependencyProperty ReleaseCommandProperty =
        DependencyProperty.Register(nameof(ReleaseCommand), typeof(ICommand), typeof(HoldToRunButton), new PropertyMetadata(null));

    public static readonly DependencyProperty ReleaseCommandParameterProperty =
        DependencyProperty.Register(nameof(ReleaseCommandParameter), typeof(object), typeof(HoldToRunButton), new PropertyMetadata(null));

    private bool _isActive;
    private TouchDevice? _activeTouchDevice;

    public HoldToRunButton()
    {
        Unloaded += OnUnloaded;
    }

    public ICommand? PressCommand
    {
        get => (ICommand?)GetValue(PressCommandProperty);
        set => SetValue(PressCommandProperty, value);
    }

    public object? PressCommandParameter
    {
        get => GetValue(PressCommandParameterProperty);
        set => SetValue(PressCommandParameterProperty, value);
    }

    public ICommand? ReleaseCommand
    {
        get => (ICommand?)GetValue(ReleaseCommandProperty);
        set => SetValue(ReleaseCommandProperty, value);
    }

    public object? ReleaseCommandParameter
    {
        get => GetValue(ReleaseCommandParameterProperty);
        set => SetValue(ReleaseCommandParameterProperty, value);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (!IsEnabled)
        {
            return;
        }

        Focus();
        CaptureMouse();
        Activate();
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        Deactivate();

        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_isActive)
        {
            return;
        }

        Deactivate();

        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        Deactivate();
    }

    protected override void OnPreviewTouchDown(TouchEventArgs e)
    {
        base.OnPreviewTouchDown(e);

        if (!IsEnabled)
        {
            return;
        }

        if (_isActive)
        {
            return;
        }

        Focus();
        CaptureTouch(e.TouchDevice);
        _activeTouchDevice = e.TouchDevice;
        Activate();
    }

    protected override void OnPreviewTouchMove(TouchEventArgs e)
    {
        base.OnPreviewTouchMove(e);

        if (!_isActive || _activeTouchDevice is null || !ReferenceEquals(e.TouchDevice, _activeTouchDevice))
        {
            return;
        }

        var position = e.GetTouchPoint(this).Position;
        if (position.X >= 0
            && position.Y >= 0
            && position.X <= ActualWidth
            && position.Y <= ActualHeight)
        {
            return;
        }

        Deactivate();
        ReleaseTouchCapture(_activeTouchDevice);
        _activeTouchDevice = null;
    }

    protected override void OnPreviewTouchUp(TouchEventArgs e)
    {
        base.OnPreviewTouchUp(e);
        Deactivate();
        ReleaseTouchCapture(e.TouchDevice);
        if (ReferenceEquals(_activeTouchDevice, e.TouchDevice))
        {
            _activeTouchDevice = null;
        }
    }

    protected override void OnLostTouchCapture(TouchEventArgs e)
    {
        base.OnLostTouchCapture(e);
        Deactivate();
        if (ReferenceEquals(_activeTouchDevice, e.TouchDevice))
        {
            _activeTouchDevice = null;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Deactivate();

        if (_activeTouchDevice is not null)
        {
            ReleaseTouchCapture(_activeTouchDevice);
            _activeTouchDevice = null;
        }

        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }
    }

    private void Activate()
    {
        if (_isActive)
        {
            return;
        }

        _isActive = true;
        ExecuteCommand(PressCommand, PressCommandParameter);
    }

    private void Deactivate()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;
        ExecuteCommand(ReleaseCommand, ReleaseCommandParameter ?? PressCommandParameter);
    }

    private static void ExecuteCommand(ICommand? command, object? parameter)
    {
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }
}
