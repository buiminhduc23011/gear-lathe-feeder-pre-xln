using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Desktop.App.Views;

public partial class AlarmBar : UserControl
{
    public AlarmBar()
    {
        InitializeComponent();
        DataContext = ((App)Application.Current).HeaderViewModel;

        Loaded += (_, _) => RestartAnimation();
        IsVisibleChanged += (_, _) => RestartAnimation();
    }

    private void Viewport_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RestartAnimation();
    }

    private void MarqueeText_OnTargetUpdated(object sender, DataTransferEventArgs e)
    {
        RestartAnimation();
    }

    private void RestartAnimation()
    {
        if (!IsLoaded || Viewport.ActualWidth <= 0)
        {
            return;
        }

        MarqueeText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var textWidth = MarqueeText.DesiredSize.Width;

        if (textWidth <= 0)
        {
            return;
        }

        if (MarqueeText.RenderTransform is not TranslateTransform transform)
        {
            transform = new TranslateTransform();
            MarqueeText.RenderTransform = transform;
        }

        var from = Viewport.ActualWidth;
        var to = -textWidth - 48;
        var distance = from - to;
        var durationSeconds = Math.Max(8d, distance / 110d);

        transform.BeginAnimation(
            TranslateTransform.XProperty,
            new DoubleAnimation(from, to, TimeSpan.FromSeconds(durationSeconds))
            {
                RepeatBehavior = RepeatBehavior.Forever,
            });
    }
}
