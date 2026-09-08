using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Desktop.App.Controls;
using Desktop.App.Services;

namespace Desktop.App.Behaviors;

/// <summary>
/// Attached behavior that wires any <see cref="TextBox"/> or <see cref="PasswordBox"/>
/// to the virtual on-screen keyboard.
///
/// Usage in XAML:
///   behaviors:VirtualKeyboardBehavior.IsEnabled="True"
///
/// Or use the global hook in App.xaml.cs:
///   VirtualKeyboardBehavior.RegisterGlobalHook()
/// which defaults all TextBox to use VirtualKeyboardBehavior,
/// unless overridden per-control via the attached properties.
/// </summary>
public static class VirtualKeyboardBehavior
{
    // ─── Attached: IsEnabled ─────────────────────────────────────────────────

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(VirtualKeyboardBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject d) => (bool)d.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject d, bool value) => d.SetValue(IsEnabledProperty, value);

    // ─── Global hook registration ─────────────────────────────────────────────

    private static bool _globalHookRegistered;

    /// <summary>
    /// Register class-level GotKeyboardFocus handlers for TextBox and PasswordBox.
    /// Call once from App.xaml.cs OnStartup(). Controls can still override via
    /// <see cref="IsEnabledProperty"/> = false to opt out.
    /// </summary>
    public static void RegisterGlobalHook()
    {
        if (_globalHookRegistered) return;
        _globalHookRegistered = true;

        EventManager.RegisterClassHandler(
            typeof(TextBox),
            UIElement.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnTextBoxGotFocus),
            handledEventsToo: true);

        EventManager.RegisterClassHandler(
            typeof(TextBox),
            UIElement.LostKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnTextBoxLostFocus),
            handledEventsToo: true);

        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            UIElement.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnPasswordBoxGotFocus),
            handledEventsToo: true);

        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            UIElement.LostKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnPasswordBoxLostFocus),
            handledEventsToo: true);
    }

    // ─── Per-control IsEnabled changed ───────────────────────────────────────

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Per-control explicit opt-in: wire events directly on the element
        if (d is TextBox tb)
        {
            tb.GotKeyboardFocus -= TextBox_GotFocus_Local;
            tb.LostKeyboardFocus -= AnyControl_LostFocus;
            if ((bool)e.NewValue)
            {
                tb.GotKeyboardFocus += TextBox_GotFocus_Local;
                tb.LostKeyboardFocus += AnyControl_LostFocus;
            }
        }
        else if (d is PasswordBox pb)
        {
            pb.GotKeyboardFocus -= PasswordBox_GotFocus_Local;
            pb.LostKeyboardFocus -= AnyControl_LostFocus;
            if ((bool)e.NewValue)
            {
                pb.GotKeyboardFocus += PasswordBox_GotFocus_Local;
                pb.LostKeyboardFocus += AnyControl_LostFocus;
            }
        }
    }

    private static void TextBox_GotFocus_Local(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox tb)
            TryShowForTextBox(tb);
    }

    private static void PasswordBox_GotFocus_Local(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is PasswordBox pb)
            TryShowForPasswordBox(pb);
    }

    private static void AnyControl_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!IsKeyboardSwitchingFocus(e))
            VirtualKeyboardManager.Hide();
    }

    // ─── Global hook handlers ─────────────────────────────────────────────────

    private static void OnTextBoxGotFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        // If the control explicitly opted OUT, skip
        if (GetIsEnabled(tb) == false && tb.ReadLocalValue(IsEnabledProperty) != DependencyProperty.UnsetValue)
            return;

        TryShowForTextBox(tb);
    }

    private static void OnTextBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!IsKeyboardSwitchingFocus(e))
            VirtualKeyboardManager.Hide();
    }

    private static void OnPasswordBoxGotFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is not PasswordBox pb) return;
        TryShowForPasswordBox(pb);
    }

    private static void OnPasswordBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!IsKeyboardSwitchingFocus(e))
            VirtualKeyboardManager.Hide();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static void TryShowForTextBox(TextBox tb)
    {
        if (!VirtualKeyboardStateService.Instance.IsEnabled) return;

        VirtualKeyboardManager.ShowFor(tb);
    }

    private static void TryShowForPasswordBox(PasswordBox pb)
    {
        if (!VirtualKeyboardStateService.Instance.IsEnabled) return;

        VirtualKeyboardManager.ShowFor(pb);
    }

    /// <summary>
    /// Returns true when focus is shifting to another keyboard-interactive element
    /// (e.g. clicking inside the keyboard popup itself), so we don't hide prematurely.
    /// </summary>
    private static bool IsKeyboardSwitchingFocus(KeyboardFocusChangedEventArgs e)
    {
        // If new focus owner is inside the virtual keyboard, keep it open
        var newFocus = e.NewFocus as DependencyObject;
        return newFocus is not null
            && IsDescendantOfKeyboardPopup(newFocus);
    }

    private static bool IsDescendantOfKeyboardPopup(DependencyObject element)
    {
        var current = element;
        while (current is not null)
        {
            if (current is Controls.VirtualKeyboardControl)
                return true;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current)
                      ?? LogicalTreeHelper.GetParent(current);
        }

        return false;
    }
}
