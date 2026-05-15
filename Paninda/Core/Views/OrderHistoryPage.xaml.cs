using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class OrderHistoryPage : ContentPage
{
    private User _currentUser;

    public OrderHistoryPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadOrders();
    }

    private async void LoadOrders()
    {
        // ✅ filter by current user ID
        var orders = await App.Database.GetSupplierOrdersAsync(_currentUser.Id);
        var displayOrders = new ObservableCollection<OrderDisplay>();
        int counter = 1;
        foreach (var o in orders)
        {
            displayOrders.Add(new OrderDisplay
            {
                OrderNumber = $"Order #{1000 + counter}",
                SupplierName = o.SupplierName,
                ItemsSummary = $"{o.ProductName} - {o.Quantity}",
                Status = o.Status,
                TotalCost = o.Quantity * 20
            });
            counter++;
        }
        OrdersHistoryList.ItemsSource = displayOrders;
    }

    private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();
}

// OrderDisplay already defined elsewhere, but if not, uncomment:
// public class OrderDisplay
// {
//     public string OrderNumber { get; set; } = string.Empty;
//     public string SupplierName { get; set; } = string.Empty;
//     public string ItemsSummary { get; set; } = string.Empty;
//     public string Status { get; set; } = string.Empty;
//     public decimal TotalCost { get; set; }
// }