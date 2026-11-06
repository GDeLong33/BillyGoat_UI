using System.Windows;
using System.Windows.Controls;
using BillyGoat_UI.ViewModels;

namespace BillyGoat_UI.Views;

/// <summary>
/// Login screen. A little code-behind is needed here because WPF's PasswordBox
/// cannot be data-bound (on purpose, so the password is not kept in a plain property).
/// </summary>
public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    // Put the cursor in the User ID box when the screen appears.
    private void LoginView_Loaded(object sender, RoutedEventArgs e)
    {
        UserIdBox.Focus();
    }

    // Sign in button (or Enter): hand the password to the ViewModel to check.
    private void SignIn_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.SignIn(PasswordBox.Password);
            if (vm.ErrorMessage.Length > 0)
            {
                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }
    }
}
