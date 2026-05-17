using Paninda.Models;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Paninda.Views;

public partial class UserProfilePage : ContentPage
{
    private User _currentUser;

    public UserProfilePage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadUserData();
        SetPremiumVisibility();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var updatedUser = await App.Database.GetUserByEmailAsync(_currentUser.Email);
        if (updatedUser != null)
            _currentUser = updatedUser;

        LoadUserData();
        SetPremiumVisibility();
    }

    private void LoadUserData()
    {
        OwnerNameLabel.Text = _currentUser.FullName;
        StoreNameLabel.Text = _currentUser.StoreName ?? "My Store";
        PhoneLabel.Text = _currentUser.Phone ?? "Not set";
        LocationLabel.Text = _currentUser.Location ?? "Not set";

        if (!string.IsNullOrEmpty(_currentUser.ProfilePicturePath) &&
            File.Exists(_currentUser.ProfilePicturePath))
        {
            ProfileImage.Source = ImageSource.FromFile(_currentUser.ProfilePicturePath);
            ProfileImage.IsVisible = true;
            ProfileEmoji.IsVisible = false;
        }
        else
        {
            ProfileImage.IsVisible = false;
            ProfileEmoji.IsVisible = true;
        }
    }

    private void SetPremiumVisibility()
    {
        PremiumUpgradeContainer.IsVisible = !_currentUser.IsPremium;
        PremiumActiveLabel.IsVisible = _currentUser.IsPremium;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnEditProfile(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditProfilePage(_currentUser));
    }

    private async void OnOrderHistory(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OrderHistoryPage(_currentUser));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
        if (confirm)
            Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    private async void OnChangeProfilePicture(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Profile Picture"
            });

            if (result == null)
                return;

            var localFilePath = Path.Combine(
                FileSystem.AppDataDirectory,
                $"user_{_currentUser.Id}_profile.jpg"
            );

            using var stream = await result.OpenReadAsync();
            using var fileStream = File.Create(localFilePath);
            await stream.CopyToAsync(fileStream);

            _currentUser.ProfilePicturePath = localFilePath;
            await App.Database.UpdateUserAsync(_currentUser);

            LoadUserData();

            await DisplayAlert("Success", "Profile picture updated.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not select image: " + ex.Message, "OK");
        }
    }

    private async void OnUpgradeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PremiumPage(_currentUser));
    }
}