using Paninda.Models;

namespace Paninda.Views;

public partial class AddProductPage : ContentPage
{
    private readonly User _currentUser;

    public AddProductPage(User user)
    {
        InitializeComponent();
        _currentUser = user;

        PremiumPriceSection.IsVisible = _currentUser.IsPremium;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Product name required", "OK");
            return;
        }

        decimal price = 0;
        decimal cost = 0;

        if (_currentUser.IsPremium)
        {
            price = decimal.TryParse(PriceEntry.Text, out var parsedPrice) ? parsedPrice : 0;
            cost = decimal.TryParse(CostPriceEntry.Text, out var parsedCost) ? parsedCost : 0;
        }

        var product = new Product
        {
            Name = NameEntry.Text.Trim(),
            Category = CategoryEntry.Text?.Trim() ?? "General",
            Stock = int.TryParse(StockEntry.Text, out var stock) ? stock : 0,
            MinStockLevel = int.TryParse(MinStockEntry.Text, out var min) ? min : 5,
            Price = price,
            CostPrice = cost,
            SoldToday = 0,
            UserId = _currentUser.Id,
            LastSoldDate = DateTime.Today
        };

        bool success = await App.Database.SaveProductAsync(product);

        if (success)
        {
            await DisplayAlert("Success", "Product added", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Error", "Failed to save product", "OK");
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private async void OnLogoClicked(object sender, EventArgs e)
        => await Navigation.PopToRootAsync();

    private async void OnProfileClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new UserProfilePage(_currentUser));
}