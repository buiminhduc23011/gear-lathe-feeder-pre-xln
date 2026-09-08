using System.Windows;
using System.Windows.Controls;
using Desktop.App.ViewModels.Dialogs;

namespace Desktop.App.Views.Dialogs;

public partial class LoginDialog : UserControl
{
    public LoginDialog()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginDialogViewModel vm)
        {
            vm.SetPassword(PasswordBox.Password);
        }
    }

    private void PasswordPlainBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is LoginDialogViewModel vm)
        {
            vm.SetPassword(PasswordPlainBox.Text);
        }
    }

    private void PasswordToggle_Checked(object sender, RoutedEventArgs e)
    {
        PasswordPlainBox.Text = PasswordBox.Password;
        PasswordBox.Visibility = Visibility.Collapsed;
        PasswordPlainBox.Visibility = Visibility.Visible;
        PasswordPlainBox.CaretIndex = PasswordPlainBox.Text.Length;
        PasswordPlainBox.Focus();
    }

    private void PasswordToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        PasswordBox.Password = PasswordPlainBox.Text;
        PasswordPlainBox.Visibility = Visibility.Collapsed;
        PasswordBox.Visibility = Visibility.Visible;
        PasswordBox.Focus();
    }
}
