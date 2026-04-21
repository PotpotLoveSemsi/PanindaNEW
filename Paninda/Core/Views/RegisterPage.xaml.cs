using Paninda;
using Paninda.Models;
using System;

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
            Email = emailEntry.Text.Trim().ToLower(), // store email as lowercase to avoid case issues
            Password = passwordEntry.Text,
            DateOfBirth = dobPicker.Date ?? DateTime.Today
        };

        var existing = await App.Database.GetUserByEmailAsync(user.Email);
        if (existing != null)
        {
            await DisplayAlert("Error", "Email already registered", "OK");
            return;
        }

        await App.Database.SaveUserAsync(user);

        // DEBUG: verify save worked
        var saved = await App.Database.GetUserByEmailAsync(user.Email);
        if (saved != null)
            await DisplayAlert("Debug", $"Saved user: {saved.Email}, {saved.Password}", "OK");
        else
            await DisplayAlert("Debug", "Save failed - user not found after insert", "OK");

        await DisplayAlert("Success", "Account Created!", "OK");
        await Navigation.PopAsync();
    }
}