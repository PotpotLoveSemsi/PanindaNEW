using Paninda.Models;

namespace Paninda.Views;

public partial class AddProductPage : ContentPage
{
    private User _currentUser;

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
            Category = CategoryEntry.Text.Trim(),
            Stock = int.TryParse(StockEntry.Text, out var stock) ? stock : 0,
            MinStockLevel = int.TryParse(MinStockEntry.Text, out var min) ? min : 5,
            Price = 0,
            UserId = _currentUser.Id   
        };

        await App.Database.SaveProductAsync(product);
        await DisplayAlert("Success", "Product added", "OK");
        await Navigation.PopAsync();
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}