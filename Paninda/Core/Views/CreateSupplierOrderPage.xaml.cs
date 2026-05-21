using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class CreateSupplierOrderPage : ContentPage
{
    private readonly User _currentUser;
    private ObservableCollection<OrderItem> _orderItems = new();

    private decimal _supplierQuotedPrice;
    private DateTime _eta;
    private string _selectedSupplier = "";

    public static event Action? OnOrderCreated;
    public static List<NotificationItem> AppNotifications = new();

    public CreateSupplierOrderPage(User user, List<OrderItem> selectedItems)
    {
        InitializeComponent();

        _currentUser = user;
        _orderItems = new ObservableCollection<OrderItem>(selectedItems);

        OrderItemsList.ItemsSource = _orderItems;
    }

    public CreateSupplierOrderPage(User user) : this(user, new List<OrderItem>())
    {
    }

    private void OnSupplierSelected(object sender, EventArgs e)
    {
        NotifyButton.IsEnabled = SupplierPicker.SelectedItem != null;
    }

    private async void OnNotifyClicked(object sender, EventArgs e)
    {
        _selectedSupplier = SupplierPicker.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(_selectedSupplier))
        {
            await DisplayAlert("Error", "Please select a supplier.", "OK");
            return;
        }

        if (_orderItems.Count == 0)
        {
            await DisplayAlert("Error", "No items selected to order.", "OK");
            return;
        }

        NotifyButton.IsEnabled = false;
        NotifyButton.Text = "Processing...";

        int totalQuantity = _orderItems.Sum(i => i.Quantity);

        decimal basePrice = _selectedSupplier switch
        {
            "ABC Foods" => 48m,
            "XYZ Supply" => 52m,
            "YNS Snacks" => 35m,
            "TGS Hygienix" => 60m,
            _ => 45m
        };

        _supplierQuotedPrice = Math.Round(totalQuantity * basePrice * 1.10m, 2);
        _eta = DateTime.Now.AddDays(new Random().Next(2, 6));

        DeliveryDateLabel.Text = _eta.ToString("MMMM dd");
        GeneratedPriceLabel.Text = $"Price: ₱{_supplierQuotedPrice:N2}";
        GeneratedEtaLabel.Text = $"Delivery: {_eta:MMMM dd}";

        NotifyButton.IsVisible = false;
        PriceEntryContainer.IsVisible = true;

        AppNotifications.Add(new NotificationItem
        {
            Title = "📦 Supplier Response",
            Message = $"{_selectedSupplier} quoted ₱{_supplierQuotedPrice:N2}\nETA: {_eta:MMMM dd}"
        });

        await DisplayAlert("Done", "Supplier responded successfully.", "OK");
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (_orderItems.Count == 0)
        {
            await DisplayAlert("Error", "No items selected to order.", "OK");
            return;
        }

        foreach (var item in _orderItems)
        {
            var supplierOrder = new SupplierOrder
            {
                SupplierName = _selectedSupplier,
                ProductName = item.Name,
                Quantity = item.Quantity,
                OrderDate = DateTime.Now,
                Status = "Confirmed",
                ETA = _eta,
                UserId = _currentUser.Id,
                RequestedPrice = _supplierQuotedPrice,
                ConfirmedPrice = _supplierQuotedPrice
            };

            await App.Database.SaveSupplierOrderAsync(supplierOrder);
        }

        OnOrderCreated?.Invoke();

        await DisplayAlert("Success", "Order confirmed!", "OK");
        await Navigation.PushAsync(new OrdersPage(_currentUser));
    }

    private async void OnBackTapped(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private async void OnLogoClicked(object sender, EventArgs e)
        => await Navigation.PopToRootAsync();

    private async void OnProfileClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new UserProfilePage(_currentUser));
}