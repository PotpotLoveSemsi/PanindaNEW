using Paninda.Models;

namespace Paninda.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameEntry.Text) ||
            string.IsNullOrWhiteSpace(emailEntry.Text) ||
            string.IsNullOrWhiteSpace(passwordEntry.Text))
        {
            await DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        var user = new User
        {
            FullName = nameEntry.Text.Trim(),
            Email = emailEntry.Text.Trim().ToLower(),
            Password = passwordEntry.Text,
            DateOfBirth = dobPicker.Date ?? DateTime.Today,
            StoreName = "",
            Phone = "",
            Location = "",
            ProfilePicturePath = "",
            IsPremium = false
        };

        bool success = await App.Supabase.RegisterUserAsync(user);

        if (success)
        {
            await DisplayAlert("Success", "Account Created!", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to create account. Check Supabase table/RLS.", "OK");
        }
    }
}