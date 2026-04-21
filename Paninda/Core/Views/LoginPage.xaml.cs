using Paninda.Models;

namespace Paninda.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string enteredEmail = emailEntry.Text?.Trim().ToLower() ?? "";
        string enteredPassword = passwordEntry.Text ?? "";

        if (string.IsNullOrWhiteSpace(enteredEmail) || string.IsNullOrWhiteSpace(enteredPassword))
        {
            await DisplayAlert("Error", "Please enter email and password", "OK");
            return;
        }

        var user = await App.Database.GetUserByEmailAndPasswordAsync(enteredEmail, enteredPassword);

        if (user != null)
        {
            await DisplayAlert("Success", $"Welcome {user.FullName}!", "OK");
            // ✅ Replace the entire navigation stack so Dashboard becomes root
            Application.Current.MainPage = new NavigationPage(new DashboardPage(user));
        }
        else
        {
            await DisplayAlert("Error", "Invalid email or password", "OK");
        }
    }
}