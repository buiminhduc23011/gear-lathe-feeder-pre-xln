using System.Windows;
using System.Windows.Controls;

namespace Desktop.App.Views.Shared;

public partial class PageScaffold : UserControl
{
    public static readonly DependencyProperty PageTitleProperty =
        DependencyProperty.Register(nameof(PageTitle), typeof(string), typeof(PageScaffold), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PageSubtitleProperty =
        DependencyProperty.Register(nameof(PageSubtitle), typeof(string), typeof(PageScaffold), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StatusContentProperty =
        DependencyProperty.Register(nameof(StatusContent), typeof(object), typeof(PageScaffold), new PropertyMetadata(null));

    public static readonly DependencyProperty BodyContentProperty =
        DependencyProperty.Register(nameof(BodyContent), typeof(object), typeof(PageScaffold), new PropertyMetadata(null));

    public static readonly DependencyProperty BodyVerticalScrollBarVisibilityProperty =
        DependencyProperty.Register(
            nameof(BodyVerticalScrollBarVisibility),
            typeof(ScrollBarVisibility),
            typeof(PageScaffold),
            new PropertyMetadata(ScrollBarVisibility.Auto));

    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(nameof(ActionContent), typeof(object), typeof(PageScaffold), new PropertyMetadata(null));

    public static readonly DependencyProperty HeaderActionContentProperty =
        DependencyProperty.Register(nameof(HeaderActionContent), typeof(object), typeof(PageScaffold), new PropertyMetadata(null));

    public PageScaffold()
    {
        InitializeComponent();
    }

    public string PageTitle
    {
        get => (string)GetValue(PageTitleProperty);
        set => SetValue(PageTitleProperty, value);
    }

    public string PageSubtitle
    {
        get => (string)GetValue(PageSubtitleProperty);
        set => SetValue(PageSubtitleProperty, value);
    }

    public object? StatusContent
    {
        get => GetValue(StatusContentProperty);
        set => SetValue(StatusContentProperty, value);
    }

    public object? BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    public ScrollBarVisibility BodyVerticalScrollBarVisibility
    {
        get => (ScrollBarVisibility)GetValue(BodyVerticalScrollBarVisibilityProperty);
        set => SetValue(BodyVerticalScrollBarVisibilityProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public object? HeaderActionContent
    {
        get => GetValue(HeaderActionContentProperty);
        set => SetValue(HeaderActionContentProperty, value);
    }
}
