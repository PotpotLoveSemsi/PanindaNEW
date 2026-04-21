using Paninda.Models;

namespace Paninda.Views;

public partial class SubscriptionSuccessPage : ContentPage
{
    private User _currentUser;

    public SubscriptionSuccessPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private async void OnGoDashboard(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}