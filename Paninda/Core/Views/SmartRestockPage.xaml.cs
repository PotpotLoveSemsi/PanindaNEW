using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class SmartRestockPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<RestockSuggestion> _suggestions = new();

    public SmartRestockPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadSuggestions();
    }

    private async void LoadSuggestions()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id); // ✅ pass userId
        _suggestions.Clear();
        foreach (var p in products.Where(p => p.Stock <= p.MinStockLevel))
        {
            int suggested = (p.MinStockLevel * 2) - p.Stock;
            if (suggested < 5) suggested = 5;
            _suggestions.Add(new RestockSuggestion
            {
                Name = p.Name,
                Stock = p.Stock,
                SuggestedOrder = suggested
            });
        }
        RestockSuggestionsList.ItemsSource = _suggestions;
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateSupplierOrderPage(_currentUser));
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}

public class RestockSuggestion
{
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int SuggestedOrder { get; set; }
}