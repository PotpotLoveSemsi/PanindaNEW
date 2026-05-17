using Paninda.Models;
using System.Text;

namespace Paninda.Views;

public partial class DashboardPage : ContentPage
{
    private User _currentUser;

    public DashboardPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var updatedUser = await App.Database.GetUserByEmailAsync(_currentUser.Email);
        if (updatedUser != null)
            _currentUser = updatedUser;

        UpdateProfilePicture();
        SetPremiumVisibility();
        await LoadDashboardData();
    }

    private void UpdateProfilePicture()
    {
        if (!string.IsNullOrEmpty(_currentUser.ProfilePicturePath) &&
            File.Exists(_currentUser.ProfilePicturePath))
        {
            DashboardProfileImage.Source = ImageSource.FromFile(_currentUser.ProfilePicturePath);
            DashboardProfileImage.IsVisible = true;
            DashboardProfileEmoji.IsVisible = false;
        }
        else
        {
            DashboardProfileImage.IsVisible = false;
            DashboardProfileEmoji.IsVisible = true;
        }
    }

    private void SetPremiumVisibility()
    {
        PremiumLockedContainer.IsVisible = !_currentUser.IsPremium;
        PremiumUnlockedContainer.IsVisible = _currentUser.IsPremium;
    }

    private async Task LoadDashboardData()
    {
        await LoadLowStockItems();
        await LoadTopSellingItems();
        await LoadIncomingShipments();
        await LoadSuggestedRestock();
    }

    private async Task LoadLowStockItems()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        var lowStock = products.Where(p => p.Stock <= p.MinStockLevel).ToList();

        LowStockLabel.Text = lowStock.Count == 0
            ? "No items running low."
            : string.Join("\n", lowStock.Select(p => $"• {p.Name} – {p.Stock} left"));
    }

    private async Task LoadTopSellingItems()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        var topSelling = products.OrderByDescending(p => p.SoldToday).Take(3).ToList();

        TopSellingLabel.Text = topSelling.Count == 0
            ? "No sales data yet."
            : string.Join("\n", topSelling.Select(p => $"• {p.Name} – {p.SoldToday} sold"));
    }

    private async Task LoadIncomingShipments()
    {
        var orders = await App.Database.GetSupplierOrdersAsync(_currentUser.Id);
        var incoming = orders.Where(o => o.Status == "Pending" && (o.ETA >= DateTime.Now || o.ETA == null)).ToList();

        IncomingLabel.Text = incoming.Count == 0
            ? "No incoming shipments."
            : string.Join("\n", incoming.Select(o => $"{o.SupplierName} - ETA: {(o.ETA?.ToString("MMM dd") ?? "TBD")}"));
    }

    private async Task LoadSuggestedRestock()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        var lowStock = products.Where(p => p.Stock <= p.MinStockLevel).ToList();

        RestockLabel.Text = lowStock.Count == 0
            ? "No restock needed."
            : string.Join("\n", lowStock.Select(p =>
            {
                int suggested = (p.MinStockLevel * 2) - p.Stock;
                if (suggested < 5) suggested = 5;
                return $"• Order {suggested} {p.Name}";
            }));
    }

    private async void OnInventoryClicked(object sender, EventArgs e) => await Navigation.PushAsync(new InventoryPage(_currentUser));
    private async void OnRestockClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SmartRestockPage(_currentUser));
    private async void OnSuppliersClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SupplierPage(_currentUser));
    private async void OnUpgradeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new PremiumPage(_currentUser));
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new UserProfilePage(_currentUser));
    private async void OnNotificationClicked(object sender, EventArgs e) => await Navigation.PushAsync(new NotificationPage(_currentUser));
}