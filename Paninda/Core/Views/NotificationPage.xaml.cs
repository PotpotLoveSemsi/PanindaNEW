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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotifications();
    }

    private async Task LoadNotifications()
    {
        var notifications = new ObservableCollection<NotificationItem>();

        var products = await App.Products.GetProductsAsync(_currentUser.Id);

        foreach (var p in products.Where(p => p.Stock <= p.MinStockLevel))
        {
            notifications.Add(new NotificationItem
            {
                Title = "⚠ Low Stock",
                Message = $"{p.Name} only {p.Stock} left"
            });
        }

        var orders = await App.Supabase.GetSupplierOrdersAsync(_currentUser.Id);

        foreach (var o in orders.Where(o => o.Status == "Pending"))
        {
            notifications.Add(new NotificationItem
            {
                Title = "📦 Shipment Update",
                Message = $"{o.SupplierName} ETA {o.ETA?.ToString("MMM dd") ?? "soon"}"
            });
        }

        foreach (var n in CreateSupplierOrderPage.AppNotifications)
            notifications.Add(n);

        if (notifications.Count == 0)
        {
            notifications.Add(new NotificationItem
            {
                Title = "✨ All Good",
                Message = "No notifications."
            });
        }

        NotificationsList.ItemsSource = notifications;
    }

    private async void OnBackClicked(object sender, EventArgs e) =>
        await Navigation.PopAsync();
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}