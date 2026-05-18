using Paninda.Models;

namespace Paninda.Views;

public partial class LoginPage : ContentPage
{
    private bool _isPasswordVisible = false;

    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnShowPasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        passwordEntry.IsPassword = !_isPasswordVisible;
        ShowPasswordButton.Text = _isPasswordVisible ? "🙈" : "👁";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim().ToLower() ?? "";
        string password = passwordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter email and password", "OK");
            return;
        }

        var user = await App.Supabase.LoginUserAsync(email, password);

        if (user != null)
        {
            await DisplayAlert("Success", $"Welcome {user.FullName}!", "OK");
            Application.Current.MainPage = new NavigationPage(new DashboardPage(user));
        }
        else
        {
            await DisplayAlert("Error", "Invalid email or password", "OK");
        }
    }
}