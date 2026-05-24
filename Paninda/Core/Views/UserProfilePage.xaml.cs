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

        var updatedUser = await App.Supabase.GetUserByIdAsync(_currentUser.Id);

        if (updatedUser != null)
            _currentUser = updatedUser;

        LoadUserData();
        SetPremiumVisibility();
    }

    private void LoadUserData()
    {
        OwnerNameLabel.Text = string.IsNullOrWhiteSpace(_currentUser.FullName)
            ? "Not set"
            : _currentUser.FullName;

        StoreNameLabel.Text = string.IsNullOrWhiteSpace(_currentUser.StoreName)
            ? "My Store"
            : _currentUser.StoreName;

        PhoneLabel.Text = string.IsNullOrWhiteSpace(_currentUser.Phone)
            ? "Not set"
            : _currentUser.Phone;

        LocationLabel.Text = string.IsNullOrWhiteSpace(_currentUser.Location)
            ? "Not set"
            : _currentUser.Location;

        LoadProfileImage();
    }

    private void LoadProfileImage()
    {
        if (!string.IsNullOrWhiteSpace(_currentUser.ProfilePicturePath) &&
            Uri.TryCreate(_currentUser.ProfilePicturePath, UriKind.Absolute, out Uri? imageUri))
        {
            ProfileImage.Source = ImageSource.FromUri(imageUri);
            ProfileImage.IsVisible = true;
            ProfileEmoji.IsVisible = false;
        }
        else
        {
            ProfileImage.Source = null;
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
                await DisplayAlert("Error", "Image upload failed. Check Supabase storage policy.", "OK");
                return;
            }

            _currentUser.ProfilePicturePath = imageUrl;

            bool updated = await App.Supabase.UpdateUserAsync(_currentUser);

            if (!updated)
            {
                await DisplayAlert("Error", "Profile picture was not saved.", "OK");
                return;
            }

            LoadUserData();
            await DisplayAlert("Success", "Profile picture updated.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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