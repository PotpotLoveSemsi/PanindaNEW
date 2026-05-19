using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class NotificationPage : ContentPage
{
    private User _currentUser;
    private List<SupplierOrder> _allPendingOrders = new();
    private List<Product> _lowStockProducts = new();
    private bool _showAllOrders = false;

    public NotificationPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotifications();
    }

    private async Task LoadNotifications()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id);
        var orders = await App.Database.GetSupplierOrdersAsync(_currentUser.Id);

        _allPendingOrders = orders
            .Where(o => o.Status == "Pending")
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        _lowStockProducts = products
            .Where(p => p.Stock <= p.MinStockLevel)
            .OrderBy(p => p.Stock)
            .ToList();

        ShowNotifications();
    }

    private void ShowNotifications()
    {
        var notifications = new ObservableCollection<NotificationItem>();

        var ordersToShow = _showAllOrders
            ? _allPendingOrders
            : _allPendingOrders.Take(3).ToList();

        foreach (var o in ordersToShow)
        {
            notifications.Add(new NotificationItem
            {
                Title = "📦 Pending Order",
                Message = $"{o.ProductName} from {o.SupplierName}\nDate: {o.OrderDate:MMM dd, yyyy}"
            });
        }

        foreach (var p in _lowStockProducts)
        {
            notifications.Add(new NotificationItem
            {
                Title = "⚠ Low Stock",
                Message = $"{p.Name} only {p.Stock} left"
            });
        }

        if (notifications.Count == 0)
        {
            notifications.Add(new NotificationItem
            {
                Title = "✨ All Good",
                Message = "No notifications."
            });
        }

        NotificationsList.ItemsSource = notifications;

        ViewMoreButton.IsVisible = _allPendingOrders.Count > 3;
        ViewMoreButton.Text = _showAllOrders ? "Show Less" : "View More";
    }

    private void OnViewMoreClicked(object sender, EventArgs e)
    {
        _showAllOrders = !_showAllOrders;
        ShowNotifications();
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}