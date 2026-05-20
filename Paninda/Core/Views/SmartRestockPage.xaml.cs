using Paninda.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Paninda.Views;

public partial class SmartRestockPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<RestockSuggestion> _suggestions = new();

    public SmartRestockPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        SetPremiumVisibility();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSuggestions();
    }

    private void SetPremiumVisibility()
    {
        PremiumLockedContainer.IsVisible = !_currentUser.IsPremium;
        PremiumUnlockedContainer.IsVisible = _currentUser.IsPremium;
    }

    private async Task LoadSuggestions()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        _suggestions.Clear();

        foreach (var p in products.Where(p => p.Stock <= p.MinStockLevel))
        {
            int suggested = (p.MinStockLevel * 2) - p.Stock;
            if (suggested < 5) suggested = 5;

            _suggestions.Add(new RestockSuggestion
            {
                Name = p.Name,
                Stock = p.Stock,
                SuggestedOrder = suggested,
                Quantity = suggested,
                Price = p.Price,
                IsSelected = true
            });
        }

        RestockSuggestionsList.ItemsSource = _suggestions;
    }

    private void OnDecreaseQuantityClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RestockSuggestion item)
        {
            if (item.Quantity > 1)
                item.Quantity--;
        }
    }

    private void OnIncreaseQuantityClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RestockSuggestion item)
        {
            item.Quantity++;
        }
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        var selectedItems = _suggestions.Where(s => s.IsSelected).ToList();

        if (selectedItems.Count == 0)
        {
            await DisplayAlert("No items selected", "Please select at least one product to order.", "OK");
            return;
        }

        var orderItems = selectedItems.Select(s => new OrderItem
        {
            Name = s.Name,
            Price = s.Price,
            Quantity = s.Quantity,
            Subtotal = s.Quantity * s.Price
        }).ToList();

        await Navigation.PushAsync(new CreateSupplierOrderPage(_currentUser, orderItems));
    }

    private void OnSelectAllClicked(object sender, EventArgs e)
    {
        foreach (var item in _suggestions)
            item.IsSelected = true;
    }

    private void OnClearAllClicked(object sender, EventArgs e)
    {
        foreach (var item in _suggestions)
            item.IsSelected = false;
    }

    private async void OnUnlockPremiumClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new PremiumPage(_currentUser));

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new UserProfilePage(_currentUser));
}

public class RestockSuggestion : INotifyPropertyChanged
{
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int SuggestedOrder { get; set; }
    public decimal Price { get; set; }

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}