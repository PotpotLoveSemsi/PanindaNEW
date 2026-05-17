using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class CreateSupplierOrderPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<OrderItem> _orderItems = new();
    private decimal _supplierQuotedPrice;

    public CreateSupplierOrderPage(User user, List<OrderItem> selectedItems)
    {
        InitializeComponent();
        _currentUser = user;
        _orderItems = new ObservableCollection<OrderItem>(selectedItems);
        OrderItemsList.ItemsSource = _orderItems;
        DeliveryDateLabel.Text = DateTime.Now.AddDays(3).ToString("MMMM dd");
    }

    public CreateSupplierOrderPage(User user) : this(user, new List<OrderItem>()) { }

    // Enable the Notify button when a supplier is selected
    private void OnSupplierSelected(object sender, EventArgs e)
    {
        var selectedSupplier = SupplierPicker.SelectedItem?.ToString();
        NotifyButton.IsEnabled = !string.IsNullOrWhiteSpace(selectedSupplier) && selectedSupplier != "Select Supplier";
    }

    private async void OnNotifyClicked(object sender, EventArgs e)
    {
        var selectedSupplier = SupplierPicker.SelectedItem?.ToString() ?? "Unknown";
        if (string.IsNullOrWhiteSpace(selectedSupplier) || selectedSupplier == "Select Supplier")
        {
            await DisplayAlert("Error", "Please select a supplier.", "OK");
            return;
        }

        decimal originalTotal = _orderItems.Sum(i => i.Subtotal);
        _supplierQuotedPrice = originalTotal * 1.15m;
        _supplierQuotedPrice = Math.Round(_supplierQuotedPrice, 2);

        foreach (var item in _orderItems)
        {
            var supplierOrder = new SupplierOrder
            {
                SupplierName = selectedSupplier,
                ProductName = item.Name,
                Quantity = item.Quantity,
                OrderDate = DateTime.Now,
                Status = "PriceRequested",
                ETA = DateTime.Now.AddDays(3),
                UserId = _currentUser.Id,
                RequestedPrice = _supplierQuotedPrice
            };
            await App.Database.SaveSupplierOrderAsync(supplierOrder);
        }

        await DisplayAlert("Notification Sent", $"Supplier has quoted ₱{_supplierQuotedPrice:N2}.", "OK");

        NotifyButton.IsVisible = false;
        PriceEntryContainer.IsVisible = true;
        SupplierPriceEntry.Text = _supplierQuotedPrice.ToString();
        SupplierPriceEntry.IsEnabled = false;
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SupplierPriceEntry.Text))
        {
            await DisplayAlert("Error", "No price available. Please notify supplier first.", "OK");
            return;
        }
        if (!decimal.TryParse(SupplierPriceEntry.Text, out decimal price))
        {
            await DisplayAlert("Error", "Invalid price format.", "OK");
            return;
        }

        var selectedSupplier = SupplierPicker.SelectedItem?.ToString() ?? "Unknown";
        var orders = await App.Database.GetSupplierOrdersAsync(_currentUser.Id);
        var pendingOrders = orders.Where(o => o.SupplierName == selectedSupplier && o.Status == "PriceRequested").ToList();
        foreach (var order in pendingOrders)
        {
            order.ConfirmedPrice = price;
            order.Status = "Confirmed";
            await App.Database.UpdateOrderAsync(order);
        }

        await DisplayAlert("Success", "Order Confirmed!", "OK");
        await Navigation.PushAsync(new OrdersPage(_currentUser));
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
}

public class OrderItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}