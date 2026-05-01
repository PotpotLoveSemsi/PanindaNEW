using Paninda.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Paninda.Views;

public partial class InventoryPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<ProductViewModel> _products = new(); // ✅ initialized

    public InventoryPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadProducts();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProducts();
    }

    private async void LoadProducts()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        _products.Clear(); // ✅ reuse the collection instead of creating a new one
        foreach (var p in products)
        {
            _products.Add(new ProductViewModel(p, async () =>
            {
                await App.Database.UpdateProductAsync(p);
                LoadProducts();
            }));
        }
        ProductsCollectionView.ItemsSource = _products;
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(searchText))
            ProductsCollectionView.ItemsSource = _products;
        else
            ProductsCollectionView.ItemsSource = _products.Where(p => p.Name.ToLower().Contains(searchText)).ToList();
    }

    private async void OnAddProductClicked(object sender, EventArgs e) => await Navigation.PushAsync(new AddProductPage(_currentUser));
    private async void OnRemoveProductClicked(object sender, EventArgs e) => await Navigation.PushAsync(new RemoveProductPage(_currentUser));
    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();

    // ✅ Add these two methods to handle logo and profile taps
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new UserProfilePage(_currentUser));
}

// ProductViewModel remains unchanged
public class ProductViewModel : INotifyPropertyChanged
{
    private Product _product;
    private Action _onStockChanged;

    public ProductViewModel(Product product, Action onStockChanged)
    {
        _product = product;
        _onStockChanged = onStockChanged;
        IncreaseStockCommand = new Command(async () => await ChangeStock(1));
        DecreaseStockCommand = new Command(async () => await ChangeStock(-1));
    }

    public string Name => _product.Name;
    public string Category => _product.Category;
    public int Stock
    {
        get => _product.Stock;
        set { _product.Stock = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsLowStock)); }
    }
    public bool IsLowStock => _product.Stock <= _product.MinStockLevel;

    public ICommand IncreaseStockCommand { get; }
    public ICommand DecreaseStockCommand { get; }

    private async Task ChangeStock(int delta)
    {
        int newStock = Stock + delta;
        if (newStock < 0) return;
        Stock = newStock;
        _product.Stock = Stock;
        await App.Database.UpdateProductAsync(_product);
        _onStockChanged?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}