using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Desktop.App.Services;

namespace Desktop.App.Controls;

/// <summary>
/// On-screen virtual keyboard control.
/// </summary>
public partial class VirtualKeyboardControl : UserControl
{
    // ──────────────────────────────── Events ────────────────────────────────

    /// <summary>Fired when an insertable character key is pressed. Value = the character string.</summary>
    public event Action<string>? KeyPressed;

    /// <summary>Fired when the Backspace key is pressed.</summary>
    public event Action? BackspacePressed;

    /// <summary>Fired when the Confirm / Enter key is pressed.</summary>
    public event Action? ConfirmPressed;

    // ──────────────────────────────── Constructor ───────────────────────────

    public VirtualKeyboardControl()
    {
        InitializeComponent();
        DragHandle.MouseLeftButtonDown += DragHandle_MouseLeftButtonDown;
    }

    // ──────────────────────────────── Win32 P/Invoke ────────────────────────

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint pt);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

    /// <summary>Move window directly without resize or z-order change (no WPF layout pass).</summary>
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    private const uint SWP_NOSIZE     = 0x0001;
    private const uint SWP_NOZORDER   = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct NativePoint { public int X, Y; }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }

    // ──────────────────────────────── Drag-to-move support ──────────────────

    private void DragHandle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (this.Parent is not System.Windows.Controls.Primitives.Popup popup)
            return;

        // Get the HWND of the Popup's own window (created when IsOpen = true).
        var hwndSource = System.Windows.Interop.HwndSource.FromVisual(this)
                      as System.Windows.Interop.HwndSource;
        if (hwndSource == null) return;
        var hwnd = hwndSource.Handle;

        // Snapshot: cursor + actual window top-left in physical pixels.
        GetCursorPos(out var startCursor);
        GetWindowRect(hwnd, out var startRect);

        void OnMouseMove(object s, System.Windows.Input.MouseEventArgs me)
        {
            // Call SetWindowPos directly on the HWND — bypasses WPF layout pipeline
            // entirely, giving zero-latency drag on any DPI setting.
            GetCursorPos(out var cur);
            SetWindowPos(
                hwnd, IntPtr.Zero,
                startRect.Left + (cur.X - startCursor.X),
                startRect.Top  + (cur.Y - startCursor.Y),
                0, 0,
                SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
        }

        void OnMouseUp(object s, System.Windows.Input.MouseButtonEventArgs me)
        {
            DragHandle.ReleaseMouseCapture();
            DragHandle.MouseMove         -= OnMouseMove;
            DragHandle.MouseLeftButtonUp -= OnMouseUp;

            // Sync Popup offset to the final physical position so WPF doesn't
            // snap it back if something triggers a layout recalculation later.
            GetWindowRect(hwnd, out var finalRect);
            popup.HorizontalOffset = finalRect.Left;
            popup.VerticalOffset   = finalRect.Top;
            VirtualKeyboardManager.SavePosition(finalRect.Left, finalRect.Top);
        }

        DragHandle.CaptureMouse();
        DragHandle.MouseMove         += OnMouseMove;
        DragHandle.MouseLeftButtonUp += OnMouseUp;
    }

    // ──────────────────────────────── Numeric handlers ──────────────────────

    private void OnKeyClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string val)
        {
            KeyPressed?.Invoke(val);
        }
    }

    private void OnBackspaceClick(object sender, RoutedEventArgs e)
    {
        BackspacePressed?.Invoke();
    }

    private void OnConfirmClick(object sender, RoutedEventArgs e)
    {
        ConfirmPressed?.Invoke();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        VirtualKeyboardManager.HideByUser();
    }

    // ──────────────────────────────── Alphanumeric handlers ─────────────────

    private bool _shiftActive;

    private void OnAlphaKeyClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string rawVal)
        {
            return;
        }

        var val = (_shiftActive && rawVal.Length == 1 && char.IsLetter(rawVal[0]))
            ? rawVal.ToUpperInvariant()
            : rawVal;

        KeyPressed?.Invoke(val);

        // Auto-release shift after one character (standard behaviour)
        if (_shiftActive && rawVal.Length == 1 && char.IsLetter(rawVal[0]))
        {
            ShiftToggle.IsChecked = false;
        }
    }

    private void ShiftToggle_Changed(object sender, RoutedEventArgs e)
    {
        _shiftActive = ShiftToggle.IsChecked == true;
        UpdateAlphaKeyLabels();
    }

    private void UpdateAlphaKeyLabels()
    {
        // All letter-key buttons are named Key_<Letter>; update content based on shift
        var letterKeys = new (Button btn, string lower)[]
        {
            (Key_Q, "q"), (Key_W, "w"), (Key_E, "e"), (Key_R, "r"), (Key_T, "t"),
            (Key_Y, "y"), (Key_U, "u"), (Key_I, "i"), (Key_O, "o"), (Key_P, "p"),
            (Key_A, "a"), (Key_S, "s"), (Key_D, "d"), (Key_F, "f"), (Key_G, "g"),
            (Key_H, "h"), (Key_J, "j"), (Key_K, "k"), (Key_L, "l"),
            (Key_Z, "z"), (Key_X, "x"), (Key_C, "c"), (Key_V, "v"), (Key_B, "b"),
            (Key_N, "n"), (Key_M, "m"),
        };

        foreach (var (btn, lower) in letterKeys)
        {
            var display = _shiftActive ? lower.ToUpperInvariant() : lower;
            btn.Content = display;
            btn.Tag = lower; // tag always stays lowercase; handler applies shift
        }
    }
}
