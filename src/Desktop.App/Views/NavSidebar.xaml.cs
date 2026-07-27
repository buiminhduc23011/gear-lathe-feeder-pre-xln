using System.Windows.Controls;

namespace Desktop.App.Views;

public partial class NavSidebar : UserControl
{
    public NavSidebar()
    {
        InitializeComponent();
        DataContext = ((App)System.Windows.Application.Current).HeaderViewModel;
    }
}
