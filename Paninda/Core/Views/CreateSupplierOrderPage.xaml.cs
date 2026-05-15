using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class CreateSupplierOrderPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<OrderItem> _orderItems = new();

    // New constructor that accepts preselected items
    public CreateSupplierOrderPage(User user, List<OrderItem> selectedItems)
    {
        InitializeComponent();
        _currentUser = user;
        _orderItems = new ObservableCollection<OrderItem>(selectedItems);
        OrderItemsList.ItemsSource = _orderItems;
        DeliveryDateLabel.Text = DateTime.Now.AddDays(3).ToString("MMMM dd");
    }

    // Old constructor for compatibility (if needed elsewhere)
    public CreateSupplierOrderPage(User user) : this(user, new List<OrderItem>()) { }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        var selectedSupplier = SupplierPicker.SelectedItem?.ToString() ?? "Unknown";
        foreach (var item in _orderItems)
        {
            var supplierOrder = new SupplierOrder
            {
                SupplierName = selectedSupplier,
                ProductName = item.Name,
                Quantity = item.Quantity,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ETA = DateTime.Now.AddDays(3),
                UserId = _currentUser.Id
            };
            await App.Database.SaveSupplierOrderAsync(supplierOrder);
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