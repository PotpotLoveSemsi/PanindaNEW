using Paninda.Models;

namespace Paninda.Views;

public partial class PremiumPage : ContentPage
{
    private User _currentUser;

    public PremiumPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSubscribeClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConfirmSubscriptionPage(_currentUser));
    }
}