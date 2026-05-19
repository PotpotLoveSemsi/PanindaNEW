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
        SelectPayment("GCash", "gcash_qr.png");
    }

    private void OnMayaClicked(object sender, EventArgs e)
    {
        SelectPayment("Maya", "maya_qr.png");
    }

    private void SelectPayment(string method, string imageFile)
    {
        _selectedMethod = method;

        QrImage.Source = imageFile;
        QrImage.IsVisible = true;
        QrTitleLabel.Text = $"Scan {method} QR to pay ₱200";

        GCashBtn.BackgroundColor = method == "GCash"
            ? Color.FromArgb("#F4B400")
            : Color.FromArgb("#A855F7");

        GCashBtn.TextColor = method == "GCash"
            ? Colors.Black
            : Colors.White;

        MayaBtn.BackgroundColor = method == "Maya"
            ? Color.FromArgb("#F4B400")
            : Color.FromArgb("#A855F7");

        MayaBtn.TextColor = method == "Maya"
            ? Colors.Black
            : Colors.White;
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedMethod))
        {
            await DisplayAlert("Error", "Please select GCash or Maya first.", "OK");
            return;
        }

        bool confirm = await DisplayAlert(
            "Confirm Payment",
            $"Did you already pay using {_selectedMethod}?",
            "Yes",
            "No"
        );

        if (!confirm) return;

        _currentUser.IsPremium = true;

        bool updated = await App.Supabase.UpdateUserAsync(_currentUser);

        if (!updated)
        {
            await DisplayAlert("Error", "Premium update failed.", "OK");
            return;
        }

        await DisplayAlert("Success", "You are now Premium!", "OK");

        Application.Current.MainPage =
            new NavigationPage(new DashboardPage(_currentUser));
    }
}