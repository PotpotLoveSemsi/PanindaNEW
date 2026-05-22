using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class NotificationPage : ContentPage
{
    private User _currentUser;
    private bool _showAllOrders = false;
    private List<SupplierOrder> _orders = new();

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
        var orders = await App.Supabase.GetSupplierOrdersAsync(_currentUser.Id);
        var products = await App.Supabase.GetProductsAsync(_currentUser.Id);

        _orders = orders.OrderByDescending(o => o.OrderDate).ToList();

        var notifications = new ObservableCollection<NotificationItem>();

        var ordersToShow = _showAllOrders ? _orders : _orders.Take(3).ToList();

        foreach (var o in ordersToShow)
        {
            notifications.Add(new NotificationItem
            {
                Title = "📦 Supplier Order",
                Message = $"{o.ProductName} from {o.SupplierName}\nStatus: {o.Status}\nQty: {o.Quantity}"
            });
        }

        foreach (var p in products.Where(p => p.Stock <= p.MinStockLevel))
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

        NotificationsList.ItemsSource = null;
        NotificationsList.ItemsSource = notifications;

        ViewMoreButton.IsVisible = _orders.Count > 3;
        ViewMoreButton.Text = _showAllOrders ? "Show Less" : "View More";
    }

    private async void OnViewMoreClicked(object sender, EventArgs e)
    {
        _showAllOrders = !_showAllOrders;
        await LoadNotifications();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

public class NotificationItem
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
}