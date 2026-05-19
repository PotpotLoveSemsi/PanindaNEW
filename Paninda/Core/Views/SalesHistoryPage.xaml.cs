using Paninda.Models;
using System.Collections.ObjectModel;

namespace Paninda.Views;

public partial class SalesHistoryPage : ContentPage
{
    private User _currentUser;
    private List<Sale> _allSales = new();

    public SalesHistoryPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSales();
        ShowToday();
    }

    private async Task LoadSales()
    {
        _allSales = await App.Sales.GetSalesAsync(_currentUser.Id);
    }

    private void ShowToday()
    {
        var filtered = _allSales
            .Where(s => s.DateSold.Date == DateTime.Today)
            .ToList();

        UpdateView(filtered, "Today’s Sales");
        SetActiveButton("Today");
    }

    private void ShowWeek()
    {
        var startDate = DateTime.Today.AddDays(-6);

        var filtered = _allSales
            .Where(s => s.DateSold.Date >= startDate)
            .ToList();

        UpdateView(filtered, "This Week’s Sales");
        SetActiveButton("Week");
    }

    private void ShowMonth()
    {
        var startDate = DateTime.Today.AddDays(-29);

        var filtered = _allSales
            .Where(s => s.DateSold.Date >= startDate)
            .ToList();

        UpdateView(filtered, "This Month’s Sales");
        SetActiveButton("Month");
    }

    private void UpdateView(List<Sale> sales, string title)
    {
        FilterTitleLabel.Text = title;
        TotalSalesLabel.Text = $"₱{sales.Sum(s => s.TotalPrice):N2}";

        SalesList.ItemsSource = new ObservableCollection<SaleHistoryViewModel>(
            sales.Select(s => new SaleHistoryViewModel(s))
        );
    }

    private void SetActiveButton(string active)
    {
        TodayButton.BackgroundColor = active == "Today" ? Color.FromArgb("#6D28D9") : Colors.White;
        TodayButton.TextColor = active == "Today" ? Colors.White : Color.FromArgb("#6D28D9");

        WeekButton.BackgroundColor = active == "Week" ? Color.FromArgb("#6D28D9") : Colors.White;
        WeekButton.TextColor = active == "Week" ? Colors.White : Color.FromArgb("#6D28D9");

        MonthButton.BackgroundColor = active == "Month" ? Color.FromArgb("#6D28D9") : Colors.White;
        MonthButton.TextColor = active == "Month" ? Colors.White : Color.FromArgb("#6D28D9");
    }

    private void OnTodayClicked(object sender, EventArgs e) => ShowToday();
    private void OnWeekClicked(object sender, EventArgs e) => ShowWeek();
    private void OnMonthClicked(object sender, EventArgs e) => ShowMonth();

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

public class SaleHistoryViewModel
{
    public string ProductName { get; set; }
    public string QuantityText { get; set; }
    public string TotalText { get; set; }
    public string DateText { get; set; }

    public SaleHistoryViewModel(Sale sale)
    {
        ProductName = sale.ProductName;
        QuantityText = $"Quantity: {sale.Quantity}";
        TotalText = $"Total: ₱{sale.TotalPrice:N2}";
        DateText = sale.DateSold.ToString("MMM dd, yyyy hh:mm tt");
    }
}