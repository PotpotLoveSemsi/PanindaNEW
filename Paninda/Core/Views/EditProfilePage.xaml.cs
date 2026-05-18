using Paninda.Models;

namespace Paninda.Views;

public partial class EditProfilePage : ContentPage
{
    private User _currentUser;

    public EditProfilePage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadCurrentData();
    }

    private void LoadCurrentData()
    {
        OwnerNameEntry.Text = _currentUser.FullName;
        EmailEntry.Text = _currentUser.Email;
        StoreNameEntry.Text = _currentUser.StoreName ?? "";
        PhoneEntry.Text = _currentUser.Phone ?? "";
        AddressEntry.Text = _currentUser.Location ?? "";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OwnerNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text))
        {
            await DisplayAlert("Error", "Name and email are required.", "OK");
            return;
        }

        _currentUser.FullName = OwnerNameEntry.Text.Trim();
        _currentUser.Email = EmailEntry.Text.Trim().ToLower();
        _currentUser.StoreName = StoreNameEntry.Text?.Trim() ?? "";
        _currentUser.Phone = PhoneEntry.Text?.Trim() ?? "";
        _currentUser.Location = AddressEntry.Text?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(PasswordEntry.Text))
            _currentUser.Password = PasswordEntry.Text;

        bool success = await App.Supabase.UpdateUserAsync(_currentUser);

        if (!success)
        {
            await DisplayAlert("Error", "Failed to update profile.", "OK");
            return;
        }

        await DisplayAlert("Success", "Profile updated!", "OK");
        await Navigation.PopAsync();
    }
}