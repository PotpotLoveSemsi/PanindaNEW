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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrders();
    }

    private async Task LoadOrders()
    {
        var orders = await App.Supabase.GetSupplierOrdersAsync(_currentUser.Id);

        var displayOrders = new ObservableCollection<OrderHistoryDisplay>();
        int counter = 1;

        foreach (var o in orders.OrderByDescending(x => x.OrderDate))
        {
            decimal price = o.ConfirmedPrice ?? o.RequestedPrice ?? 0;

            displayOrders.Add(new OrderHistoryDisplay
            {
                OrderNumber = $"Order #{1000 + counter}",
                SupplierName = o.SupplierName,
                ItemsSummary = $"{o.ProductName} - {o.Quantity}",
                Status = o.Status,
                TotalCost = o.Quantity * price
            });

            counter++;
        }

        OrdersHistoryList.ItemsSource = displayOrders;
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}

public class OrderHistoryDisplay
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemsSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
}