using System.Windows.Controls;

namespace Desktop.App.Views;

public partial class MainContent : UserControl
{
    public MainContent()
    {
        InitializeComponent();
        DataContext = new ViewModels.MainContentViewModel();
    }
}
