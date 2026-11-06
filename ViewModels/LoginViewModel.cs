using BillyGoat_UI.Models;
using BillyGoat_UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BillyGoat_UI.ViewModels;

/// <summary>
/// ViewModel for the login screen. It checks the user ID and password against
/// the fake users in FakeData, and tells MainViewModel who signed in.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    // MainViewModel passes this in. We call it when the sign-in is correct.
    private readonly Action<User> _onSignedIn;

    public LoginViewModel(Action<User> onSignedIn)
    {
        _onSignedIn = onSignedIn;
        UserId = "";
        ErrorMessage = "";
    }

    /// <summary>Text in the User ID box.</summary>
    [ObservableProperty]
    public partial string UserId { get; set; }

    /// <summary>Red message under the form, empty when there is no error.</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; }

    /// <summary>
    /// Called by LoginView when Sign in is pressed. The password is passed in as a
    /// parameter because WPF's PasswordBox does not allow data binding (for security).
    /// </summary>
    public void SignIn(string password)
    {
        string id = UserId.Trim();

        if (id.Length == 0 || password.Length == 0)
        {
            ErrorMessage = "Enter your user ID and password.";
            return;
        }

        // User IDs are not case sensitive; passwords are.
        User? user = FakeData.Users.FirstOrDefault(u =>
            u.UserId.Equals(id, StringComparison.OrdinalIgnoreCase) && u.Password == password);

        if (user == null)
        {
            ErrorMessage = "The user ID or password is not correct. Please try again.";
            return;
        }

        ErrorMessage = "";
        _onSignedIn(user);
    }
}
