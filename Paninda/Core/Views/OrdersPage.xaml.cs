using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class OrdersPage : ContentPage
{
    private User _currentUser;

    public OrdersPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadOrders();
    }

    private async void LoadOrders()
    {
        var orders = await App.Database.GetSupplierOrdersAsync();
        var displayOrders = new ObservableCollection<OrderDisplay>();
        int counter = 1;
        foreach (var o in orders)
        {
            decimal total = o.Quantity * 20; // placeholder price
            displayOrders.Add(new OrderDisplay
            {
                OrderNumber = $"Order #{1000 + counter}",
                SupplierName = o.SupplierName,
                ItemsSummary = $"{o.ProductName} - {o.Quantity}",
                Status = o.Status,
                ETA = o.ETA?.ToString("MMMM dd") ?? "Pending",
                TotalCost = total
            });
            counter++;
        }
        OrdersList.ItemsSource = displayOrders;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnLogoClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
}

public class OrderDisplay
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemsSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ETA { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
}