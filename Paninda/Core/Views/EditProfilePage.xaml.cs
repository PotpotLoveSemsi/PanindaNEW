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

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _currentUser.FullName = OwnerNameEntry.Text.Trim();
        _currentUser.Email = EmailEntry.Text.Trim();
        _currentUser.StoreName = StoreNameEntry.Text.Trim();
        _currentUser.Phone = PhoneEntry.Text.Trim();
        _currentUser.Location = AddressEntry.Text.Trim();
        if (!string.IsNullOrWhiteSpace(PasswordEntry.Text))
            _currentUser.Password = PasswordEntry.Text;

        await App.Database.UpdateUserAsync(_currentUser);
        await DisplayAlert("Success", "Profile updated!", "OK");
        await Navigation.PopAsync();
    }
}