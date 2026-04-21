using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class NotificationPage : ContentPage
{
    private User _currentUser;

    public NotificationPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadNotifications();
    }

    private async void LoadNotifications()
    {
        var notifications = new ObservableCollection<NotificationItem>();

        // 1. Low stock alerts (filter by user)
        var products = await App.Database.GetProductsAsync(_currentUser.Id); // ✅ pass userId
        var lowStockProducts = products.Where(p => p.Stock <= p.MinStockLevel).ToList();
        foreach (var p in lowStockProducts)
        {
            notifications.Add(new NotificationItem
            {
                Title = "⚠ Low Stock Alert",
                Message = $"{p.Name} only {p.Stock} left"
            });
        }

        // 2. Predictive Delay Alerts (based on ETA)
        var orders = await App.Database.GetSupplierOrdersAsync();
        var today = DateTime.Today;
        foreach (var o in orders.Where(o => o.Status == "Pending" && o.ETA.HasValue))
        {
            var eta = o.ETA.Value.Date;
            if (eta < today)
            {
                notifications.Add(new NotificationItem
                {
                    Title = "⚠ Overdue Shipment",
                    Message = $"Order from {o.SupplierName} was due on {eta:MMM dd}!"
                });
            }
            else if (eta == today)
            {
                notifications.Add(new NotificationItem
                {
                    Title = "📦 Arriving Today",
                    Message = $"Order from {o.SupplierName} is expected today!"
                });
            }
            else if (eta == today.AddDays(1))
            {
                notifications.Add(new NotificationItem
                {
                    Title = "📦 Arriving Tomorrow",
                    Message = $"Order from {o.SupplierName} will arrive tomorrow."
                });
            }
            else if (eta <= today.AddDays(3))
            {
                notifications.Add(new NotificationItem
                {
                    Title = "📦 Upcoming Shipment",
                    Message = $"Order from {o.SupplierName} arriving on {eta:MMM dd}."
                });
            }
        }

        // 3. Other incoming shipments (without duplicating the ones already added)
        var incomingOrders = orders.Where(o => o.Status == "Pending" && (o.ETA >= today || o.ETA == null)).ToList();
        foreach (var o in incomingOrders)
        {
            if (!notifications.Any(n => n.Message.Contains(o.SupplierName)))
            {
                notifications.Add(new NotificationItem
                {
                    Title = "📦 Shipment Update",
                    Message = $"Order from {o.SupplierName} arriving {(o.ETA?.ToString("MMM dd") ?? "soon")}"
                });
            }
        }

        // 4. If no notifications, show a friendly message
        if (notifications.Count == 0)
        {
            notifications.Add(new NotificationItem
            {
                Title = "✨ All Good",
                Message = "No low stock, delays, or pending shipments."
            });
        }

        NotificationsList.ItemsSource = notifications;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}