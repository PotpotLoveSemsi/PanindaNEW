using Paninda.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Paninda.Views;

public partial class InventoryPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<ProductViewModel> _products = new();

    public InventoryPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        var products = await App.Supabase.GetProductsAsync(_currentUser.Id);

        _products.Clear();

        foreach (var p in products)
        {
            _products.Add(new ProductViewModel(
                p,
                this,
                _currentUser.IsPremium,
                async () => await LoadProducts()
            ));
        }

        ProductsCollectionView.ItemsSource = _products;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower() ?? "";

        ProductsCollectionView.ItemsSource = string.IsNullOrWhiteSpace(searchText)
            ? _products
            : _products.Where(p => p.Name.ToLower().Contains(searchText)).ToList();
    }

    private async void OnAddProductClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AddProductPage(_currentUser));

    private async void OnRemoveProductClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new RemoveProductPage(_currentUser));

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Navigation.PopAsync();

    private async void OnLogoClicked(object sender, EventArgs e) =>
        await Navigation.PopToRootAsync();
}

public class ProductViewModel : INotifyPropertyChanged
{
    private Product _product;
    private Page _page;
    private Func<Task> _onStockChanged;

    public ProductViewModel(Product product, Page page, bool isPremium, Func<Task> onStockChanged)
    {
        _product = product;
        _page = page;
        ShowQuickSale = isPremium;
        _onStockChanged = onStockChanged;

        IncreaseStockCommand = new Command(async () => await ChangeStock(1));
        DecreaseStockCommand = new Command(async () => await ChangeStock(-1));
        QuickSaleCommand = new Command(async () => await QuickSale());
    }

    public string Name => _product.Name;
    public string Category => _product.Category;
    public decimal Price => _product.Price;
    public decimal CostPrice => _product.CostPrice;

    public int Stock
    {
        get => _product.Stock;
        set
        {
            _product.Stock = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLowStock));
        }
    }

    public int SoldToday
    {
        get => _product.SoldToday;
        set
        {
            _product.SoldToday = value;
            OnPropertyChanged();
        }
    }

    public bool IsLowStock => _product.Stock <= _product.MinStockLevel;
    public bool ShowQuickSale { get; }

    public ICommand IncreaseStockCommand { get; }
    public ICommand DecreaseStockCommand { get; }
    public ICommand QuickSaleCommand { get; }

    private async Task ChangeStock(int delta)
    {
        int newStock = Stock + delta;
        if (newStock < 0) return;

        Stock = newStock;

        bool updated = await App.Supabase.UpdateProductAsync(_product);

        if (!updated)
        {
            await _page.DisplayAlert("Error", "Product stock did not update.", "OK");
            return;
        }

        await _onStockChanged();
    }

    private async Task QuickSale()
    {
        if (!ShowQuickSale) return;

        if (_product.Stock <= 0)
        {
            await _page.DisplayAlert("Out of Stock", "No stock left.", "OK");
            return;
        }

        string result = await _page.DisplayPromptAsync(
            "Quick Sale",
            $"Enter quantity for {Name}:",
            "Confirm",
            "Cancel",
            "Quantity",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(result)) return;

        if (!int.TryParse(result, out int quantity) || quantity <= 0)
        {
            await _page.DisplayAlert("Invalid", "Enter valid number.", "OK");
            return;
        }

        if (quantity > _product.Stock)
        {
            await _page.DisplayAlert("Error", $"Only {_product.Stock} left.", "OK");
            return;
        }

        decimal totalPrice = quantity * _product.Price;
        decimal profit = (_product.Price - _product.CostPrice) * quantity;

        _product.Stock -= quantity;
        _product.SoldToday += quantity;
        _product.LastSoldDate = DateTime.Today;

        var sale = new Sale
        {
            ProductId = _product.Id,
            ProductName = _product.Name,
            Quantity = quantity,
            TotalPrice = totalPrice,
            Profit = profit,
            DateSold = DateTime.Now,
            UserId = _product.UserId
        };

        bool saleSaved = await App.Supabase.SaveSaleAsync(sale);

        if (!saleSaved)
        {
            await _page.DisplayAlert("Error", "Sale did not save to Supabase.", "OK");
            return;
        }

        bool productUpdated = await App.Supabase.UpdateProductAsync(_product);

        if (!productUpdated)
        {
            await _page.DisplayAlert("Error", "Product stock did not update.", "OK");
            return;
        }

        OnPropertyChanged(nameof(Stock));
        OnPropertyChanged(nameof(SoldToday));
        OnPropertyChanged(nameof(IsLowStock));

        await _page.DisplayAlert("Success", "Sale saved.", "OK");
        await _onStockChanged();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}