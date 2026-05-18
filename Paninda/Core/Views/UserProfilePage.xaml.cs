using Paninda.Models;
using Microsoft.Maui.Media;

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

        var updatedUser = await App.Supabase.GetUserByEmailAsync(_currentUser.Email);
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

        if (!string.IsNullOrWhiteSpace(_currentUser.ProfilePicturePath))
        {
            ProfileImage.Source = ImageSource.FromUri(new Uri(_currentUser.ProfilePicturePath));
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

    private async void OnChangeProfilePicture(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select Profile Picture"
            });

            if (result == null) return;

            using var stream = await result.OpenReadAsync();

            string fileName = $"user_{_currentUser.Id}_{Guid.NewGuid()}.jpg";
            string imageUrl = await App.Supabase.UploadProfileImageAsync(stream, fileName);

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                await DisplayAlert("Error", "Image upload failed.", "OK");
                return;
            }

            _currentUser.ProfilePicturePath = imageUrl;

            await App.Supabase.UpdateUserAsync(_currentUser);

            LoadUserData();

            await DisplayAlert("Success", "Profile picture updated.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not update image: " + ex.Message, "OK");
        }
    }

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Navigation.PopAsync();

    private async void OnEditProfile(object sender, EventArgs e) =>
        await Navigation.PushAsync(new EditProfilePage(_currentUser));

    private async void OnOrderHistory(object sender, EventArgs e) =>
        await Navigation.PushAsync(new OrderHistoryPage(_currentUser));

    private async void OnUpgradeClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new PremiumPage(_currentUser));

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");

        if (confirm)
            Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}