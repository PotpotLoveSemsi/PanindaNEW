using Paninda.Models;

namespace Paninda.Views;

public partial class AddProductPage : ContentPage
{
    private readonly User _currentUser;

    public AddProductPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Product name required", "OK");
            return;
        }

        var product = new Product
        {
            Name = NameEntry.Text.Trim(),
            Category = CategoryEntry.Text?.Trim() ?? "General",
            Stock = int.TryParse(StockEntry.Text, out var stock) ? stock : 0,
            MinStockLevel = int.TryParse(MinStockEntry.Text, out var min) ? min : 5,
            SoldToday = 0,
            Price = 0,
            UserId = _currentUser.Id
        };

        bool success = await App.Products.SaveProductAsync(product);

        if (success)
        {
            await DisplayAlert("Success", "Product added", "OK");

            // 🔥 JUST GO BACK (Dashboard will auto refresh)
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to save product", "OK");
        }
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new UserProfilePage(_currentUser));
}