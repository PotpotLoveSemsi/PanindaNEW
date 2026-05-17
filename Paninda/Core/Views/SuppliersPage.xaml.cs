using Paninda.Models;

namespace Paninda.Views;

public partial class SupplierPage : ContentPage
{
    private User _currentUser;

    public SupplierPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnViewOrdersClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OrdersPage(_currentUser));
    }
}