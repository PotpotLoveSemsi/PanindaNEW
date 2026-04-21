using Paninda.Models;

namespace Paninda.Views;

public partial class ConfirmSubscriptionPage : ContentPage
{
    private User _currentUser;

    public ConfirmSubscriptionPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnConfirmPayment(object sender, EventArgs e)
    {
        if (PaymentPicker.SelectedIndex == -1)
        {
            await DisplayAlert("Error", "Select payment method", "OK");
            return;
        }

        _currentUser.IsPremium = true;
        await App.Database.UpdateUserAsync(_currentUser);

        await Navigation.PushAsync(new SubscriptionSuccessPage(_currentUser));
    }
}