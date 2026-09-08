using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Desktop.App.Controls;

namespace Desktop.App.Services;

/// <summary>
/// Manages a single, reusable floating Popup that hosts the virtual keyboard.
/// The popup is draggable by the user and persists its position across focus changes,
/// so the user only needs to drag it once per session.
/// </summary>
public static class VirtualKeyboardManager
{
    // ─── Singleton infrastructure ────────────────────────────────────────────
    private static Popup? _popup;
    private static VirtualKeyboardControl? _keyboard;

    /// <summary>Current target TextBox (or null if targeting a PasswordBox).</summary>
    private static TextBox? _targetTextBox;

    /// <summary>Current target PasswordBox (or null).</summary>
    private static PasswordBox? _targetPasswordBox;

    // Flag: track whether position has been set at least once this session
    private static bool _positionInitialized;
    private static double _savedLeft = 100;
    private static double _savedTop = 100;

    /// <summary>
    /// When the user explicitly closes the keyboard via the ✕ button,
    /// we store the element that was active so we can suppress re-showing
    /// when that same element regains focus (e.g. after the Popup closes).
    /// Cleared automatically when a DIFFERENT element receives focus.
    /// </summary>
    private static UIElement? _suppressForElement;

    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Show the keyboard for a <see cref="TextBox"/> target.
    /// </summary>
    public static void ShowFor(TextBox target)
    {
        // Same element the user just dismissed → stay hidden
        if (target == _suppressForElement) return;

        // Different element → clear suppression and show
        _suppressForElement = null;
        _targetTextBox = target;
        _targetPasswordBox = null;
        EnsurePopup();
        ShowPopup();
    }

    /// <summary>
    /// Show the keyboard for a <see cref="PasswordBox"/> target.
    /// </summary>
    public static void ShowFor(PasswordBox target)
    {
        // Same element the user just dismissed → stay hidden
        if (target == _suppressForElement) return;

        // Different element → clear suppression and show
        _suppressForElement = null;
        _targetPasswordBox = target;
        _targetTextBox = null;
        EnsurePopup();
        ShowPopup();
    }

    /// <summary>
    /// Persist the popup position after a user drag (called from VirtualKeyboardControl).
    /// </summary>
    public static void SavePosition(double left, double top)
    {
        _savedLeft = left;
        _savedTop  = top;
    }

    /// <summary>Hide the keyboard (internal — e.g. on LostFocus or Confirm key).</summary>
    public static void Hide()
    {
        if (_popup is { IsOpen: true })
        {
            _popup.IsOpen = false;
        }
    }

    /// <summary>
    /// Hide the keyboard because the user explicitly pressed the ✕ close button.
    /// Suppresses re-showing for the currently active input element so the keyboard
    /// does not immediately pop back up when focus returns to that element.
    /// </summary>
    public static void HideByUser()
    {
        // Remember which element triggered the close
        _suppressForElement = (UIElement?)_targetTextBox ?? _targetPasswordBox;
        Hide();
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private static void EnsurePopup()
    {
        if (_popup is not null) return;

        _keyboard = new VirtualKeyboardControl();
        _keyboard.KeyPressed += OnKeyPressed;
        _keyboard.BackspacePressed += OnBackspacePressed;
        _keyboard.ConfirmPressed += OnConfirmPressed;

        _popup = new Popup
        {
            Child = _keyboard,
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Fade,
            Placement = PlacementMode.AbsolutePoint,
            StaysOpen = true,   // We manage close ourselves on focus-lost
            IsOpen = false,
        };
    }

    private static void ShowPopup()
    {
        if (!_positionInitialized)
        {
            // Derive initial position from the window that currently hosts the app.
            // This ensures the keyboard appears on the correct monitor in a
            // multi-display configuration (including secondary monitors with
            // negative X-offsets).
            var mainWindow = System.Windows.Application.Current?.MainWindow;
            if (mainWindow != null && mainWindow.IsLoaded)
            {
                // Bottom-right area of the window, in screen coordinates
                var origin = mainWindow.PointToScreen(
                    new System.Windows.Point(mainWindow.ActualWidth, mainWindow.ActualHeight));
                _savedLeft = origin.X - 560;
                _savedTop  = origin.Y - 420;
            }
            else
            {
                // Fallback: primary work area
                var area = SystemParameters.WorkArea;
                _savedLeft = area.Width  - 560;
                _savedTop  = area.Height - 420;
            }

            _positionInitialized = true;
        }

        _popup!.HorizontalOffset = _savedLeft;
        _popup!.VerticalOffset   = _savedTop;
        _popup!.IsOpen = true;
    }

    // ─── Key handlers ────────────────────────────────────────────────────────

    private static void OnKeyPressed(string value)
    {
        if (_targetTextBox is not null)
        {
            // Insert at caret position
            var box = _targetTextBox;
            var idx = box.CaretIndex;
            var text = box.Text ?? string.Empty;
            box.Text = text.Insert(idx, value);
            box.CaretIndex = idx + value.Length;
        }
        else if (_targetPasswordBox is not null)
        {
            // PasswordBox has no caret API — append to end
            _targetPasswordBox.Password += value;
        }
    }

    private static void OnBackspacePressed()
    {
        if (_targetTextBox is not null)
        {
            var box = _targetTextBox;
            if (box.SelectionLength > 0)
            {
                var start = box.SelectionStart;
                box.Text = box.Text.Remove(start, box.SelectionLength);
                box.CaretIndex = start;
            }
            else if (box.CaretIndex > 0)
            {
                var idx = box.CaretIndex - 1;
                box.Text = box.Text.Remove(idx, 1);
                box.CaretIndex = idx;
            }
        }
        else if (_targetPasswordBox is not null)
        {
            var pwd = _targetPasswordBox.Password;
            if (pwd.Length > 0)
            {
                _targetPasswordBox.Password = pwd[..^1];
            }
        }
    }

    private static void OnConfirmPressed()
    {
        var target = (UIElement?)_targetTextBox ?? _targetPasswordBox;
        if (target is not null)
        {
            var scope = FocusManager.GetFocusScope(target);
            if (scope is not null)
            {
                FocusManager.SetFocusedElement(scope, null);
            }
            Keyboard.ClearFocus();
        }

        HideByUser();
    }
}
