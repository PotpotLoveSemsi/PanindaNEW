using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class OrdersPage : ContentPage
{
    private User _currentUser;
    private List<OrderDisplay> _allOrders = new();
    private bool _showingAll = false;

    public OrdersPage(User user)
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

        _allOrders = orders
            .OrderByDescending(o => o.OrderDate)
            .Select((o, index) => new OrderDisplay
            {
                OrderNumber = $"Order #{1000 + index + 1}",
                SupplierName = o.SupplierName,
                ItemsSummary = $"{o.ProductName} - {o.Quantity}",
                Status = o.Status,
                ETA = o.ETA?.ToString("MMMM dd") ?? "Pending",
                TotalCost = o.ConfirmedPrice ?? o.RequestedPrice ?? 0,
                RequestedPrice = o.RequestedPrice,
                ConfirmedPrice = o.ConfirmedPrice
            })
            .ToList();

        ShowLatestOnly();
    }

    private void ShowLatestOnly()
    {
        OrdersList.ItemsSource = null;
        OrdersList.ItemsSource = new ObservableCollection<OrderDisplay>(_allOrders.Take(1));

        ShowMoreButton.IsVisible = _allOrders.Count > 1;
        ShowMoreButton.Text = "SHOW MORE";
        _showingAll = false;
    }

    private void OnShowMoreClicked(object sender, EventArgs e)
    {
        OrdersList.ItemsSource = null;

        if (_showingAll)
        {
            ShowLatestOnly();
        }
        else
        {
            OrdersList.ItemsSource = new ObservableCollection<OrderDisplay>(_allOrders);
            ShowMoreButton.Text = "SHOW LESS";
            _showingAll = true;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Navigation.PopAsync();

    private async void OnDashboardClicked(object sender, EventArgs e) =>
        await Navigation.PopToRootAsync();

    private async void OnLogoClicked(object sender, EventArgs e) =>
        await Navigation.PopToRootAsync();

    private async void OnProfileClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new UserProfilePage(_currentUser));
}

public class OrderDisplay
{
    public string OrderNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string ItemsSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ETA { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal? RequestedPrice { get; set; }
    public decimal? ConfirmedPrice { get; set; }
}