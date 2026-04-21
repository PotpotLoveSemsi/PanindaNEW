using Paninda.Views;

namespace Paninda.Core.Views;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void OnSignupClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());

    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }
}