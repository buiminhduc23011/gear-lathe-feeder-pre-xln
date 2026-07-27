using System.Windows.Controls;

namespace Desktop.App.Views;

public partial class MainLayout : UserControl
{
    public MainLayout()
    {
        InitializeComponent();
        DataContext = new ViewModels.MainLayoutViewModel();
    }
}
