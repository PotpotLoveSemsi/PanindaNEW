using Paninda.Models;

namespace Paninda.Views;

public partial class PremiumPage : ContentPage
{
    private User _currentUser;
    private string _selectedMethod = "";

    public PremiumPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private void OnGCashClicked(object sender, EventArgs e)
    {
        _selectedMethod = "GCash";
        QrImage.Source = "gcash.png";
        QrImage.IsVisible = true;
    }

    private void OnMayaClicked(object sender, EventArgs e)
    {
        _selectedMethod = "Maya";
        QrImage.Source = "maya.png";
        QrImage.IsVisible = true;
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedMethod))
        {
            await DisplayAlert("Error", "Select payment method first", "OK");
            return;
        }

        bool confirm = await DisplayAlert(
            "Confirm Payment",
            $"Confirm {_selectedMethod} payment?",
            "Yes",
            "No"
        );

        if (!confirm) return;

        _currentUser.IsPremium = true;
        await App.Database.UpdateUserAsync(_currentUser);

        await DisplayAlert("Success", "You are now Premium!", "OK");

        await Navigation.PopAsync();
    }
}