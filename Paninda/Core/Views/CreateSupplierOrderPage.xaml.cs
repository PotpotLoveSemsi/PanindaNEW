using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class CreateSupplierOrderPage : ContentPage
{
    private User _currentUser;
    private ObservableCollection<OrderItem> _orderItems = new();
    private decimal _supplierQuotedPrice;
    private DateTime _eta;

    public static event Action? OnOrderCreated;
    public static List<NotificationItem> AppNotifications = new();

    public CreateSupplierOrderPage(User user, List<OrderItem> selectedItems)
    {
        InitializeComponent();
        _currentUser = user;
        _orderItems = new ObservableCollection<OrderItem>(selectedItems);
        OrderItemsList.ItemsSource = _orderItems;
    }

    public CreateSupplierOrderPage(User user) : this(user, new List<OrderItem>()) { }

    private void OnSupplierSelected(object sender, EventArgs e)
    {
        NotifyButton.IsEnabled = SupplierPicker.SelectedItem != null;
    }

    private async void OnNotifyClicked(object sender, EventArgs e)
    {
        var selectedSupplier = SupplierPicker.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(selectedSupplier))
        {
            await DisplayAlert("Error", "Please select a supplier.", "OK");
            return;
        }

        NotifyButton.IsEnabled = false;
        NotifyButton.Text = "Processing...";

        await Task.Delay(1500);

        int totalQuantity = _orderItems.Sum(i => i.Quantity);

        decimal basePrice = selectedSupplier switch
        {
            "ABC Foods" => 48m,
            "XYZ Supply" => 52m,
            "YNS Snacks" => 35m,
            "TGS Hygienix" => 60m,
            _ => 45m
        };

        _supplierQuotedPrice = Math.Round(totalQuantity * basePrice * 1.10m, 2);
        _eta = DateTime.Now.AddDays(new Random().Next(2, 6));

        foreach (var item in _orderItems)
        {
            var supplierOrder = new SupplierOrder
            {
                SupplierName = selectedSupplier,
                ProductName = item.Name,
                Quantity = item.Quantity,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ETA = _eta,
                UserId = _currentUser.Id,
                RequestedPrice = _supplierQuotedPrice
            };

            await App.Supabase.SaveSupplierOrderAsync(supplierOrder);
        }

        AppNotifications.Add(new NotificationItem
        {
            Title = "📦 Supplier Response",
            Message = $"{selectedSupplier} quoted ₱{_supplierQuotedPrice:N2}\nETA: {_eta:MMMM dd}"
        });

        DeliveryDateLabel.Text = _eta.ToString("MMMM dd");
        GeneratedPriceLabel.Text = $"Price: ₱{_supplierQuotedPrice:N2}";
        GeneratedEtaLabel.Text = $"Delivery: {_eta:MMMM dd}";

        NotifyButton.IsVisible = false;
        PriceEntryContainer.IsVisible = true;

        await DisplayAlert("Done", "Supplier responded successfully.", "OK");
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        OnOrderCreated?.Invoke();

        await DisplayAlert("Success", "Order confirmed!", "OK");
        await Navigation.PushAsync(new OrdersPage(_currentUser));
    }

    private async void OnBackTapped(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnLogoClicked(object sender, EventArgs e) => await Navigation.PopToRootAsync();
    private async void OnProfileClicked(object sender, EventArgs e) => await Navigation.PushAsync(new UserProfilePage(_currentUser));
}