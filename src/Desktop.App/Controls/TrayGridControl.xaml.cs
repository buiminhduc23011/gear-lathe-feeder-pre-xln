using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Desktop.App.Models.Tray;

namespace Desktop.App.Controls;

public partial class TrayGridControl : UserControl
{
    public TrayGridControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty GridRowsProperty =
        DependencyProperty.Register(nameof(GridRows), typeof(int), typeof(TrayGridControl),
            new PropertyMetadata(5));

    public int GridRows
    {
        get => (int)GetValue(GridRowsProperty);
        set => SetValue(GridRowsProperty, value);
    }

    public static readonly DependencyProperty GridColumnsProperty =
        DependencyProperty.Register(nameof(GridColumns), typeof(int), typeof(TrayGridControl),
            new PropertyMetadata(9));

    public int GridColumns
    {
        get => (int)GetValue(GridColumnsProperty);
        set => SetValue(GridColumnsProperty, value);
    }

    public static readonly DependencyProperty SlotStatesProperty =
        DependencyProperty.Register(nameof(SlotStates), typeof(ObservableCollection<TraySlotState>), typeof(TrayGridControl),
            new PropertyMetadata(null));

    public ObservableCollection<TraySlotState> SlotStates
    {
        get => (ObservableCollection<TraySlotState>)GetValue(SlotStatesProperty);
        set => SetValue(SlotStatesProperty, value);
    }

    public static readonly DependencyProperty TrayLabelProperty =
        DependencyProperty.Register(nameof(TrayLabel), typeof(string), typeof(TrayGridControl),
            new PropertyMetadata("TRAY"));

    public string TrayLabel
    {
        get => (string)GetValue(TrayLabelProperty);
        set => SetValue(TrayLabelProperty, value);
    }

    #endregion
}
