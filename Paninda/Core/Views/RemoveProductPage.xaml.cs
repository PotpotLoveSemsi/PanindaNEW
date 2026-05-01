using Paninda.Models;

namespace Paninda.Views;

public partial class RemoveProductPage : ContentPage
{
    private User _currentUser;
    private Product? _selectedProduct = null; // ✅ nullable and initialized

    public RemoveProductPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadProducts();
        ProductsListView.SelectionChanged += OnSelectionChanged;
    }

    private async void LoadProducts()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        ProductsListView.ItemsSource = products;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedProduct = e.CurrentSelection.FirstOrDefault() as Product;
    }

    private async void OnRemoveSelectedClicked(object sender, EventArgs e)
    {
        if (_selectedProduct == null)
        {
            await DisplayAlert("Error", "Select a product to remove", "OK");
            return;
        }
        bool confirm = await DisplayAlert("Confirm", $"Remove {_selectedProduct.Name}?", "Yes", "No");
        if (confirm)
        {
            await App.Database.DeleteProductAsync(_selectedProduct);
            await DisplayAlert("Success", "Product removed", "OK");
            await Navigation.PopAsync();
        }
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}