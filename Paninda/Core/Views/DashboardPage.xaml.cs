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
        await LoadDashboardData();
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
        var products = await App.Database.GetProductsAsync(_currentUser.Id); // ✅ filter by user
        var lowStock = products.Where(p => p.Stock <= p.MinStockLevel).ToList();

        if (lowStock.Count == 0)
        {
            LowStockLabel.Text = "No items running low.";
            return;
        }

        var sb = new StringBuilder();
        foreach (var p in lowStock)
            sb.AppendLine($"• {p.Name} – {p.Stock} left");
        LowStockLabel.Text = sb.ToString();
    }

    private async Task LoadTopSellingItems()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id); // ✅ filter by user
        var topSelling = products.OrderByDescending(p => p.SoldToday).Take(3).ToList();

        if (topSelling.Count == 0)
        {
            TopSellingLabel.Text = "No sales data yet.";
            return;
        }

        var sb = new StringBuilder();
        foreach (var p in topSelling)
            sb.AppendLine($"• {p.Name} – {p.SoldToday} sold");
        TopSellingLabel.Text = sb.ToString();
    }

    private async Task LoadIncomingShipments()
    {
        var orders = await App.Database.GetSupplierOrdersAsync();
        var incoming = orders.Where(o => o.Status == "Pending" && (o.ETA >= DateTime.Now || o.ETA == null)).ToList();

        if (incoming.Count == 0)
        {
            IncomingLabel.Text = "No incoming shipments.";
            return;
        }

        var sb = new StringBuilder();
        foreach (var o in incoming)
            sb.AppendLine($"{o.SupplierName} - ETA: {(o.ETA?.ToString("MMM dd") ?? "TBD")}");
        IncomingLabel.Text = sb.ToString();
    }

    private async Task LoadSuggestedRestock()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id); // ✅ filter by user
        var lowStock = products.Where(p => p.Stock <= p.MinStockLevel).ToList();

        if (lowStock.Count == 0)
        {
            RestockLabel.Text = "No restock needed.";
            return;
        }

        var sb = new StringBuilder();
        foreach (var p in lowStock)
        {
            int suggested = (p.MinStockLevel * 2) - p.Stock;
            if (suggested < 5) suggested = 5;
            sb.AppendLine($"• Order {suggested} {p.Name}");
        }
        RestockLabel.Text = sb.ToString();
    }

    // Navigation methods
    private async void OnInventoryClicked(object sender, EventArgs e) => await Navigation.PushAsync(new InventoryPage(_currentUser));
    private async void OnRestockClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SmartRestockPage(_currentUser));
    private async void OnSuppliersClicked(object sender, EventArgs e) => await Navigation.PushAsync(new SupplierPage(_currentUser));
    private async void OnUpgradeClicked(object sender, EventArgs e) => await Navigation.PushAsync(new PremiumPage(_currentUser));
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new UserProfilePage(_currentUser));

    private async void OnNotificationClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotificationPage(_currentUser));
    }
}