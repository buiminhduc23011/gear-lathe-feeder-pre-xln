using System.Windows.Controls;

namespace Desktop.App.Views;

public partial class Footer : UserControl
{
    public Footer()
    {
        InitializeComponent();
        DataContext = ((App)System.Windows.Application.Current).FooterViewModel;
    }
}
