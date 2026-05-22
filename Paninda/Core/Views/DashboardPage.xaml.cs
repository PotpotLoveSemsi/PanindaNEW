using Paninda.Models;
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiRectF = Microsoft.Maui.Graphics.RectF;

namespace Paninda.Views;

public partial class DashboardPage : ContentPage
{
    private User _currentUser;

    public DashboardPage(User user)
    {
        InitializeComponent();
        _currentUser = user;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var updatedUser = await App.Supabase.GetUserByEmailAsync(_currentUser.Email);
        if (updatedUser != null)
            _currentUser = updatedUser;

        await LoadDashboardData();
        UpdateProfilePicture();
        SetDashboardMode();
    }

    private void SetDashboardMode()
    {
        bool isPremium = _currentUser.IsPremium;

        NonPremiumDashboard.IsVisible = !isPremium;
        PremiumDashboard.IsVisible = isPremium;
        PremiumIcon.IsVisible = isPremium;
    }

    private void UpdateProfilePicture()
    {
        if (!string.IsNullOrWhiteSpace(_currentUser.ProfilePicturePath) &&
            Uri.TryCreate(_currentUser.ProfilePicturePath, UriKind.Absolute, out Uri? imageUri))
        {
            DashboardProfileImage.Source = ImageSource.FromUri(imageUri);
            DashboardProfileImage.IsVisible = true;
            DashboardProfileEmoji.IsVisible = false;
        }
        else
        {
            DashboardProfileImage.Source = null;
            DashboardProfileImage.IsVisible = false;
            DashboardProfileEmoji.IsVisible = true;
        }
    }

    private async Task LoadDashboardData()
    {
        var products = await App.Supabase.GetProductsAsync(_currentUser.Id);
        var orders = await App.Supabase.GetSupplierOrdersAsync(_currentUser.Id);
        var sales = await App.Supabase.GetSalesAsync(_currentUser.Id);

        LoadNonPremiumData(products, orders);
        LoadPremiumData(products, orders, sales);
    }

    private void LoadNonPremiumData(List<Product> products, List<SupplierOrder> orders)
    {
        var topSelling = products.OrderByDescending(p => p.SoldToday).Take(3).ToList();
        var lowStock = products.Where(p => p.Stock <= p.MinStockLevel).ToList();

        var incoming = orders
    .OrderByDescending(o => o.OrderDate)
    .Take(3)
    .ToList();

        TopSellingLabel.Text = topSelling.Any(p => p.SoldToday > 0)
            ? string.Join("\n", topSelling.Where(p => p.SoldToday > 0).Select(p => $"• {p.Name} – {p.SoldToday} sold"))
            : "No sales data yet.";

        LowStockLabel.Text = lowStock.Count == 0
            ? "No items running low."
            : string.Join("\n", lowStock.Select(p => $"• {p.Name} – {p.Stock} left"));

        IncomingLabel.Text = incoming.Count == 0
            ? "No incoming shipments."
            : string.Join("\n", incoming.Select(o => $"• {o.ProductName} from {o.SupplierName}"));

        RestockLabel.Text = lowStock.Count == 0
            ? "No restock needed."
            : string.Join("\n", lowStock.Select(p => $"• Order {Math.Max((p.MinStockLevel * 2) - p.Stock, 5)} {p.Name}"));
    }

    private void LoadPremiumData(List<Product> products, List<SupplierOrder> orders, List<Sale> sales)
    {
        int totalItems = products.Sum(p => p.Stock);
        int lowStockCount = products.Count(p => p.Stock <= p.MinStockLevel);
        int outOfStockCount = products.Count(p => p.Stock <= 0);

        var todaySales = sales.Where(s => s.DateSold.Date == DateTime.Today).ToList();

        int transactionsToday = todaySales.Sum(s => s.Quantity);
        decimal totalSales = todaySales.Sum(s => s.TotalPrice);
        decimal estimatedProfit = todaySales.Sum(s => s.Profit);

        var topItemName = todaySales
            .GroupBy(s => s.ProductName)
            .Select(g => new { ProductName = g.Key, Quantity = g.Sum(s => s.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .FirstOrDefault();

        PremiumTotalSalesLabel.Text = $"₱{totalSales:N0}";
        PremiumTransactionsLabel.Text = transactionsToday.ToString();
        PremiumProfitLabel.Text = $"₱{estimatedProfit:N0}";
        PremiumLowStockAlertsLabel.Text = lowStockCount.ToString();
        PremiumSalesOverviewLabel.Text = $"₱{totalSales:N0}";

        PremiumTopItemLabel.Text = topItemName == null
            ? "No sales yet"
            : $"{topItemName.ProductName}\n{topItemName.Quantity} sold today";

        PremiumLowStockLabel.Text = lowStockCount == 0
            ? "No low stock items"
            : $"{lowStockCount} items\nNeed restocking";

        PremiumRestockLabel.Text = lowStockCount == 0
            ? "No restock needed"
            : $"{lowStockCount} items\nView suggestions";

        PremiumTotalItemsLabel.Text = totalItems.ToString();
        PremiumInventoryLowLabel.Text = lowStockCount.ToString();
        PremiumOutOfStockLabel.Text = outOfStockCount.ToString();

        var latestActivities = todaySales
            .OrderByDescending(s => s.DateSold)
            .Take(3)
            .Select(s => $"• {s.ProductName} - {s.Quantity} sold")
            .ToList();

        PremiumActivityLabel.Text = latestActivities.Count == 0
            ? "No recent activity."
            : string.Join("\n", latestActivities);

        SalesChartView.Drawable = new SalesChartDrawable(todaySales);
        SalesChartView.Invalidate();
    }

    private async void OnInventoryClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new InventoryPage(_currentUser));

    private async void OnRestockClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new SmartRestockPage(_currentUser));

    private async void OnSuppliersClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new SupplierPage(_currentUser));

    private async void OnUpgradeClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new PremiumPage(_currentUser));

    private async void OnLogoClicked(object sender, EventArgs e) =>
        await Navigation.PopToRootAsync();

    private async void OnProfileClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new UserProfilePage(_currentUser));

    private async void OnNotificationClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new NotificationPage(_currentUser));

    private async void OnSalesHistoryClicked(object sender, EventArgs e) =>
        await Navigation.PushAsync(new SalesHistoryPage(_currentUser));
}

public class SalesChartDrawable : IDrawable
{
    private readonly List<Sale> _sales;

    public SalesChartDrawable(List<Sale> sales)
    {
        _sales = sales;
    }

    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        var values = new List<float>();

        for (int i = 0; i < 6; i++)
        {
            int startHour = i * 4;
            int endHour = startHour + 4;

            float total = (float)_sales
                .Where(s => s.DateSold.Hour >= startHour && s.DateSold.Hour < endHour)
                .Sum(s => s.TotalPrice);

            values.Add(total);
        }

        if (values.All(v => v <= 0))
            values = new List<float> { 0, 0, 0, 0, 0, 0 };

        float max = Math.Max(values.Max(), 1);
        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        canvas.StrokeColor = MauiColor.FromArgb("#F3D1E8");
        canvas.StrokeSize = 1;

        for (int i = 1; i <= 3; i++)
        {
            float y = height * i / 4;
            canvas.DrawLine(0, y, width, y);
        }

        canvas.StrokeColor = MauiColor.FromArgb("#EC4899");
        canvas.StrokeSize = 4;

        PathF path = new();

        for (int i = 0; i < values.Count; i++)
        {
            float x = i * (width / (values.Count - 1));
            float y = height - ((values[i] / max) * (height - 24)) - 12;

            if (i == 0)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);
        }

        canvas.DrawPath(path);
    }
}