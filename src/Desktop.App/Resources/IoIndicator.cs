using System.Windows;
using System.Windows.Controls;

namespace Desktop.App.Resources;

public sealed class IoIndicator : Control
{
    private static readonly DependencyPropertyKey DisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(DisplayText),
            typeof(string),
            typeof(IoIndicator),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty AddressProperty =
        DependencyProperty.Register(
            nameof(Address),
            typeof(string),
            typeof(IoIndicator),
            new PropertyMetadata(string.Empty, OnDisplaySegmentChanged));

    public static readonly DependencyProperty NameIOProperty =
        DependencyProperty.Register(
            nameof(NameIO),
            typeof(string),
            typeof(IoIndicator),
            new PropertyMetadata(string.Empty, OnDisplaySegmentChanged));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(bool), typeof(IoIndicator), new PropertyMetadata(false));

    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    static IoIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IoIndicator), new FrameworkPropertyMetadata(typeof(IoIndicator)));
    }

    public IoIndicator()
    {
        UpdateDisplayText();
    }

    public string Address
    {
        get => (string)GetValue(AddressProperty);
        set => SetValue(AddressProperty, value);
    }

    public string NameIO
    {
        get => (string)GetValue(NameIOProperty);
        set => SetValue(NameIOProperty, value);
    }

    public bool State
    {
        get => (bool)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string DisplayText => (string)GetValue(DisplayTextProperty);

    private static void OnDisplaySegmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IoIndicator indicator)
        {
            indicator.UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        SetValue(DisplayTextPropertyKey, $"{Address} : {NameIO}");
    }
}
