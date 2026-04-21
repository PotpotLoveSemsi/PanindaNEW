using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class CreateSupplierOrderPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<OrderItem> _orderItems = new();

    public CreateSupplierOrderPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadOrderItems();
        DeliveryDateLabel.Text = DateTime.Now.AddDays(3).ToString("MMMM dd");
    }

    private async void LoadOrderItems()
    {
        var products = await App.Database.GetProductsAsync(_currentUser.Id); // ✅ pass userId
        var lowStock = products.Where(p => p.Stock <= p.MinStockLevel).ToList();
        _orderItems.Clear();
        foreach (var p in lowStock)
        {
            int suggested = (p.MinStockLevel * 2) - p.Stock;
            if (suggested < 5) suggested = 5;
            _orderItems.Add(new OrderItem
            {
                Name = p.Name,
                Price = p.Price,
                Quantity = suggested,
                Subtotal = p.Price * suggested
            });
        }
        OrderItemsList.ItemsSource = _orderItems;
    }

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
                ETA = DateTime.Now.AddDays(3)
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